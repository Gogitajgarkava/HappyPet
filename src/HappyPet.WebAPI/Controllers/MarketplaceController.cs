using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/marketplace")]
    [ApiController]
    [Authorize]
    public class MarketplaceController : ControllerBase
    {
        private readonly IMarketplaceService _marketplaceService;

        public MarketplaceController(IMarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
        }

        [HttpPost("listings")]
        public async Task<IActionResult> CreateListing(
            [FromBody] CreateMarketplaceListingRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _marketplaceService.CreateAsync(CurrentUserId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(GetListing), new { id = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpGet("listings")]
        public async Task<IActionResult> GetListings(
            [FromQuery] PetSpecies? species = null,
            [FromQuery] ListingType? listingType = null,
            [FromQuery] string? city = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (minPrice.HasValue && minPrice.Value < 0)
            {
                return BadRequestError("VALIDATION_ERROR", "მინიმალური ფასი უარყოფითი არ შეიძლება იყოს.");
            }

            if (maxPrice.HasValue && maxPrice.Value < 0)
            {
                return BadRequestError("VALIDATION_ERROR", "მაქსიმალური ფასი უარყოფითი არ შეიძლება იყოს.");
            }

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                return BadRequestError("VALIDATION_ERROR", "მინიმალური ფასი მაქსიმალურზე მეტი არ შეიძლება იყოს.");
            }

            var criteria = new MarketplaceSearchCriteria
            {
                Species = species,
                ListingType = listingType,
                City = city,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Search = search
            };

            var result = await _marketplaceService.GetActiveAsync(
                criteria,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("listings/{id:int}")]
        public async Task<IActionResult> GetListing(int id, CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null || !IsPubliclyVisible(listing.Status, listing.ExpiresAt))
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            return Ok(listing);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyListings(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _marketplaceService.GetBySellerAsync(
                CurrentUserId.Value,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [HttpPut("listings/{id:int}")]
        public async Task<IActionResult> UpdateListing(
            int id,
            [FromBody] UpdateMarketplaceListingRequest request,
            CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            var result = await _marketplaceService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("listings/{id:int}")]
        public async Task<IActionResult> WithdrawListing(int id, CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            await _marketplaceService.WithdrawAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("listings/{id:int}/complete")]
        public async Task<IActionResult> CompleteListing(
            int id,
            [FromBody] CompleteMarketplaceListingRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            var result = await _marketplaceService.CompleteAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("listings/{id:int}/block")]
        public async Task<IActionResult> BlockListing(
            int id,
            [FromBody] BlockMarketplaceListingRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _marketplaceService.BlockAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("listings/{id:int}/unblock")]
        public async Task<IActionResult> UnblockListing(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _marketplaceService.UnblockAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("listings/{id:int}/photos")]
        public async Task<IActionResult> GetListingPhotos(int id, CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null || !IsPubliclyVisible(listing.Status, listing.ExpiresAt))
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            var photos = await _marketplaceService.GetPhotosAsync(id, cancellationToken);
            return Ok(photos);
        }

        [HttpPost("listings/{id:int}/photos")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddListingPhoto(
            int id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequestError("VALIDATION_ERROR", "სურათი არ არის არჩეული.");
            }

            await using var stream = file.OpenReadStream();
            var result = await _marketplaceService.AddPhotoAsync(
                id,
                stream,
                file.Length,
                file.ContentType,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("listings/{id:int}/photos/{photoId:int}/primary")]
        public async Task<IActionResult> SetPrimaryListingPhoto(
            int id,
            int photoId,
            CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            var result = await _marketplaceService.SetPrimaryPhotoAsync(id, photoId, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("listings/{id:int}/photos/{photoId:int}")]
        public async Task<IActionResult> DeleteListingPhoto(
            int id,
            int photoId,
            CancellationToken cancellationToken)
        {
            var listing = await _marketplaceService.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            if (!CanManage(listing.SellerId))
            {
                return Forbid();
            }

            await _marketplaceService.DeletePhotoAsync(id, photoId, cancellationToken);
            return NoContent();
        }

        private static bool IsPubliclyVisible(ListingStatus status, DateTime expiresAt)
        {
            return status switch
            {
                ListingStatus.Active => expiresAt > DateTime.UtcNow,
                ListingStatus.Completed => true,
                _ => false
            };
        }

        private NotFoundObjectResult NotFoundError(string message)
        {
            return NotFound(new ApiErrorResponse
            {
                Code = "NOT_FOUND",
                Message = message,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        private BadRequestObjectResult BadRequestError(string code, string message)
        {
            return BadRequest(new ApiErrorResponse
            {
                Code = code,
                Message = message,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        private int? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        private bool CanManage(int sellerId)
        {
            return User.IsInRole(nameof(UserRole.Admin)) || CurrentUserId == sellerId;
        }
    }
}
