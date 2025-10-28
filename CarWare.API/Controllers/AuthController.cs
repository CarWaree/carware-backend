using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("login")]
        [Authorize]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody]ForgetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { success = false, message = "Email is required." });

            var result =  await _authService.RequestResetAsync(dto);

            return Ok(new
            {
                success = true,
                message = "If your email is registered, you will receive a verification code shortly."
            });
        }

        [HttpPost("Verify-Otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody]VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);
            return result == null ? BadRequest("Invalid or expired OTP") : Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return result.Succeeded ? Ok("Password reset successfully"): BadRequest(result.Errors);
        }

        [HttpGet("login-google")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
                return BadRequest("Google authentication failed.");

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest("Email not found in Google profile.");

            var tokenResult = await _authService.CreateJwtToken(email, name);

            if (!tokenResult.IsAuthenticated)
                return BadRequest(tokenResult.Message);

            return Ok(new
            {
                message = "Google login successful",
                token = tokenResult.Token,
                email = tokenResult.Email,
                username = tokenResult.Username
            });
        }
    }
}
