using AutoMapper;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IEnumerable<MaintenanceTypeDto>> GetAllAsync()
        {
            var data = await _unitOfWork.Repository<MaintenanceType>().GetAllAsync();
            return _mapper.Map<IEnumerable<MaintenanceTypeDto>>(data);
        }
       
    }
}
