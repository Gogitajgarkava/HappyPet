using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Services;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
            return Ok(result.Response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(Error(
                    "REFRESH_TOKEN_MISSING",
                    "სესია ვადაგასულია. გთხოვთ ხელახლა შეხვიდეთ."));
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
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    Error("EMAIL_NOT_VERIFIED", ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(Error("REFRESH_TOKEN_INVALID", ex.Message));
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
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
        {
            await _userService.VerifyEmailAsync(request, cancellationToken);
            return Ok(new { message = "ელ-ფოსტა წარმატებით დადასტურდა." });
        }

        [HttpPost("resend-verification")]
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
        {
            await _userService.ResendVerificationAsync(request, cancellationToken);
            return Ok(new { message = "თუ ელ-ფოსტა რეგისტრირებულია და არ არის დადასტურებული, კოდი გაიგზავნა." });
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _userService.ForgotPasswordAsync(request, cancellationToken);
            return Ok(new { message = "თუ მითითებული ელ-ფოსტა რეგისტრირებულია, აღდგენის კოდი გაიგზავნა." });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("auth-otp")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            await _userService.ResetPasswordAsync(request, cancellationToken);
            return Ok(new { message = "პაროლი წარმატებით განახლდა." });
        }

        private ApiErrorResponse Error(string code, string message)
        {
            return new ApiErrorResponse
            {
                Code = code,
                Message = message,
                TraceId = HttpContext.TraceIdentifier
            };
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
