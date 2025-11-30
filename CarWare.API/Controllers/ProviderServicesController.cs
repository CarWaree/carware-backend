using CarWare.API.Errors.NonGeneric;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderServicesController : ControllerBase
    {
        private readonly IProviderServicesService _providerServicesService;

        public ProviderServicesController(IProviderServicesService providerServicesService)
        {
            _providerServicesService = providerServicesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _providerServicesService.GetAllAsync();
            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(result.Data);
        }
        [HttpGet("by-service-type")]
        public async Task<IActionResult> GetByServiceType([FromQuery] int serviceTypeId)
        {
            var result = await _providerServicesService.GetProvidersByServiceTypeAsync(serviceTypeId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(result.Data);
        }
    }
}
