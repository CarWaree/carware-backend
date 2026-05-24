using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Dashboard;
using CarWare.Application.DTOs.Dashboard.Setup;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CENTERADMIN")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISetupService _setupService;

        public DashboardController(IDashboardService dashboardService,
            ISetupService setupService)
        {
            _dashboardService = dashboardService;
            _setupService = setupService;
        }

        // Setup
        [HttpPost("Setup")]
        public async Task<IActionResult> CompleteSetup(
    [FromBody] SetupRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse.Fail("Invalid request data"));
            }

            // Get logged-in admin id from JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(
                    ApiResponse.Fail("Unauthorized"));
            }

            var result = await _setupService
                .CompleteSetupAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(
                    ApiResponse.Fail(result.Error!));
            }

            return Ok(ApiResponseGeneric<SetupResponseDto>.Success(result.Data));
        }

        /// Returns today's stat cards + the weekly calendar.
        [HttpGet]
        public async Task<IActionResult> GetDashboard([FromQuery] DashboardQueryParams queryParams)
        {
            var result = await _dashboardService.GetDashboardAsync(queryParams);

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error!));

            return Ok(ApiResponseGeneric<object>.Success(result.Data));
        }
    }
}