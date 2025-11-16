using AutoMapper;
using CarWare.Application.DTOs.Vehicle;
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
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleController(IUnitOfWork unitOfWork,IMapper mapper)
        {
           
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        //Get All
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDTOs>>> GetAll() {
            var vehicleRepo = _unitOfWork.Repository<Vehicle>();
            var vehicles = await vehicleRepo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<Vehicle>,IEnumerable<VehicleDTOs>>(vehicles));
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
            return Ok(_mapper.Map<Vehicle,VehicleDTOs>(vehicle));
        }
    }
}
