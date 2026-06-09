using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using System;
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

        //All Services 
        public async Task<Result<IEnumerable<MaintenanceTypeDto>>> GetAllAsync()
        {
            try
            {
                var maintenanceTypes =
                    await _unitOfWork.Repository<MaintenanceType>().GetAllAsync();

                var result = _mapper.Map<IEnumerable<MaintenanceTypeDto>>(maintenanceTypes);

                return Result<IEnumerable<MaintenanceTypeDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<MaintenanceTypeDto>>
                    .Fail($"An error occurred: {ex.Message}");
            }
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