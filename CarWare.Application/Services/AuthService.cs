using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.Common.Cache;
using CarWare.Application.Common.helper;
using CarWare.Application.Common.Helpers;
using CarWare.Application.Common.Security;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOtpGenerator _otpGenerator;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly IRefreshTokenGenerator _refreshToken;
        private readonly IJwtTokenGenerator _jwtToken;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int OtpValidityMinutes = 3;
        private const int MaxOtpAttempts = 5;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOtpGenerator otpGenerator,
            IEmailSender emailSender,
            IRefreshTokenGenerator refreshToken,
            IJwtTokenGenerator jwtToken,
            IDistributedCache cache,
            IConfiguration config,
            IMapper mapper,
            ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _otpGenerator = otpGenerator;
            _cache = cache;
            _config = config;
            _emailSender = emailSender;
            _refreshToken = refreshToken;
            _jwtToken = jwtToken;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // ─── Register
        public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto model)
        {
            _logger.LogInformation("Register attempt for {Email}", model.Email);

            if (await _userManager.FindByNameAsync(model.Username) != null)
                return Result<RegisterResponseDto>.Fail("Username is already taken");

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                _logger.LogWarning("Register failed - email exists {Email}", model.Email);
                return Result<RegisterResponseDto>.Fail("Email already registered");
            }

            var user = _mapper.Map<ApplicationUser>(model);
            user.EmailConfirmed = false;

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Register failed for {Email}: {Errors}", model.Email, errors);
                return Result<RegisterResponseDto>.Fail(errors);
            }

            await _userManager.AddToRoleAsync(user, "User");

            await SendEmailOtpAsync(user);

            var response = _mapper.Map<RegisterResponseDto>(user);
            response.IsEmailVerified = false;

            return Result<RegisterResponseDto>.Ok(response);
        }

        // ─── Verify Email OTP 
        public async Task<Result<VerifyEmailResponseDto>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.EmailConfirmed)
                return Result<VerifyEmailResponseDto>.Fail("Invalid request", "InvalidRequest");

            var attemptsKey = CacheKeys.EmailVerifyOtpAttempts(user.Id);
            var attempts = await GetAttemptsAsync(attemptsKey);

            if (attempts >= MaxOtpAttempts)
                return Result<VerifyEmailResponseDto>.Fail("OTP locked", "OtpLocked");

            var cachedOtp = await _cache.GetStringAsync(CacheKeys.EmailVerifyOtp(user.Id));

            if (cachedOtp != dto.Otp)
            {
                await OtpAttemptManager.IncrementAsync(_cache, attemptsKey, attempts, OtpValidityMinutes);
                return Result<VerifyEmailResponseDto>.Fail("Invalid OTP", "InvalidOtp");
            }

            await _cache.RemoveAsync(CacheKeys.EmailVerifyOtp(user.Id));
            await _cache.RemoveAsync(attemptsKey);

            user.EmailConfirmed = true;

            var refreshToken = _refreshToken.Generate();
            user.RefreshTokens.Add(refreshToken);

            await _userManager.UpdateAsync(user);

            var jwt = await _jwtToken.CreateToken(user);

            return Result<VerifyEmailResponseDto>.Ok(new VerifyEmailResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refreshToken.Token
            });
        }

        // ─── Resend Email OTP 
        public async Task<Result<bool>> ResendEmailOtpAsync(ResendEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Result<bool>.Fail("User not found", "NotFound");

            if (user.EmailConfirmed)
                return Result<bool>.Fail("Email already verified", "AlreadyVerified");

            await SendEmailOtpAsync(user);

            return Result<bool>.Ok(true);
        }

        // ─── Login 
        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

            if (user == null)
                return Result<LoginResponseDto>.Fail("Invalid credentials", "InvalidLogin");

            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user, loginDto.Password, true);

            if (signInResult.IsLockedOut)
                return Result<LoginResponseDto>.Fail("Account locked", "LockedOut");

            if (!signInResult.Succeeded)
                return Result<LoginResponseDto>.Fail("Invalid credentials", "InvalidLogin");

            if (!user.EmailConfirmed)
                return Result<LoginResponseDto>.Fail("Please verify email", "EmailNotVerified");

            var roles = await _userManager.GetRolesAsync(user);

            var refreshToken = _refreshToken.Generate();
            user.RefreshTokens.Add(refreshToken);

            await _userManager.UpdateAsync(user);

            var jwt = await _jwtToken.CreateToken(user);

            return Result<LoginResponseDto>.Ok(new LoginResponseDto
            {
                IsAuthenticated = true,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn
            });
        }

        // ─── Refresh Token 
        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == dto.RefreshToken));

            if (user == null)
                return Result<AuthResponseDto>.Fail("Invalid token", "InvalidToken");

            var token = user.RefreshTokens.FirstOrDefault(t => t.Token == dto.RefreshToken);

            if (token == null || token.RevokedOn != null || token.ExpiresOn <= DateTime.UtcNow)
                return Result<AuthResponseDto>.Fail("Expired token", "ExpiredToken");

            token.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = _refreshToken.Generate();
            user.RefreshTokens.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            var jwt = await _jwtToken.CreateToken(user);

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiration = jwt.ValidTo,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn
            });
        }

        // ─── Revoke Refresh Token 
        public async Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
                return Result<bool>.Fail("Invalid token", "InvalidToken");

            var token = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
                return Result<bool>.Fail("Already revoked", "AlreadyRevoked");

            token.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result<bool>.Ok(true);
        }

        // ─── Forget Password 
        public async Task<Result<bool>> RequestResetAsync(ForgetPasswordDto forgetDTO)
        {
            _logger.LogInformation("Password reset requested for {Email}", forgetDTO.Email);

            var user = await _userManager.FindByEmailAsync(forgetDTO.Email);

            // Return Ok regardless to prevent email enumeration
            if (user == null) return Result<bool>.Ok(true);

            await SendResetOtpAsync(user, forgetDTO.Email);

            return Result<bool>.Ok(true);
        }

        // Resend OTP
        public async Task<Result<bool>> ResendResetOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Result<bool>.Ok(true);

            var cooldownKey = $"reset_otp_cooldown:{user.Id}";
            var exists = await _cache.GetStringAsync(cooldownKey);

            if (exists != null)
                return Result<bool>.Fail("Try again after 60 seconds");

            await _cache.SetStringAsync(
                cooldownKey,
                "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });

            await SendResetOtpAsync(user, email);

            return Result<bool>.Ok(true);
        }

        // ─── Verify Reset OTP 
        public async Task<Result<ResetPasswordResultDto>> VerifyOtpAsync(VerifyOtpDto optDto)
        {
            var user = await _userManager.FindByEmailAsync(optDto.Email);

            if (user == null)
                return Result<ResetPasswordResultDto>.Fail("Invalid OTP", "InvalidOtp");

            var attemptsKey = CacheKeys.ResetOtpAttempts(user.Id);
            var attempts = await GetAttemptsAsync(attemptsKey);

            if (attempts >= MaxOtpAttempts)
                return Result<ResetPasswordResultDto>.Fail("OTP locked due to too many attempts", "OtpLocked");

            var cachedOtp = await _cache.GetStringAsync(CacheKeys.ResetOtp(user.Id));

            if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != optDto.Otp)
            {
                await OtpAttemptManager.IncrementAsync(_cache, attemptsKey, attempts, OtpValidityMinutes);
                return Result<ResetPasswordResultDto>.Fail("Invalid or expired OTP", "InvalidOtp");
            }

            await _cache.RemoveAsync(CacheKeys.ResetOtp(user.Id));
            await _cache.RemoveAsync(attemptsKey);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var handle = Guid.NewGuid().ToString("N");

            await _cache.SetStringAsync(CacheKeys.ResetHandle(handle), resetToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            await _cache.SetStringAsync(CacheKeys.ResetHandleUserId(handle), user.Id,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return Result<ResetPasswordResultDto>.Ok(new ResetPasswordResultDto
            {
                ResetPasswordToken = handle
            });
        }

        // ─── Reset Password
        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            if (resetDto.NewPassword != resetDto.ConfirmPassword)
                return Result<bool>.Fail("Passwords do not match", "PasswordMismatch");

            if (string.IsNullOrWhiteSpace(resetDto.ResetPasswordToken))
                return Result<bool>.Fail("Token is required", "MissingToken");

            var realResetToken = await _cache.GetStringAsync(CacheKeys.ResetHandle(resetDto.ResetPasswordToken));
            var userId = await _cache.GetStringAsync(CacheKeys.ResetHandleUserId(resetDto.ResetPasswordToken));

            if (realResetToken == null || userId == null)
                return Result<bool>.Fail("Reset token expired or invalid", "InvalidToken");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<bool>.Fail("User not found", "NotFound");

            var result = await _userManager.ResetPasswordAsync(user, realResetToken, resetDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Fail(errors, "ResetFailed");
            }

            await _cache.RemoveAsync(CacheKeys.ResetHandle(resetDto.ResetPasswordToken));
            await _cache.RemoveAsync(CacheKeys.ResetHandleUserId(resetDto.ResetPasswordToken));

            return Result<bool>.Ok(true);
        }

        // ─── Change Password
        public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto changeDto)
        {
            // Get Current User
            var userId = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<bool>.Fail("User not found");

            // Confirm password validation
            if (changeDto.NewPassword != changeDto.ConfirmPassword)
                return Result<bool>.Fail("Passwords do not match");

            // Prevent same password
            if (changeDto.OldPassword == changeDto.NewPassword)
                return Result<bool>.Fail("New password cannot be same as old password");

            // Change password using Identity
            var result = await _userManager.ChangePasswordAsync(
                user,
                changeDto.OldPassword,
                changeDto.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();

                return Result<bool>.Fail(string.Join(" | ", errors));
            }

            return Result<bool>.Ok(true);
        }

        // ─── Google OAuth (Callback)
        public (string RedirectUrl, AuthenticationProperties Props) GetGoogleRedirectUrl(string? returnUrl = null)
        {
            var baseUrl = _config["ExternalAuth:Google:CallbackBaseUrl"];
            var callbackPath = _config["ExternalAuth:Google:CallbackPath"];
            var redirectUrl = $"{baseUrl}{callbackPath}?returnUrl={returnUrl}";
            var props = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return (redirectUrl, props);
        }
        public async Task<Result<AuthResponseDto>> HandleGoogleCallbackAsync (string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return Result<AuthResponseDto>.Fail(remoteError, "GoogleAuthError");

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return Result<AuthResponseDto>.Fail("Error loading external login info", "GoogleAuthError");

            var signInResult = await _signInManager
                .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            ApplicationUser user;

            if (!signInResult.Succeeded)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = email,
                        UserName = UsernameGenerator.Generate(email),
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                }

                await _userManager.AddLoginAsync(user, info);
            }
            else
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }

            var refreshToken = _refreshToken.Generate();
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var jwt = await _jwtToken.CreateToken(user);

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = refreshToken.Token,
                IsProfileCompleted = user.IsProfileCompleted
            });
        }

        // ─── Google Login (Mobile / SPA) 
        public async Task<Result<AuthResponseDto>> GoogleLoginAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = payload.Email,
                        UserName = UsernameGenerator.Generate(payload.Email),
                        EmailConfirmed = true,
                        FirstName = payload.Name?.Split(' ')[0] ?? "",
                        LastName = payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        GoogleId = payload.Subject
                    };

                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                }

                var refreshToken = _refreshToken.Generate();
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                var jwt = await _jwtToken.CreateToken(user);

                return Result<AuthResponseDto>.Ok(new AuthResponseDto
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                    RefreshToken = refreshToken.Token,
                    IsProfileCompleted = user.IsProfileCompleted
                });
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDto>.Fail(ex.Message, "GoogleAuthError");
            }
        }

        #region Helpers
        // ─── Private Helpers 
        private async Task SendEmailOtpAsync(ApplicationUser user)
        {
            var otp = _otpGenerator.Generate();

            await _cache.SetStringAsync(
                CacheKeys.EmailVerifyOtp(user.Id),
                otp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
                });

            await _cache.RemoveAsync(CacheKeys.EmailVerifyOtpAttempts(user.Id));

            await _emailSender.SendEmailAsync(
                user.Email,
                "Email Verification Code",
                $"Your verification code is <b>{otp}</b>. It expires in {OtpValidityMinutes} minutes."
            );
        }

        private async Task SendResetOtpAsync(ApplicationUser user, string email)
        {
            var otp = _otpGenerator.Generate();

            await _cache.SetStringAsync(
                CacheKeys.ResetOtp(user.Id),
                otp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
                });

            await _emailSender.SendEmailAsync(
                email,
                "Reset Password OTP",
                $"Your OTP is <b>{otp}</b>. It expires in {OtpValidityMinutes} minutes."
            );
        }

        private async Task<int> GetAttemptsAsync(string attemptsKey)
        {
            var attemptsStr = await _cache.GetStringAsync(attemptsKey);
            return string.IsNullOrEmpty(attemptsStr) ? 0 : int.Parse(attemptsStr);
        }
        #endregion
    }
}