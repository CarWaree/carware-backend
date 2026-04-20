using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.Common;
using CarWare.Application.DTOs.ServiceRequests;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/service-requests")]
    [ApiController]
    [Authorize]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _service;

        public ServiceRequestController(IServiceRequestService service)
        {
            _service = service;
        }

        #region Dashboard
        [HttpGet]
        public async Task<ActionResult> GetDashboard([FromQuery] ServiceRequestQueryParams queryParams)
        {
            var result = await _service.GetDashboardRequestsAsync(queryParams);

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<ServiceRequest>.Fail(result.Error!));

            return Ok(result.Data);
        }
        #endregion

        #region Details

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var result = await _service.GetRequestDetailsAsync(id);

            return Ok(ApiResponseGeneric<ServiceRequestDto>.Success(result.Data));
        }

        #endregion

        #region Workflow Actions

        [HttpPatch("{id}/accept")]
        public async Task<ActionResult> Accept(int id, [FromBody] AcceptServiceRequestDto dto)
        {
            await _service.AcceptAsync(id, dto);

            return Ok(ApiResponse.Success("Request accepted successfully"));
        }

        [HttpPatch("{id}/reject")]
        public async Task<ActionResult> Reject(int id, [FromBody] RejectServiceRequestDto dto)
        {
            await _service.RejectAsync(id, dto);

            return Ok(ApiResponse.Success("Request rejected successfully"));
        }

        [HttpPatch("{id}/complete")]
        public async Task<ActionResult> Complete(int id, [FromBody] CompleteServiceRequestDto dto)
        {
            await _service.CompleteAsync(id, dto);

            return Ok(ApiResponse.Success("Request completed successfully"));
        }
        #endregion
    }
}