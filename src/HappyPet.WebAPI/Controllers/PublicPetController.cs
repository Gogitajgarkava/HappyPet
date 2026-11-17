using HappyPet.Application.Interfaces.Services;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HappyPet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/public/pets")]
    [AllowAnonymous]
    public class PublicPetController : ControllerBase
    {
        private readonly IPetService _petService;

        public PublicPetController(IPetService petService)
        {
            _petService = petService;
        }

        [HttpGet("{token}")]
        [EnableRateLimiting("public-profile")]
        public async Task<IActionResult> GetPublicProfile(string token, CancellationToken cancellationToken)
        {
            var profile = await _petService.GetPublicProfileAsync(token, cancellationToken);
            if (profile == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "საჯარო პროფილი ვერ მოიძებნა.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            Response.Headers.CacheControl = "no-store";
            return Ok(profile);
        }
    }
}
