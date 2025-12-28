using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IMaintenanceTypeService _service;

        public ServiceController(IMaintenanceTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            if (!result.Success)
            {
                return BadRequest(ApiResponse.Fail(result.Error));
            }

            return Ok(ApiResponseGeneric<IEnumerable<MaintenanceTypeDto>>
                .Success(result.Data, "Maintenance types retrieved successfully"));
        }

    }
}
