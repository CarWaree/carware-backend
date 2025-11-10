using CarWare.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDto> RegisterAsync(RegisterDto request);
        Task<AuthDto> LoginAsync(LoginDto loginDto);
        Task<bool> RequestResetAsync(ForgetPasswordDto forgetDTO);
        Task<ResetPasswordResultDto?> VerifyOtpAsync(VerifyOtpDto optDto);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetDto);
        Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null);
        Task<IActionResult> Logout();
    }
}