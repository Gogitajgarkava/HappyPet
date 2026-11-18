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
    [Route("api/v1/shelters")]
    [ApiController]
    [Authorize]
    public class ShelterController : ControllerBase
    {
        private readonly IShelterService _shelterService;

        public ShelterController(IShelterService shelterService)
        {
            _shelterService = shelterService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetShelters(
            [FromQuery] string? search = null,
            [FromQuery] string? city = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var criteria = new ShelterSearchCriteria
            {
                Search = search,
                City = city
            };

            var result = await _shelterService.GetPublicListAsync(
                criteria,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetShelter(int id, CancellationToken cancellationToken)
        {
            var result = await _shelterService.GetPublicProfileAsync(id, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _shelterService.GetMyProfileAsync(CurrentUserId.Value, cancellationToken);
            if (result == null)
            {
                return NotFoundError("თავშესაფრის პროფილი ჯერ არ არის შევსებული.");
            }

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpPut("me")]
        public async Task<IActionResult> UpsertMyProfile(
            [FromBody] UpdateShelterProfileRequest request,
            CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _shelterService.UpsertProfileAsync(
                CurrentUserId.Value,
                request,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Shelter))]
        [HttpGet("me/dashboard")]
        public async Task<IActionResult> GetMyDashboard(CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _shelterService.GetDashboardAsync(CurrentUserId.Value, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("admin/dashboard")]
        public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
        {
            var result = await _shelterService.GetAdminDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("admin/list")]
        public async Task<IActionResult> GetAdminShelters(
            [FromQuery] string? search = null,
            [FromQuery] string? city = null,
            [FromQuery] bool? verified = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var criteria = new ShelterSearchCriteria
            {
                Search = search,
                City = city
            };

            var result = await _shelterService.GetAdminListAsync(
                criteria,
                verified,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("admin/users/{userId:int}/grant-shelter")]
        public async Task<IActionResult> GrantShelterRole(int userId, CancellationToken cancellationToken)
        {
            await _shelterService.GrantShelterRoleAsync(userId, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("{id:int}/verify")]
        public async Task<IActionResult> Verify(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _shelterService.VerifyAsync(
                id,
                CurrentUserId.Value,
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("{id:int}/unverify")]
        public async Task<IActionResult> Unverify(int id, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _shelterService.UnverifyAsync(
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
