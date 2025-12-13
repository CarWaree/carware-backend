using AutoMapper;
using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IMapper _mapper;

        private string userId => User.FindFirst("uid")?.Value;

        public VehicleController(IVehicleService vehicleService, IMapper mapper)
        {
            _vehicleService = vehicleService;
            _mapper = mapper;
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            var result = await _vehicleService.GetAllVehiclesAsync();

            if (!result.Success)
                return BadRequest(ApiResponseGeneric<string>.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<VehicleDTOs>>.Success(
                result.Data, "Vehicles retrieved successfully"
            ));
        }

        [Authorize]
        [HttpGet("my-vehicles")]
        public async Task<ActionResult> GetMyVehiclesAsync()
        {
            var userId = User.FindFirst("uid")?.Value;

            var result = await _vehicleService.GetMyVehiclesAsync(userId);
            return Ok(result);
        }

        [HttpGet("brands")]
        public async Task<ActionResult> GetAllBrands()
        {
            var result = await _vehicleService.GetAllBrandsAsync();

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<BrandDTO>>
                .Success(result.Data, "Brands fetched successfully."));
        }

        [HttpGet("models")]
        public async Task<ActionResult> GetModelsByBrand(int brandId)
        {
            var result = await _vehicleService.GetModelsByBrandsAsync(brandId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponseGeneric<List<ModelDTO>>
                .Success(result.Data, "Models fetched successfully."));
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponseGeneric<VehicleDTOs>>> AddVehicle([FromBody] VehicleCreateDTO dto)
        {
            var result = await _vehicleService.AddVehicleAsync(dto, userId);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error!));

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id },
                ApiResponseGeneric<VehicleDTOs>.Success(result.Data, "Vehicle created successfully"));

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateVehicle(int id, [FromBody]VehicleCreateDTO dto)
        {
            dto.id = id;
            var result = await _vehicleService.UpdateVehicleAsync(dto, userId);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Vehicle updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteVehicle(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id, userId);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error!, 404));

            return Ok(ApiResponse.Success("Vehicle deleted successfully"));
        }
    }
}