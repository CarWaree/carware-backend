using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
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

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));
            return Ok(ApiResponseGeneric<AuthDto>.Success(result.Data, "Registration successful"));
        }

        //[Authorize]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));
            return Ok(ApiResponseGeneric<AuthDto>.Success(result.Data, "Login successful"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody]ForgetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(ApiResponse.Fail("Email is required."));

            var result =  await _authService.RequestResetAsync(dto);

            return Ok(ApiResponse.Success("Check your email for the verification code"));
        }

        [HttpPost("Verify-Otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody]VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);
            if (result == null)
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