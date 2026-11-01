using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyPet.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "happypet_refresh";
        private const string RefreshTokenCookiePath = "/api/v1/auth";

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
                var result = await _authService.LoginAsync(request, cancellationToken);
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
                return Ok(result.Response);
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(new
                {
                    code = "REFRESH_TOKEN_MISSING",
                    message = "სესია ვადაგასულია. გთხოვთ ხელახლა შეხვიდეთ."
                });
            }

            try
            {
                var result = await _authService.RefreshAsync(refreshToken, cancellationToken);
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
                return Ok(result.Response);
            }
            catch (EmailNotVerifiedException ex)
            {
                DeleteRefreshTokenCookie();
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    code = "EMAIL_NOT_VERIFIED",
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(new
                {
                    code = "REFRESH_TOKEN_INVALID",
                    message = ex.Message
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            await _authService.LogoutAsync(refreshToken ?? string.Empty, cancellationToken);
            DeleteRefreshTokenCookie();
            return Ok(new { message = "სესია წარმატებით დაიხურა." });
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
        {
            if (CurrentUserId == null)
            {
                return Unauthorized();
            }

            await _authService.LogoutAllAsync(CurrentUserId.Value, cancellationToken);
            DeleteRefreshTokenCookie();
            return Ok(new { message = "ყველა სესია დაიხურა." });
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

        private void SetRefreshTokenCookie(string token, DateTime expiresAt)
        {
            Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = RefreshTokenCookiePath,
                Expires = new DateTimeOffset(expiresAt.ToUniversalTime()),
                IsEssential = true
            });
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Append(RefreshTokenCookieName, string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = RefreshTokenCookiePath,
                Expires = DateTimeOffset.UnixEpoch,
                IsEssential = true
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
