using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using CarWare.Domain.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private const int OtpValidityMinutes = 3;
        private const int MaxOtpAttempts = 5;
        private readonly JWT _jwt;

        private readonly IMapper _mapper;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JWT> jwt,
            IEmailSender emailSender,
            IDistributedCache cache,
            IConfiguration config,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt.Value;
            _cache = cache;
            _config = config;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        private string GetCacheKey(string email) => $"OTP_{email.ToUpperInvariant()}";

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {

            #region Claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            }
            .Union(userClaims)
            .Union(roleClaims);
            #endregion

            #region Signing Credentials
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            #endregion

            #region Design Token 
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);
            #endregion

            //var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return jwtSecurityToken;
        }

        public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByNameAsync(model.Username) != null)
                return Result<RegisterResponseDto>.Fail("Username is already taken");

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return Result<RegisterResponseDto>.Fail("Email is already registered");

            var user = _mapper.Map<ApplicationUser>(model);
            user.EmailConfirmed = false;

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Result<RegisterResponseDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "User");

            // Generate OTP
            var otp = OtpGenerator.Generate();

            // Cache keys (
            var otpKey = $"email_verify_otp:{otp}";
            var attemptsKey = $"email_verify_otp_attempts:{user.Id}";

            // Store OTP → Email
            await _cache.SetStringAsync(
                otpKey,
                user.Id,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
                });

            // reset attempts if re-register
            await _cache.RemoveAsync(attemptsKey);

            // Send OTP via email
            await _emailSender.SendEmailAsync(
                user.Email,
                "Email Verification Code",
                $"Your verification code is <b>{otp}</b>. It expires in {OtpValidityMinutes} minutes."
            );

            var RegisterResponse = _mapper.Map<RegisterResponseDto>(user);
            RegisterResponse.IsEmailVerified = false;

            return Result<RegisterResponseDto>.Ok(RegisterResponse);
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Result<LoginResponseDto>.Fail("Invalid Username or Password");

            if (!user.EmailConfirmed)
                return Result<LoginResponseDto>.Fail("Please verify your email before logging in.");


            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            var authDto = new LoginResponseDto
            {
                IsAuthenticated = true,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = rolesList.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo
            };

            return Result<LoginResponseDto>.Ok(authDto);
        }

        public async Task<Result<bool>> RequestResetAsync(ForgetPasswordDto forgetDTO)
        {
            var email = forgetDTO.Email.Trim().ToUpper();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return Result<bool>.Ok(true);

            // Generate OTP
            var otpBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(otpBytes);

            var otpCode = (BitConverter.ToUInt32(otpBytes, 0) % 900000 + 100000).ToString();

            var cacheKey = $"otp:{otpCode}";

            await _cache.SetStringAsync(cacheKey, email,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
                });

            // Send email
            await _emailSender.SendEmailAsync(
                forgetDTO.Email,
                "Password Reset Code",
                $"Your verification code is <b>{otpCode}</b>. It expires in 3 minutes."
            );

            return Result<bool>.Ok(true);
        }

        public async Task<Result<ResetPasswordResultDto>> VerifyOtpAsync(VerifyOtpDto optDto)
        {
            var userId = await _cache.GetStringAsync($"otp:{optDto.Otp}");
            if (string.IsNullOrEmpty(userId))
                return Result<ResetPasswordResultDto>.Fail("Invalid or expired OTP");

            var attemptsKey = $"otp_attempts:{userId}";
            var attemptsStr = await _cache.GetStringAsync(attemptsKey);
            var attempts = string.IsNullOrEmpty(attemptsStr) ? 0 : int.Parse(attemptsStr);

            if (attempts >= MaxOtpAttempts)
            {
                await _cache.RemoveAsync($"otp:{userId}");
                await _cache.RemoveAsync(attemptsKey);
                return Result<ResetPasswordResultDto>.Fail("OTP has been locked due to too many attempts");
            }

            //Get user by Id
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await IncrementOtpAttempts.IncrementOtpAttemptsAsync(_cache, attemptsKey, attempts, OtpValidityMinutes);
                return Result<ResetPasswordResultDto>.Fail("User not found");
            }

            //cleanup
            await _cache.RemoveAsync($"otp:{optDto.Otp}");
            await _cache.RemoveAsync(attemptsKey);

            // Generate Identity reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create a short handle (GUID)
            var handle = Guid.NewGuid().ToString("N");

            // Store mapping: handle -> actual resetToken (consider encrypting if you want)
            await _cache.SetStringAsync($"reset_handle:{handle}", resetToken,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

            // Store also handle -> email if you want quick lookup or extra validation
            await _cache.SetStringAsync($"reset_handle_email:{handle}", userId,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

            // Return handle to client (short and safe)
            return Result<ResetPasswordResultDto>.Ok(new ResetPasswordResultDto { Token = handle });
        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            if (resetDto.NewPassword != resetDto.ConfirmPassword)
                return Result<bool>.Fail("New Password and Confirmation do not match");

            // The client sends the handle (not the real Identity token)
            var handle = resetDto.Token;
            if (string.IsNullOrWhiteSpace(handle))
                return Result<bool>.Fail("Token is required.");

            // Retrieve the real reset token from cache
            var realResetToken = await _cache.GetStringAsync($"reset_handle:{handle}");
            var email = await _cache.GetStringAsync($"reset_handle_email:{handle}");
            if (realResetToken == null || email == null)
                return Result<bool>.Fail("The password reset token is invalid or has expired.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<bool>.Fail("User not found.");

            // Use the real Identity token
            var result = await _userManager.ResetPasswordAsync(user, realResetToken, resetDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Fail(errors);
            }

            // remove from cache (invalidate handle)
            await _cache.RemoveAsync($"reset_handle:{handle}");
            await _cache.RemoveAsync($"reset_handle_email:{handle}");

            return Result<bool>.Ok(true);
        }

        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var baseUrl = _config["ExternalAuth:Google:CallbackBaseUrl"];
            var callbackPath = _config["ExternalAuth:Google:CallbackPath"];

            var redirectUrl = $"{baseUrl}{callbackPath}?returnUrl={returnUrl}";
            var props = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return new ChallengeResult("Google", props);
        }

        public async Task<IActionResult> GoogleCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return new BadRequestObjectResult(new { error = remoteError });

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return new BadRequestObjectResult(new { error = "Error loading external login info" });

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

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
                        UserName = email,
                    };

                    await _userManager.CreateAsync(user);
                }

                await _userManager.AddLoginAsync(user, info);
            }
            else
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }

            var jwtToken = await CreateJwtToken(user);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new OkObjectResult(new
            {
                message = "Google Login Successful",
                token
            });
        }

        public async Task<Result<VerifyEmailResponseDto>> VerifyEmailOtpAsync(VerifyEmailOtpDto dto)
        {
            var userId = await _cache.GetStringAsync($"email_verify_otp:{dto.Otp}");
            if (string.IsNullOrEmpty(userId))
                return Result<VerifyEmailResponseDto>.Fail("Invalid or expired OTP");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.EmailConfirmed)
                return Result<VerifyEmailResponseDto>.Fail("Invalid or expired OTP");

            var attemptsKey = $"email_verify_otp_attempts:{user.Id}";
            var attemptsStr = await _cache.GetStringAsync(attemptsKey);
            var attempts = string.IsNullOrEmpty(attemptsStr) ? 0 : int.Parse(attemptsStr);

            if (attempts >= MaxOtpAttempts)
            {
                await _cache.RemoveAsync($"email_verify_otp:{dto.Otp}");
                await _cache.RemoveAsync(attemptsKey);
                return Result<VerifyEmailResponseDto>.Fail("OTP has been locked due to too many attempts");
            }

            var email = await _cache.GetStringAsync($"email_verify_otp:{dto.Otp}");
            if (string.IsNullOrEmpty(email))
            {
                await IncrementOtpAttempts.IncrementOtpAttemptsAsync(_cache, attemptsKey, attempts, OtpValidityMinutes);
                return Result<VerifyEmailResponseDto>.Fail("Invalid or expired OTP");
            }

            //confirm email
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            //cleanup
            await _cache.RemoveAsync($"email_verify_otp:{dto.Otp}");
            await _cache.RemoveAsync(attemptsKey);

            //generate JWT taken
            var jwt = await CreateJwtToken(user);

            return Result<VerifyEmailResponseDto>.Ok(new VerifyEmailResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpiresOn = jwt.ValidTo
            });
        }

        public async Task<Result<bool>> ResendEmailOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<bool>.Fail("User not found");

            if (user.EmailConfirmed)
                return Result<bool>.Fail("Email is already verified");

            // Generate new OTP
            var otp = OtpGenerator.Generate() ; 

            // Save OTP in cache
            await _cache.SetStringAsync(
                $"email_verify_otp:{otp}",
                user.Id,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
                });

            // Reset attempts
            await _cache.RemoveAsync($"email_verify_otp_attempts:{user.Id}");

            // Send email
            await _emailSender.SendEmailAsync(
                user.Email,
                "Resend Verification Code",
                $"Your verification code is <b>{otp}</b>. It expires in {OtpValidityMinutes} minutes."
            );

            return Result<bool>.Ok(true);
        }
    }
}