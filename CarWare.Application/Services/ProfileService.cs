using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Auth;
using CarWare.Application.DTOs.Dashboard.Profile;
using CarWare.Application.DTOs.Maintenance;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authService = authService;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<EditProfileResponseDto>> GetProfileAsync(string userId)
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

            // Full Name
            var names = dto.FullName.Trim().Split(' ', 2);

            user.FirstName = names[0];
            user.LastName = names.Length > 1 ? names[1] : "";

            // Phone
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            // Email
            var newEmail = dto.Email.Trim().ToLower();

            if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(newEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                    return Result<string>.Fail("Email is already in use");

                user.Email = newEmail;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Result<string>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result<string>.Ok("Profile updated successfully");
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

        public async Task<Result<CenterProfileDto>> GetCenterProfileAsync(string UserId)
        {
            // 1. Find the center 
            var center = await _unitOfWork.ServiceCenterRepository
                .GetByAdminUserIdAsync(UserId);

            if (center == null)
                return Result<CenterProfileDto>
                    .Fail("No ServiceCenter linked to this admin account.");

            // 2. Get services 
            var providerServices = await _unitOfWork.ProviderServicesRepository
                .GetByCenterIdAsync(center.Id);

            // 4. Map to DTO
            var dto = new CenterProfileDto
            {
                Id = center.Id,
                Name = center.Name,
                WorkingFrom = center.WorkingFrom,
                WorkingTo = center.WorkingTo,

                Services = providerServices.Select(ps => new MaintenanceTypeDto
                {
                    id = ps.Service.Id,
                    Name = ps.Service.Name
                }).ToList(),
            };

            return Result<CenterProfileDto>.Ok(dto);
        }
    }
}