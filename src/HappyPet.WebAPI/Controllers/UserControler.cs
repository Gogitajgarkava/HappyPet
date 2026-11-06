using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Enums;
using HappyPet.WebAPI.Models;
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
            var result = await _userService.RegisterAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFoundError("მომხმარებელი ვერ მოიძებნა.");
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
                return NotFoundError("მომხმარებელი ვერ მოიძებნა.");
            }

            if (!CanAccess(user.Id))
            {
                return Forbid();
            }

            return Ok(user);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var existing = await _userService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFoundError("მომხმარებელი არ მოიძებნა.");
            }

            if (!CanAccess(id))
            {
                return Forbid();
            }

            var result = await _userService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var existing = await _userService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFoundError("მომხმარებელი არ მოიძებნა.");
            }

            if (!CanAccess(id))
            {
                return Forbid();
            }

            await _userService.DeleteAsync(id, cancellationToken);
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

        private int? CurrentUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        private bool CanAccess(int userId)
        {
            return User.IsInRole(nameof(UserRole.Admin)) || CurrentUserId == userId;
        }
    }
}
