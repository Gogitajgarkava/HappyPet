using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Services
{
    public class OperatorService : IOperatorService
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        private readonly IMarketplaceRepository _marketplaceRepository;
        private readonly IPetRepository _petRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OperatorService(
            IMarketplaceRepository marketplaceRepository,
            IPetRepository petRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _marketplaceRepository = marketplaceRepository;
            _petRepository = petRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<OperatorTransferResponse>> GetTransfersAsync(
            OwnershipTransferStatus? status,
            string? search,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _marketplaceRepository.GetOwnershipTransfersPagedAsync(
                status,
                search,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<OperatorTransferResponse>
            {
                Items = items.Select(MapTransfer).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<OperatorTransferResponse> GetTransferAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var transfer = await _marketplaceRepository.GetOwnershipTransferByIdAsync(id, cancellationToken);
            if (transfer == null)
            {
                throw new KeyNotFoundException("მფლობელობის გადაცემა ვერ მოიძებნა.");
            }

            return MapTransfer(transfer);
        }

        public async Task<OperatorTransferResponse> MarkSyncedAsync(
            int id,
            int operatorUserId,
            MarkOwnershipTransferSyncedRequest request,
            CancellationToken cancellationToken = default)
        {
            var transfer = await GetPendingTransferAsync(id, cancellationToken);
            var now = DateTime.UtcNow;
            var syncedAt = request.SyncedAt.HasValue ? ToUtc(request.SyncedAt.Value) : now;

            if (syncedAt > now.AddMinutes(1))
            {
                throw new ArgumentException("AIRS-ში სინქრონიზაციის დრო მომავალში არ შეიძლება იყოს.");
            }

            if (syncedAt < transfer.TransferredAt)
            {
                throw new ArgumentException("AIRS-ში სინქრონიზაციის დრო გადაცემის დროზე ადრე არ შეიძლება იყოს.");
            }

            var buyerUserId = transfer.BuyerUserId;
            if (request.BuyerUserId.HasValue)
            {
                if (buyerUserId.HasValue && buyerUserId.Value != request.BuyerUserId.Value)
                {
                    throw new InvalidOperationException("გადაცემას უკვე აქვს სხვა მყიდველის ანგარიში მიბმული.");
                }

                buyerUserId = request.BuyerUserId.Value;
            }

            User? buyer = null;

            if (!buyerUserId.HasValue && !string.IsNullOrWhiteSpace(transfer.BuyerPersonalId))
            {
                buyer = await _userRepository.GetByPersonalIdAsync(
                    transfer.BuyerPersonalId,
                    cancellationToken);

                if (buyer != null)
                {
                    buyerUserId = buyer.Id;
                }
            }

            if (buyerUserId.HasValue)
            {
                if (buyerUserId.Value == transfer.SellerId)
                {
                    throw new InvalidOperationException("გამყიდველი არ შეიძლება იყოს მყიდველი.");
                }

                buyer ??= await _userRepository.GetByIdAsync(buyerUserId.Value, cancellationToken);
                if (buyer == null)
                {
                    throw new KeyNotFoundException("მყიდველის ანგარიში ვერ მოიძებნა.");
                }
            }

            if (transfer.PetId.HasValue)
            {
                var pet = await _petRepository.GetByIdAsync(transfer.PetId.Value, cancellationToken);
                if (pet == null)
                {
                    throw new InvalidOperationException("დაკავშირებული ცხოველი ვერ მოიძებნა.");
                }

                if (pet.IsDeceased)
                {
                    throw new InvalidOperationException("გარდაცვლილი ცხოველის მფლობელობის გადაცემა შეუძლებელია.");
                }

                if (pet.OwnerId != transfer.SellerId)
                {
                    throw new InvalidOperationException(
                        "ცხოველის მფლობელი შეიცვალა და AIRS-ის სინქრონიზაცია შეუძლებელია.");
                }

                if (!buyerUserId.HasValue)
                {
                    throw new InvalidOperationException(
                        "დაკავშირებული ცხოველის გადასაცემად მყიდველის HappyPet ანგარიში აუცილებელია. "
                        + "მიუთითე BuyerUserId ან დაამატე მომხმარებელი პირადი ნომრით.");
                }

                pet.OwnerId = buyerUserId.Value;
                pet.UpdatedAt = now;
                _petRepository.Update(pet);
            }

            transfer.BuyerUserId = buyerUserId;
            transfer.BuyerPhone ??= buyer?.PhoneNumber;
            transfer.BuyerAddress = TrimOrNull(request.BuyerAddress) ?? transfer.BuyerAddress ?? buyer?.Address;
            transfer.Status = OwnershipTransferStatus.Synced;
            transfer.AirsSyncedAt = syncedAt;
            transfer.AirsReference = request.AirsReference.Trim();
            transfer.ProcessedByUserId = operatorUserId;
            transfer.ProcessedAt = now;
            transfer.OperatorNotes = TrimOrNull(request.Notes);
            transfer.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapTransfer(transfer);
        }

        public async Task<OperatorTransferResponse> RejectAsync(
            int id,
            int operatorUserId,
            RejectOwnershipTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            var transfer = await GetPendingTransferAsync(id, cancellationToken);
            var now = DateTime.UtcNow;

            transfer.Status = OwnershipTransferStatus.Rejected;
            transfer.RejectionReason = request.Reason.Trim();
            transfer.ProcessedByUserId = operatorUserId;
            transfer.ProcessedAt = now;
            transfer.OperatorNotes = TrimOrNull(request.Notes);
            transfer.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapTransfer(transfer);
        }

        public async Task<OperatorDashboardResponse> GetDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var stats = await _marketplaceRepository.GetOwnershipTransferStatsAsync(cancellationToken);

            return new OperatorDashboardResponse
            {
                PendingTransfers = stats.PendingCount,
                SyncedTransfers = stats.SyncedCount,
                RejectedTransfers = stats.RejectedCount,
                TotalTransfers = stats.TotalCount
            };
        }

        private async Task<OwnershipTransfer> GetPendingTransferAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var transfer = await _marketplaceRepository.GetOwnershipTransferByIdAsync(id, cancellationToken);
            if (transfer == null)
            {
                throw new KeyNotFoundException("მფლობელობის გადაცემა ვერ მოიძებნა.");
            }

            if (transfer.Status != OwnershipTransferStatus.PendingAirsSync)
            {
                throw new InvalidOperationException("მფლობელობის გადაცემა უკვე დამუშავებულია.");
            }

            return transfer;
        }

        private static OperatorTransferResponse MapTransfer(OwnershipTransfer transfer)
        {
            var listing = transfer.Listing;
            var seller = listing?.Seller;

            return new OperatorTransferResponse
            {
                Id = transfer.Id,
                ListingId = transfer.ListingId,
                PetId = transfer.PetId,
                MicrochipNumber = transfer.MicrochipNumber,
                PetName = listing?.PetName ?? string.Empty,
                Species = listing?.Species ?? PetSpecies.Other,
                ListingType = listing?.ListingType ?? ListingType.Sale,
                Price = transfer.Price,
                Currency = "GEL",
                ListingCompletedAt = listing?.CompletedAt,
                SellerId = transfer.SellerId,
                SellerFullName = seller == null
                    ? string.Empty
                    : $"{seller.FirstName} {seller.LastName}".Trim(),
                BuyerUserId = transfer.BuyerUserId,
                BuyerFullName = transfer.BuyerFullName,
                BuyerPersonalId = transfer.BuyerPersonalId,
                BuyerPhone = transfer.BuyerPhone,
                BuyerAddress = transfer.BuyerAddress,
                TransferType = transfer.TransferType,
                Status = transfer.Status,
                TransferredAt = transfer.TransferredAt,
                AirsSyncedAt = transfer.AirsSyncedAt,
                AirsReference = transfer.AirsReference,
                ProcessedByUserId = transfer.ProcessedByUserId,
                ProcessedAt = transfer.ProcessedAt,
                RejectionReason = transfer.RejectionReason,
                OperatorNotes = transfer.OperatorNotes,
                Notes = transfer.Notes,
                CreatedAt = transfer.CreatedAt
            };
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            var validPage = page < 1 ? 1 : page;
            var validPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
            return (validPage, validPageSize);
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
