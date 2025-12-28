using CarWare.API.Errors.NonGeneric;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCentersController : ControllerBase
    {
        private readonly IServiceCenterService _serviceCenterService;

        public ServiceCentersController(IServiceCenterService serviceCenterService)
        {
            _serviceCenterService = serviceCenterService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _serviceCenterService.GetAllAsync();
            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(result.Data);
        }


        [HttpGet("by-service-type")]
        public async Task<IActionResult> GetByServiceType([FromQuery] int serviceTypeId)
        {
            var result = await _serviceCenterService.GetByServiceTypeAsync(serviceTypeId);
            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(result.Data);
        }
    }
}

