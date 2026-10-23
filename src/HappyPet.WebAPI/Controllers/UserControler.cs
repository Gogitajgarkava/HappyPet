using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyPet.WebAPI.Controllers
{
    [Route("api/v1/user")]
    [ApiController]
    public class UserControler : ControllerBase
    {
        private readonly IUserServices _userService;

        public UserControler(IUserServices userServices)
        {
            _userService = userServices;
        }

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

            return Ok(user);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userService.UpdateAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _userService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
