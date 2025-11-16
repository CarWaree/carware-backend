using AutoMapper;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;
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
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt.Value;
            _cache = cache;
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

        public async Task<AuthDto> RegisterAsync(RegisterDto model)
        {
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return new AuthDto { Message = "Email is already registered!", IsAuthenticated = false };

            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
                return new AuthDto { Message = "Username is already taken!", IsAuthenticated = false };

            var user = _mapper.Map<ApplicationUser>(model);

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(e => e.Description));
                return new AuthDto { Message = errors, IsAuthenticated = false };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            var authDto = _mapper.Map<AuthDto>(user);
            authDto.IsAuthenticated = true;
            authDto.Roles = rolesList.ToList();
            authDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authDto.ExpiresOn = jwtSecurityToken.ValidTo;
            authDto.Message = "Registration successful";

            return authDto;
        }

        public async Task<AuthDto> LoginAsync(LoginDto loginDto)
         {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

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

        public async Task<ResetPasswordResultDto> VerifyOtpAsync(VerifyOtpDto optDto)
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

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return new BadRequestObjectResult(new { message = $"Error from external provider: {remoteError}" });

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return new BadRequestObjectResult(new { message = "External login information not found." });

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                return new OkObjectResult(new
                {
                    message = "Login successful",
                    user = new { user.Id, user.Email, Role = role }
                });
            }

            // Create user if not exist
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return new BadRequestObjectResult(new { message = "Email claim not provided by external provider." });

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                    return new BadRequestObjectResult(createResult.Errors);

                await _userManager.AddToRoleAsync(newUser, "Admin");
                await _userManager.AddLoginAsync(newUser, info);
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                return new OkObjectResult(new
                {
                    message = "User created and logged in successfully",
                    user = new { newUser.Id, newUser.Email, Role = "Admin" }
                });
            }

            // Link login to existing user
            await _userManager.AddLoginAsync(existingUser, info);
            await _signInManager.SignInAsync(existingUser, isPersistent: false);

            var existingUserRole = (await _userManager.GetRolesAsync(existingUser)).FirstOrDefault();

            return new OkObjectResult(new
            {
                message = "External login linked successfully",
                user = new { existingUser.Id, existingUser.Email, Role = existingUserRole }
            });
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return new OkObjectResult(new { message = "Logged out successfully" });
        }

        /*//public Task<AuthDto> LoginWithGoogleAsync(string googleToken)
        {
            throw new NotImplementedException();
        }*/
    }
}