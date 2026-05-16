using CarWare.API.Errors;
using CarWare.Application.DTOs.Dashboard;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CENTERADMIN")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
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