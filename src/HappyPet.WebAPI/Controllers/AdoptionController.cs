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
    [Route("api/v1/adoptions")]
    [ApiController]
    public class AdoptionController : ControllerBase
    {
        private readonly IAdoptionService _adoptionService;

        public AdoptionController(IAdoptionService adoptionService)
        {
            _adoptionService = adoptionService;
        }

        [AllowAnonymous]
        [HttpGet("listings")]
        public async Task<IActionResult> GetListings(
            [FromQuery] PetSpecies? species = null,
            [FromQuery] string? city = null,
            [FromQuery] string? search = null,
            [FromQuery] int? shelterId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var criteria = new AdoptionSearchCriteria
            {
                Species = species,
                City = city,
                Search = search,
                ShelterId = shelterId
            };

            var result = await _adoptionService.GetActiveListingsAsync(
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
            var result = await _adoptionService.GetPublicListingAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFoundError("განცხადება ვერ მოიძებნა.");
            }

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("listings")]
        public async Task<IActionResult> CreateListing(
            [FromBody] CreateAdoptionListingRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.CreateListingAsync(
                CurrentUserId.Value,
                request,
                cancellationToken);

            return CreatedAtAction(nameof(GetListing), new { id = result.Id }, result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpGet("listings/my")]
        public async Task<IActionResult> GetMyListings(
            [FromQuery] AdoptionListingStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.GetShelterListingsAsync(
                CurrentUserId.Value,
                status,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPut("listings/{id:int}")]
        public async Task<IActionResult> UpdateListing(
            int id,
            [FromBody] UpdateAdoptionListingRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.UpdateListingAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("listings/{id:int}/withdraw")]
        public async Task<IActionResult> WithdrawListing(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.WithdrawListingAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("listings/{id:int}/pause")]
        public async Task<IActionResult> PauseListing(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.PauseListingAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("listings/{id:int}/resume")]
        public async Task<IActionResult> ResumeListing(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.ResumeListingAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("listings/{id:int}/applications")]
        public async Task<IActionResult> Apply(
            int id,
            [FromBody] CreateAdoptionApplicationRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.ApplyAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize]
        [HttpGet("applications/my")]
        public async Task<IActionResult> GetMyApplications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.GetMyApplicationsAsync(
                CurrentUserId.Value,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("applications/{id:int}")]
        public async Task<IActionResult> GetApplication(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var application = await _adoptionService.GetApplicationAsync(id, cancellationToken);
            if (application == null)
            {
                return NotFoundError("განაცხადი ვერ მოიძებნა.");
            }

            if (!User.IsInRole(nameof(UserRole.Admin))
                && application.ApplicantUserId != CurrentUserId.Value
                && application.ShelterId != CurrentUserId.Value)
            {
                return Forbid();
            }

            return Ok(application);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpGet("listings/{id:int}/applications")]
        public async Task<IActionResult> GetListingApplications(
            int id,
            [FromQuery] AdoptionApplicationStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.GetListingApplicationsAsync(
                id,
                CurrentUserId.Value,
                status,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("applications/{id:int}/approve")]
        public async Task<IActionResult> ApproveApplication(
            int id,
            [FromBody] ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.ApproveApplicationAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("applications/{id:int}/reject")]
        public async Task<IActionResult> RejectApplication(
            int id,
            [FromBody] ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.RejectApplicationAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPost("applications/{id:int}/complete")]
        public async Task<IActionResult> CompleteApplication(
            int id,
            [FromBody] ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.CompleteApplicationAsync(
                id,
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("applications/{id:int}/withdraw")]
        public async Task<IActionResult> WithdrawApplication(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _adoptionService.WithdrawApplicationAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
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

        private int? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }
    }
}
