using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.DTOs.Profile;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        public ProfileService(UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IWebHostEnvironment env)
        {
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authService = authService;
            _env = env;

        }

        public async  Task<Result<EditProfileResponseDto>> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<EditProfileResponseDto>.Fail("User not found");

            var dto = new EditProfileResponseDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl ?? "/profile-images/default.png"
            };

            return Result<EditProfileResponseDto>.Ok(dto);
        }

        public async Task<Result<string>> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<string>.Fail("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                var names = dto.FullName.Split(' ', 2);
                user.FirstName = names[0];
                user.LastName = names.Length > 1 ? names[1] : "";
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            //  Email Change 
            var newEmail = dto.PendingEmail?.Trim().ToLower();
            var currentEmail = user.Email?.Trim().ToLower();

            bool emailChanged = false;

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != currentEmail)
            {
                user.PendingEmail = newEmail;

                var otpResult = await _authService.ResendEmailOtpAsync(
                    new ResendEmailDto
                        {
                            Email = newEmail
                        });
                if (!otpResult.Success)
                    return Result<string>.Fail("Failed to send verification email");

                emailChanged = true;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<string>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Result<string>.Ok(
                emailChanged
                    ? "Profile updated successfully. Please verify your new email."
                    : "Profile updated successfully."
            );
        }

        public async Task<Result<string>> UploadImageAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<string>.Fail("User not found");

            if (file == null || file.Length == 0)
                return Result<string>.Fail("No file uploaded");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "profile-images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfileImageUrl = $"/profile-images/{uniqueFileName}";
            await _userManager.UpdateAsync(user);

            return Result<string>.Ok(user.ProfileImageUrl);
        }
    }
}