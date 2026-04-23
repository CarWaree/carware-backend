using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _authService = authService;
            _signInManager = signInManager;
            _config = config;
        }

        private void SetRefreshTokenCookie(string token, DateTime? expires = null)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires.HasValue
                    ? new DateTimeOffset(expires.Value)
                    : DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));
            return Ok(ApiResponseGeneric<RegisterResponseDto>
                .Success(result.Data, "Registration successful. Please verify your email"));
        }

        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpDto dto)
        {
            var result = await _authService.VerifyEmailOtpAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            SetRefreshTokenCookie(result.Data.RefreshToken);

            return Ok(ApiResponseGeneric<VerifyEmailResponseDto>.Success(
                    data: result.Data,
                    message: "OTP verified successfully"
            ));
        }

        [HttpPost("resend-email-otp")]
        public async Task<IActionResult> ResendEmailOtpAsync([FromBody] ResendEmailDto dto)
        {
            var result = await _authService.ResendEmailOtpAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return Ok(ApiResponse.Success("A new verification code has been sent to your email."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            SetRefreshTokenCookie(result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

            return Ok(ApiResponseGeneric<LoginResponseDto>.Success(result.Data, "Login successful"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(ApiResponse.Fail("Email is required."));

            var result = await _authService.RequestResetAsync(dto);

            return Ok(ApiResponse.Success("Check your email for the verification code"));
        }

        [HttpPost("Verify-Otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse.Fail("Invalid or expired OTP"));

            return Ok(ApiResponseGeneric<ResetPasswordResultDto>.Success(
                    data: result.Data!,
                    message: "OTP verified successfully"
            ));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));
            return Ok(ApiResponse.Success("Password reset successfully"));
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var (redirectUrl, props) = _authService.GetGoogleRedirectUrl(returnUrl);

            return new ChallengeResult("Google", props);
        }

        [HttpPost("google-mobile")]
        public async Task<IActionResult> GoogleMobileLogin([FromBody] GoogleLoginDto model)
        {
            var result = await _authService.GoogleLoginAsync(model.IdToken);

            return Ok(result);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string? remoteError = null)
        {
            try
            {
                var result = await _authService.HandleGoogleCallbackAsync(remoteError);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiResponse.Fail("Missing refresh token"));

            var dto = new RefreshTokenRequestDto
            {
                RefreshToken = refreshToken
            };

            var result = await _authService.RefreshTokenAsync(dto);

            if (!result.Success)
                return Unauthorized(ApiResponse.Fail(result.Error));

            SetRefreshTokenCookie(result.Data.RefreshToken);

            return Ok(ApiResponseGeneric<AuthResponseDto>.Success(
                result.Data,
                "Token refreshed successfully"
            ));
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(ApiResponse.Fail("Refresh token is missing"));

            var result = await _authService.RevokeRefreshTokenAsync(refreshToken);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            // Delete cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(ApiResponse.Success("Logged out successfully"));
        }
    }
}