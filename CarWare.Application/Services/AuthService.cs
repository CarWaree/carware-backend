using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
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
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
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

        public async Task<Result<AuthDto>> RegisterAsync(RegisterDto model)
        {
            // 1. Check if username or email already exists
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
                return Result<AuthDto>.Fail("Username is already taken!");

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return Result<AuthDto>.Fail("Email is already registered!");

            // 2. Map DTO to ApplicationUser
            var user = _mapper.Map<ApplicationUser>(model);

            // 3. Create user
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return Result<AuthDto>.Fail(errors);
            }

            // 4. Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // 5. Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // 6. Build verification URL (frontend link)
            var frontendUrl = _config["App:FrontendUrl"];
            var verificationUrl = $"{frontendUrl}/verify-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

            // 7. Send verification email
            await _emailSender.SendEmailAsync(user.Email,
                "Verify your email",
                $"Hello {user.FirstName},<br/><br/>" +
                $"Please confirm your email by clicking the link below:<br/>" +
                $"<a href='{verificationUrl}'>Verify Email</a><br/><br/>" +
                "Thank you!");

            // 8. Return AuthDto (user is NOT authenticated yet)
            var authDto = _mapper.Map<AuthDto>(user);
            authDto.IsAuthenticated = false; // must verify email first
            authDto.Roles = new List<string>(); // no JWT yet
            authDto.Token = null;
            authDto.ExpiresOn = null;

            return Result<AuthDto>.Ok(authDto);
        }

        public async Task<Result<AuthDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Result<AuthDto>.Fail("Invalid Username or Password");

            
            //verfiy before login
            if (!user.EmailConfirmed)
                return Result<AuthDto>.Fail("Please verify your email before logging in.");


            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            var authDto = new AuthDto
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

            return Result<AuthDto>.Ok(authDto);
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
            var cachedEmail = await _cache.GetStringAsync($"otp:{optDto.Otp}");
            if (cachedEmail == null)
                return Result<ResetPasswordResultDto>.Fail("Invalid or expired OTP");

            var user = await _userManager.FindByEmailAsync(cachedEmail);
            if (user == null)
                return Result<ResetPasswordResultDto>.Fail("User not found");

            await _cache.RemoveAsync($"otp:{optDto.Otp}");

            // Generate Identity reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create a short handle (GUID)
            var handle = Guid.NewGuid().ToString("N");

            // Store mapping: handle -> actual resetToken (consider encrypting if you want)
            await _cache.SetStringAsync($"reset_handle:{handle}", resetToken,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

            // Store also handle -> email if you want quick lookup or extra validation
            await _cache.SetStringAsync($"reset_handle_email:{handle}", cachedEmail,
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
    }
}