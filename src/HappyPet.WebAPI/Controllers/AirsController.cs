using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/airs")]
    [ApiController]
    [Authorize]
    public class AirsController : ControllerBase
    {
        private readonly IAirsService _airsService;

        public AirsController(IAirsService airsService)
        {
            _airsService = airsService;
        }

        [HttpGet("animals/by-microchip/{microchip}")]
        public async Task<IActionResult> GetAnimalByMicrochip(
            string microchip,
            CancellationToken cancellationToken)
        {
            try
            {
                var details = await _airsService.GetAnimalByMicrochipAsync(
                    microchip,
                    cancellationToken);

                return Ok(details);
            }
            catch (AirsAnimalNotFoundException ex)
            {
                return NotFound(new { code = ex.Code, message = ex.Message });
            }
            catch (AirsUnavailableException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = ex.Code,
                    message = ex.Message
                });
            }
        }

        [HttpGet("vaccinations/{microchip}")]
        public async Task<IActionResult> GetVaccinations(
            string microchip,
            CancellationToken cancellationToken)
        {
            try
            {
                var details = await _airsService.GetAnimalByMicrochipAsync(
                    microchip,
                    cancellationToken);

                return Ok(details.Vaccinations);
            }
            catch (AirsAnimalNotFoundException ex)
            {
                return NotFound(new { code = ex.Code, message = ex.Message });
            }
            catch (AirsUnavailableException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    code = ex.Code,
                    message = ex.Message
                });
            }
        }
    }
}
