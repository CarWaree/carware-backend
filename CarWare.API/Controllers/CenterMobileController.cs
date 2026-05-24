using CarWare.API.Errors;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.Slots;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenterMobileController : ControllerBase
    {
        private readonly IServiceCenterService _serviceCenterService;

        public CenterMobileController(IServiceCenterService serviceCenterService)
        {
            _serviceCenterService = serviceCenterService;
        }

        //Services Available at a Specific Center
        [HttpGet("{centerId:int}/services")]
        public async Task<IActionResult> GetServices(int centerId)
        {
            var result = await _serviceCenterService.GetCenterServicesAsync(centerId);

            if (!result.Success)
                return NotFound(ApiResponseGeneric<List<MaintenanceTypeDto>>
                    .Fail(result.ErrorCode!));

            return Ok(ApiResponseGeneric<List<MaintenanceTypeDto>>
                .Success(result.Data!));
        }

        //Available Slots for a Specific Center
        [HttpGet("{centerId:int}/slots")]
        public async Task<IActionResult> GetSlots(int centerId)
        {
            var result = await _serviceCenterService.GetCenterSlotsAsync(centerId);

            if (!result.Success)
                return NotFound(ApiResponseGeneric<string>.Fail(result.ErrorCode!));

            return Ok(ApiResponseGeneric<CenterSlotsResponseDto>.Success(result.Data!));
        }
    }
}