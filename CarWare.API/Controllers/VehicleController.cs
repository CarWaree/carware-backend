using AutoMapper;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Application.Services;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: api/vehicle
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDTOs>>> GetAll()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        // GET: api/vehicle/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDTOs>> GetById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound("Vehicle not found");
            return Ok(vehicle);
        }

        // POST: api/vehicle
        [HttpPost]
        public async Task<ActionResult<VehicleDTOs>> AddVehicle([FromBody] VehicleDTOs dto)
        {
            var createdVehicle = await _vehicleService.AddVehicleAsync(dto);
            return Ok(createdVehicle);
        }

        // PUT: api/vehicle/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDTOs dto)
        {
            dto.Id = id;
            var updated = await _vehicleService.UpdateVehicleAsync(dto);
            if (!updated) return NotFound("Vehicle not found");
            return Ok("Vehicle updated successfully");
        }

        // DELETE: api/vehicle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var deleted = await _vehicleService.DeleteVehicleAsync(id);
            if (!deleted) return NotFound("Vehicle not found");
            return Ok("Vehicle deleted successfully");
        }
    }

}
