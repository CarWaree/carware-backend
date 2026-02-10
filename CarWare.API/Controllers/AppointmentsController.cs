using AutoMapper;
using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.Interfaces;
using CarWare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
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
        private string userId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult> GetMyAppointments()
        {
            var result = await _appointmentService.GetUserAppointmentsAsync(userId);

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error!));

            return Ok(ApiResponseGeneric<List<AppointmentDto>>.Success(
                result.Data!.ToList(),
                "User appointments retrieved successfully"
            ));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddAppointment([FromBody] CreateAppointmentDto dto)
        {
            var result = await _appointmentService.AddAppointmentAsync(dto, userId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return Created(nameof(AddAppointment), ApiResponseGeneric<AppointmentDto>
                .Success(result.Data!, "Appointment created successfully"));
        }
    }
}