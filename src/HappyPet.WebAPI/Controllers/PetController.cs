using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
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

            try
            {
                var result = await _petService.CreateAsync(CurrentUserId.Value, dto, cancellationToken);
                return CreatedAtAction(nameof(GetPetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "დაფიქსირდა შეცდომა ცხოველის დამატებისას.", details = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetById(Guid id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            return Ok(pet);
        }

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<IActionResult> GetPetsByOwner(Guid ownerId, CancellationToken cancellationToken)
        {
            if (!CanAccess(ownerId))
            {
                return Forbid();
            }

            var pets = await _petService.GetByOwnerIdAsync(ownerId, cancellationToken);
            return Ok(pets);
        }

        [HttpGet("lost")]
        public async Task<IActionResult> GetLostPets(CancellationToken cancellationToken)
        {
            var pets = await _petService.GetLostAsync(cancellationToken);
            return Ok(pets);
        }

        [HttpGet("{id:guid}/photos")]
        public async Task<IActionResult> GetPetPhotos(Guid id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            var photos = await _petService.GetPhotosAsync(id, cancellationToken);
            return Ok(photos);
        }

        [HttpPost("{id:guid}/photos")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPetPhoto(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "სურათი არ არის არჩეული." });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _petService.AddPhotoAsync(
                    id,
                    stream,
                    file.Length,
                    file.ContentType,
                    cancellationToken);

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "სურათის ატვირთვა ვერ მოხერხდა.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}/photos/{photoId:guid}/primary")]
        public async Task<IActionResult> SetPrimaryPetPhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            try
            {
                var result = await _petService.SetPrimaryPhotoAsync(id, photoId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "მთავარი სურათის შეცვლა ვერ მოხერხდა.", details = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/photos/{photoId:guid}")]
        public async Task<IActionResult> DeletePetPhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            try
            {
                await _petService.DeletePhotoAsync(id, photoId, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "სურათის წაშლა ვერ მოხერხდა.", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromBody] UpdatePetRequest request, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            try
            {
                var result = await _petService.UpdateAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "დაფიქსირდა შეცდომა ცხოველის განახლებისას.", details = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePet(Guid id, CancellationToken cancellationToken)
        {
            var pet = await _petService.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                return NotFound(new { message = "ცხოველი ვერ მოიძებნა." });
            }

            if (!CanAccess(pet.OwnerId))
            {
                return Forbid();
            }

            try
            {
                await _petService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "დაფიქსირდა შეცდომა ცხოველის წაშლისას.", details = ex.Message });
            }
        }

        private Guid? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        private bool CanAccess(Guid ownerId)
        {
            return User.IsInRole(nameof(UserRole.Admin)) || CurrentUserId == ownerId;
        }
    }
}
