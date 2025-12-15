using AutoMapper;
using CarWare.API.Errors;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.Interfaces;
using CarWare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMapper _mapper;

        public AppointmentsController(IAppointmentService appointmentService, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
        }
        private string userId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // PUT /api/appointments/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> Cancel(int id)
        {
            var result = await _appointmentService.CancelAsync(id, userId);

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error));

            return Ok(ApiResponseGeneric<AppointmentDto>.Success(
                result.Data,
                "Appointment cancelled successfully"
            ));
        }

        // PUT /api/appointments/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,ServiceCenter")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] AppointmentStatus status)
        {
            var result = await _appointmentService.UpdateStatusAsync(id, status);

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error));

            return Ok(ApiResponseGeneric<AppointmentDto>.Success(
                result.Data,
                "Appointment status updated successfully"
            ));
        }
    }
}
