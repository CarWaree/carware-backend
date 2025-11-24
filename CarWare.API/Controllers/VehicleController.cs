using AutoMapper;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Mvc;
using AutoMapper.QueryableExtensions;
using CarWare.Application.Common;
using CarWare.Application.helper;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IMapper _mapper;

        public VehicleController(IVehicleService vehicleService, IMapper mapper)
        {
            _vehicleService = vehicleService;
            _mapper = mapper;
        }

        // GET: api/vehicle
        [HttpGet]
        public async Task<ActionResult<Pagination<VehicleDTOs>>> GetAll([FromQuery] PaginationParameters @params ){
            var query = _vehicleService.QueryVehicles();

            var pagedVehicles = await query
                .ProjectTo<VehicleDTOs>(_mapper.ConfigurationProvider)
                .ToPagedList(@params.PageIndex, @params.PageSize);

            return Ok(pagedVehicles);
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
