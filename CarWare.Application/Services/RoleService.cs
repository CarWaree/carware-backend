using CarWare.Application.Common;
using CarWare.Application.DTOs.Role;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(RoleManager<IdentityRole> roleManager,
                           UserManager<ApplicationUser> userManager, ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<string>> CreateRoleAsync(CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
            {
                _logger.LogWarning("CreateRole failed: Role name is empty");
                return Result<string>.Fail("Role name is required");
            }

            if (await _roleManager.RoleExistsAsync(dto.RoleName))
            {
                _logger.LogWarning("CreateRole failed: Role already exists ({RoleName})", dto.RoleName);
                return Result<string>.Fail("Role already exists");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(dto.RoleName));

            if (!result.Succeeded)
            {
                _logger.LogError("CreateRole failed for {RoleName}. Errors: {Errors}",
                    dto.RoleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return Result<string>.Fail("Error creating role");
            }

            _logger.LogInformation("Role created successfully: {RoleName}", dto.RoleName);
            return Result<string>.Ok("Role created successfully");
        }

        public async Task<Result<string>> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("AssignRole failed: User not found ({UserId})", dto.UserId);
                return Result<string>.Fail("User not found");
            }

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
            {
                _logger.LogWarning("AssignRole failed: Role does not exist ({RoleName})", dto.RoleName);
                return Result<string>.Fail("Role does not exist");
            }

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
            {
                _logger.LogError("AssignRole failed for UserId={UserId}, Role={Role}. Errors: {Errors}",
                    dto.UserId,
                    dto.RoleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return Result<string>.Fail("Error assigning role");
            }

            _logger.LogInformation("Role {RoleName} assigned to User {UserId}", dto.RoleName, dto.UserId);
            return Result<string>.Ok("Role assigned successfully");
        }

        public async Task<Result<string>> RemoveRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                _logger.LogWarning("RemoveRole failed: User not found ({UserId})", dto.UserId);
                return Result<string>.Fail("User not found");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
            {
                _logger.LogError("RemoveRole failed for UserId={UserId}, Role={Role}. Errors: {Errors}",
                    dto.UserId,
                    dto.RoleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return Result<string>.Fail("Error removing role");
            }

            _logger.LogInformation("Role {RoleName} removed from User {UserId}", dto.RoleName, dto.UserId);
            return Result<string>.Ok("Role removed successfully");
        }

        public async Task<Result<List<string>>> GetRolesAsync()
        {
            var roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            _logger.LogInformation("Fetched {Count} roles", roles.Count);

            return Result<List<string>>.Ok(roles);
        }

        public async Task<Result<List<string>>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("GetUserRoles failed: User not found ({UserId})", userId);
                return Result<List<string>>.Fail("User not found");
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            _logger.LogInformation("Fetched {Count} roles for User {UserId}", roles.Count, userId);

            return Result<List<string>>.Ok(roles);
        }
    }
}