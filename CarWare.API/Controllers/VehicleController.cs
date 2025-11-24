using AutoMapper;

using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Application.Services;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using CarWare.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
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
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IVehicleService _vehicleService;

        public VehicleController(IUnitOfWork unitOfWork, IMapper mapper, IVehicleService vehicleService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _vehicleService = vehicleService;
        }

        //Get All

        [HttpGet]
        public async Task<ActionResult<Pagination<VehicleDTOs>>> GetAll([FromQuery] PaginationParameters @params ){
            var query = _unitOfWork.Repository<Vehicle>().Query();
 
            var pagedVehicles = await query.ProjectTo<VehicleDTOs>(_mapper.ConfigurationProvider)
                                  .ToPagedList(@params.PageIndex, @params.PageSize);



            return Ok(pagedVehicles);
        }
        //GetById

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDTOs>> GetById(int id)
        {
            var vehicleRepo = _unitOfWork.Repository<Vehicle>();
            var vehicle = await vehicleRepo.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<Vehicle, VehicleDTOs>(vehicle));

        }
        //Add

        [HttpPost]
        public async Task<ActionResult> AddVehicle([FromBody] VehicleDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicleRepo = _unitOfWork.Repository<Vehicle>();
            var vehicle = _mapper.Map<Vehicle>(dto);

            await vehicleRepo.AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();

            return Ok(_mapper.Map<VehicleDTOs>(vehicle));
        }
        
        //UPDATE

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDTOs dto)
        {
            dto.Id = id;

            var updated = await _vehicleService.UpdateVehicleAsync(dto);

            if (!updated)
                return NotFound("Vehicle not found");

            return Ok("Vehicle updated successfully");
        }

        //DELETE

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var deleted = await _vehicleService.DeleteVehicleAsync(id);

            if (!deleted)
                return NotFound("Vehicle not found");

            return Ok("Vehicle deleted successfully");
        }


    }
}
