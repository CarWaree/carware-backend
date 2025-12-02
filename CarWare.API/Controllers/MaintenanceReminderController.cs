using AutoMapper;
using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.maintenanceReminder;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceReminderController : ControllerBase
    {
        private readonly IMaintenanceReminderService _reminderService;
        private readonly IMapper _mapper;

        public MaintenanceReminderController(IMaintenanceReminderService reminderService, IMapper mapper)
        {
            _reminderService = reminderService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var result = await _reminderService.GetAllAsync();

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<MaintenanceReminderResponseDto>>.Success(
                result.Data.ToList(), "Maintenance reminders retrieved successfully"
            ));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var result = await _reminderService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponseGeneric<MaintenanceReminderResponseDto>.Success(
                result.Data,
                "Maintenance reminder retrieved successfully"
            ));
        }

        [HttpPost]
        public async Task<ActionResult> AddReminder([FromBody] CreateMaintenanceReminderDto dto)
        {
            var result = await _reminderService.AddAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id },
                ApiResponseGeneric<MaintenanceReminderResponseDto>.Success(result.Data, "Maintenance reminder created successfully")
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateReminder(int id, [FromBody] UpdateMaintenanceReminderDto dto)
        {
            dto.Id = id;
            var result = await _reminderService.UpdateAsync(dto);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Maintenance reminder updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReminder(int id)
        {
            var result = await _reminderService.DeleteAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Maintenance reminder deleted successfully"));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult> UpcomingReminders(int days = 7)
        {
            var result = await _reminderService.UpcomingMaintenanceAsync(days);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return Ok(ApiResponseGeneric<List<MaintenanceReminderResponseDto>>.Success(
                result.Data.ToList(), $"Upcoming maintenance reminders for next {days} days"
            ));
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult> GetByVehicle(int vehicleId)
        {
            var result = await _reminderService.GetAllByCarAsync(vehicleId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return Ok(ApiResponseGeneric<List<MaintenanceReminderResponseDto>>.Success(
                result.Data.ToList(), $"Maintenance reminders for vehicle id {vehicleId}"
            ));
        }
    }
}
