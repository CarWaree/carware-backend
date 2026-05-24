using CarWare.Application.Common;
using CarWare.Application.DTOs.Role;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<string>> CreateRoleAsync(CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return Result<string>.Fail("Role name is required");

            if (await _roleManager.RoleExistsAsync(dto.RoleName))
                return Result<string>.Fail("Role already exists");

            var result = await _roleManager.CreateAsync(
                new IdentityRole(dto.RoleName.ToUpper())
            );

            if (!result.Succeeded)
                return Result<string>.Fail("Error creating role");

            _logger.LogInformation("Role created: {Role}", dto.RoleName);

            return Result<string>.Ok("Role created successfully");
        }

        public async Task<Result<string>> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return Result<string>.Fail("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return Result<string>.Fail("Role does not exist");

            if (await _userManager.IsInRoleAsync(user, dto.RoleName))
                return Result<string>.Fail("User already has this role");

            // Validate center if CENTERADMIN
            if (dto.RoleName.Equals("CENTERADMIN", StringComparison.OrdinalIgnoreCase))
            {
                if (!dto.ServiceCenterId.HasValue)
                    return Result<string>.Fail("ServiceCenterId is required");

                var centerExists = await _unitOfWork
                    .Repository<ServiceCenter>()
                    .AnyAsync(x => x.Id == dto.ServiceCenterId.Value);

                if (!centerExists)
                    return Result<string>.Fail("Service center not found");
            }

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
                return Result<string>.Fail("Error assigning role");

            // Assign center claim
            if (dto.RoleName.Equals("CENTERADMIN", StringComparison.OrdinalIgnoreCase))
            {
                var claims = await _userManager.GetClaimsAsync(user);

                var oldClaim = claims.FirstOrDefault(
                    c => c.Type == "ServiceCenterId"
                );

                if (oldClaim != null)
                    await _userManager.RemoveClaimAsync(user, oldClaim);

                await _userManager.AddClaimAsync(
                    user,
                    new Claim(
                        "ServiceCenterId",
                        dto.ServiceCenterId.Value.ToString()
                    ));

                user.ServiceCenterId = dto.ServiceCenterId;

                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation(
                "Role {Role} assigned to user {UserId}",
                dto.RoleName,
                dto.UserId
            );

            return Result<string>.Ok("Role assigned successfully");
        }

        public async Task<Result<string>> RemoveRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return Result<string>.Fail("User not found");

            if (!await _userManager.IsInRoleAsync(user, dto.RoleName))
                return Result<string>.Fail("User does not have this role");

            // Remove center claim if CENTERADMIN
            if (dto.RoleName.Equals("CENTERADMIN", StringComparison.OrdinalIgnoreCase))
            {
                var claims = await _userManager.GetClaimsAsync(user);

                var claim = claims.FirstOrDefault(
                    c => c.Type == "ServiceCenterId"
                );

                if (claim != null)
                    await _userManager.RemoveClaimAsync(user, claim);
            }

            var result = await _userManager.RemoveFromRoleAsync(
                user,
                dto.RoleName
            );

            if (!result.Succeeded)
                return Result<string>.Fail("Error removing role");

            _logger.LogInformation(
                "Role {Role} removed from user {UserId}",
                dto.RoleName,
                dto.UserId
            );

            return Result<string>.Ok("Role removed successfully");
        }

        public async Task<Result<List<string>>> GetRolesAsync()
        {
            var roles = _roleManager.Roles
                .Select(r => r.Name!)
                .ToList();

            return Result<List<string>>.Ok(roles);
        }

        public async Task<Result<List<string>>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Result<List<string>>.Fail("User not found");

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            return Result<List<string>>.Ok(roles);
        }
    }
}