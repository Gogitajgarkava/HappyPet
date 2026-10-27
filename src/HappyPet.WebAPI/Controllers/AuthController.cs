using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyPet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserServices _userService;

        public AuthController(IAuthService authService, IUserServices userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _authService.LoginAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (EmailNotVerifiedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    code = "EMAIL_NOT_VERIFIED",
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _userService.VerifyEmailAsync(request, cancellationToken);
                return Ok(new { message = "ელ-ფოსტა წარმატებით დადასტურდა." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
        {
            await _userService.ResendVerificationAsync(request, cancellationToken);
            return Ok(new { message = "თუ ელ-ფოსტა რეგისტრირებულია და არ არის დადასტურებული, კოდი გაიგზავნა." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _userService.ForgotPasswordAsync(request, cancellationToken);
            return Ok(new { message = "თუ მითითებული ელ-ფოსტა რეგისტრირებულია, აღდგენის კოდი გაიგზავნა." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _userService.ResetPasswordAsync(request, cancellationToken);
                return Ok(new { message = "პაროლი წარმატებით განახლდა." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
