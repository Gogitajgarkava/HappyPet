using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IMarketplaceService _marketplaceService;

        public AdminController(
            IUserServices userServices,
            IMarketplaceService marketplaceService)
        {
            _userServices = userServices;
            _marketplaceService = marketplaceService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] UserRole? role = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _userServices.GetPagedAsync(
                search,
                role,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("marketplace/listings")]
        public async Task<IActionResult> GetMarketplaceListings(
            [FromQuery] ListingStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _marketplaceService.GetAdminAsync(
                status,
                search,
                page,
                pageSize,
                cancellationToken);

            return Ok(result);
        }
    }
}
