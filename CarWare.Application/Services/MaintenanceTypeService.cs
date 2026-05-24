using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class MaintenanceTypeService : IMaintenanceTypeService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaintenanceTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //Services for a Specific Center 
        public async Task<Result<IEnumerable<MaintenanceTypeDto>>> GetCenterServicesAsync(int centerId)
        {
            var data = await _unitOfWork.Repository<MaintenanceType>().GetAllAsync();

            if (data == null || !data.Any())
                return Result<IEnumerable<MaintenanceTypeDto>>.Fail("No maintenance types found");

            var mapped = _mapper.Map<IEnumerable<MaintenanceTypeDto>>(data);

            return Result<IEnumerable<MaintenanceTypeDto>>.Ok(mapped);
        }


    }
}