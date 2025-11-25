using AutoMapper;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Mvc;
using AutoMapper.QueryableExtensions;
using CarWare.Application.Common;
using CarWare.Application.helper;
using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;

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

        [HttpGet]
        public async Task<ActionResult<Pagination<VehicleDTOs>>> GetAll([FromQuery] PaginationParameters @params ){
            var query = _vehicleService.QueryVehicles();

            var pagedVehicles = await query
                .ProjectTo<VehicleDTOs>(_mapper.ConfigurationProvider)
                .ToPagedList(@params.PageIndex, @params.PageSize);

            return Ok(ApiResponseGeneric<Pagination<VehicleDTOs>>.Success(
                pagedVehicles, "Vehicles retrieved successfully"
            ));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseGeneric<VehicleDTOs>>> GetById(int id)
        {
            var result = await _vehicleService.GetVehicleByIdAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponseGeneric<VehicleDTOs>.Success(
                result.Data,
                "Vehicle retrieved successfully"
            ));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseGeneric<VehicleDTOs>>> AddVehicle([FromBody] VehicleDTOs dto)
        {
            var result = await _vehicleService.AddVehicleAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return Ok(ApiResponseGeneric<VehicleDTOs>.Success(
                result.Data,
                "Vehicle created successfully"
            ));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateVehicle(int id, [FromBody] VehicleDTOs dto)
        {
            dto.Id = id;
            var result = await _vehicleService.UpdateVehicleAsync(dto);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Vehicle updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteVehicle(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Vehicle deleted successfully"));
        }
    }
}