using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto request);
        Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<Result<bool>> RequestResetAsync(ForgetPasswordDto forgetDTO);
        Task<Result<ResetPasswordResultDto?>> VerifyOtpAsync(VerifyOtpDto optDto);
        Task<Result<bool>> ResendEmailOtpAsync(ResendEmailDto dto);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetDto);
        Task<Result<bool>> ResendResetOtpAsync(string email);
        Task<Result<VerifyEmailResponseDto>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto);
        (string RedirectUrl, AuthenticationProperties Props) GetGoogleRedirectUrl(string? returnUrl = null);
        Task<Result<AuthResponseDto>> HandleGoogleCallbackAsync
            (string? returnUrl = null, string? remoteError = null);
        Task<Result<AuthResponseDto>> GoogleLoginAsync(string idToken);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken);
    }
}