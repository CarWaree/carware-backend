using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Role;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // Get All Roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _roleService.GetRolesAsync();

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<string>>.Success(result.Data));
        }

        // Create Role
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var result = await _roleService.CreateRoleAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponse.Success(result.Data));
        }

        // Assign Role
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var result = await _roleService.AssignRoleAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponse.Success(result.Data));
        }

        // Remove Role
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            var result = await _roleService.RemoveRoleAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponse.Success(result.Data));
        }

        // Get User Roles
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var result = await _roleService.GetUserRolesAsync(userId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<string>>.Success(result.Data));
        }
    }
}