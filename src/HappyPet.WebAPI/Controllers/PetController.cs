using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/pet")]
    [ApiController]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;

        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        [HttpPost("add-pet")]
        public async Task<IActionResult> CreatePet([FromBody] CreatePetRequest dto, CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _petService.CreateAsync(CurrentUserId.Value, dto, cancellationToken);
            return CreatedAtAction(nameof(GetPetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPetById(int id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            return Ok(pet);
        }

        [HttpGet("owner/{ownerId:int}")]
        public async Task<IActionResult> GetPetsByOwner(
            int ownerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (!CanAccess(ownerId))
            {
                return Forbid();
            }

            var pets = await _petService.GetByOwnerIdAsync(ownerId, page, pageSize, cancellationToken);
            return Ok(pets);
        }

        [AllowAnonymous]
        [HttpGet("lost")]
        public async Task<IActionResult> GetLostPets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] double? latitude = null,
            [FromQuery] double? longitude = null,
            [FromQuery] double? radiusKm = null,
            CancellationToken cancellationToken = default)
        {
            var pets = await _petService.GetLostAsync(
                page,
                pageSize,
                latitude,
                longitude,
                radiusKm,
                cancellationToken);
            return Ok(pets);
        }

        [HttpPost("{id:int}/lost")]
        public async Task<IActionResult> ReportLostPet(
            int id,
            [FromBody] ReportLostPetRequest request,
            CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var result = await _petService.ReportLostAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/found")]
        public async Task<IActionResult> ReportFoundPet(int id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var result = await _petService.ReportFoundAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}/photos")]
        public async Task<IActionResult> GetPetPhotos(int id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var photos = await _petService.GetPhotosAsync(id, cancellationToken);
            return Ok(photos);
        }

        [HttpPost("{id:int}/photos")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPetPhoto(int id, IFormFile file, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequestError("VALIDATION_ERROR", "სურათი არ არის არჩეული.");
            }

            await using var stream = file.OpenReadStream();
            var result = await _petService.AddPhotoAsync(
                id,
                stream,
                file.Length,
                file.ContentType,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:int}/photos/{photoId:int}/primary")]
        public async Task<IActionResult> SetPrimaryPetPhoto(int id, int photoId, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var result = await _petService.SetPrimaryPhotoAsync(id, photoId, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:int}/photos/{photoId:int}")]
        public async Task<IActionResult> DeletePetPhoto(int id, int photoId, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            await _petService.DeletePhotoAsync(id, photoId, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] UpdatePetRequest request, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var result = await _petService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePet(int id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFoundError("ცხოველი ვერ მოიძებნა.");
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            await _petService.DeleteAsync(id, cancellationToken);
            return NoContent();
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

        private bool CanAccess(int ownerId)
        {
            return User.IsInRole(nameof(UserRole.Admin)) || CurrentUserId == ownerId;
        }
    }
}
