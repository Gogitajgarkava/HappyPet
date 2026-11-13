using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IOperatorService
    {
        Task<PagedResult<OperatorTransferResponse>> GetTransfersAsync(
            OwnershipTransferStatus? status,
            string? search,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<OperatorTransferResponse> GetTransferAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<OperatorTransferResponse> MarkSyncedAsync(
            int id,
            int operatorUserId,
            MarkOwnershipTransferSyncedRequest request,
            CancellationToken cancellationToken = default);

        Task<OperatorTransferResponse> RejectAsync(
            int id,
            int operatorUserId,
            RejectOwnershipTransferRequest request,
            CancellationToken cancellationToken = default);

        Task<OperatorDashboardResponse> GetDashboardAsync(
            CancellationToken cancellationToken = default);
    }
}
