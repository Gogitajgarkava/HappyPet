using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/user")]
    [ApiController]
    [Authorize]
    public class UserControler : ControllerBase
    {
        private readonly IUserServices _userService;

        public UserControler(IUserServices userServices)
        {
            _userService = userServices;
        }

        [AllowAnonymous]
        [HttpPost("add-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userService.RegisterAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "დაფიქსირდა შეცდომა რეგისტრაციისას.", details = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "მომხმარებელი ვერ მოიძებნა." });
            }

            if (!CanAccess(id))
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "მომხმარებელი ვერ მოიძებნა." });
            }

            if (!CanAccess(user.Id))
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var existing = await _userService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound(new { message = "მომხმარებელი არ მოიძებნა." });
            }

            if (!CanAccess(id))
            {
                return Forbid();
            }

            try
            {
                var result = await _userService.UpdateAsync(id, request, cancellationToken);
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
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var existing = await _userService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound(new { message = "მომხმარებელი არ მოიძებნა." });
            }

            if (!CanAccess(id))
            {
                return Forbid();
            }

            try
            {
                await _userService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        private bool CanAccess(Guid userId)
        {
            return User.IsInRole(nameof(UserRole.Admin)) || CurrentUserId == userId;
        }
    }
}
