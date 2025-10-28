using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _cache;
        private readonly IEmailSender _emailSender;
        private const int OtpValidityMinutes = 3;
        private readonly JWT _jwt;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt
            , IEmailSender emailSender, IDistributedCache cache)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _cache = cache;
            _emailSender = emailSender;
        }

        private string GetCacheKey(string email) => $"OTP_{email.ToUpperInvariant()}";

        private async Task<String> CreateJwtToken(ApplicationUser user)
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

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return tokenString;
        }

        public async Task<AuthDto> RegisterAsync(RegisterDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthDto { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthDto { Message = "Username is already registered!" };

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            return new AuthDto
            {
                Email = user.Email,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsAuthenticated = true,
                Roles = rolesList.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo,
                Message = "Registration successful"
            };
        }

        public async Task<AuthDto> LoginAsync(LoginDto loginDto)
         {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return new AuthDto { Message = "Invalid email or password" };

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            return new AuthDto
            {
                IsAuthenticated = true,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = rolesList.ToList(),
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo,
                Message = "Login successful"
            };

         }

        public async Task<bool> RequestResetAsync(ForgetPasswordDto forgetDTO)
        {
            var user = await _userManager.FindByEmailAsync(forgetDTO.Email);

            //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) return true;

            //generate and store OTP code (6 digits)
            var cacheKey = GetCacheKey(forgetDTO.Email);
            var otpCode = new Random().Next(100000, 999999).ToString();
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpValidityMinutes)
            };

            //store the OTP in Distributed Cache with Expiration time
            await _cache.SetStringAsync(cacheKey, otpCode, cacheOptions);

            //send email with OTP
            await _emailSender.SendEmailAsync(forgetDTO.Email, 
                "Password Reset Code",
                $"Your verification code is <b>{otpCode}</b>. It expires in 3 minutes.");

            return true;
        }

        public async Task<ResetPasswordResultDto?> VerifyOtpAsync(VerifyOtpDto optDto)
        {
            var user = await _userManager.FindByEmailAsync(optDto.Email);
            if (user == null) return null;

            var cachedOtp = await _cache.GetStringAsync(GetCacheKey(optDto.Email));
            if (cachedOtp == null || cachedOtp != optDto.Otp) return null;

            await _cache.RemoveAsync(GetCacheKey(optDto.Email));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new ResetPasswordResultDto
            {
                UserID = user.Id,
                Email = user.Email,
                Token = token
            };
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            if (resetDto.NewPassword != resetDto.ConfirmPassword)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordMismatch",
                    Description = "New password and confirmation do not match."
                });
            }

            var user = await _userManager.FindByIdAsync(resetDto.UserId);
            if (user == null) return IdentityResult.Success;

            return await _userManager.ResetPasswordAsync(user, resetDto.Token, resetDto.NewPassword);
        }                                                             
    }
}