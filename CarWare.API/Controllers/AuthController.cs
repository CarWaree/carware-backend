using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(IAuthService authService, SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            return Ok(result);
        }

        //[Authorize]
        [HttpPost("login")]
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
                message = "Check your email for the verification code"
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

        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            return await _authService.ExternalLoginCallback(returnUrl, remoteError);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            return await _authService.Logout();
        }
    }
}