using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthDto>> RegisterAsync(RegisterDto request);
        Task<Result<AuthDto>> LoginAsync(LoginDto loginDto);
        Task<Result<bool>> RequestResetAsync(ForgetPasswordDto forgetDTO);
        Task<Result<ResetPasswordResultDto?>> VerifyOtpAsync(VerifyOtpDto optDto);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetDto);
        Task<Result<bool>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto);

        IActionResult GoogleLogin(string? returnUrl = null);
        Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null);
    }
}