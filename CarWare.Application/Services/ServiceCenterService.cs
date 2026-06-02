using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.Provider_Center;
using CarWare.Application.DTOs.Slots;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class ServiceCenterService : IServiceCenterService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ServiceCenterService(
            IMapper mapper,
            IUnitOfWork uow)
        {
            _mapper = mapper;
            _uow = uow;
        }

        // Return All Serivce Centers
        public async Task<Result<List<ServiceCenterDto>>> GetAllServiceCentersAsync()
        {
            var centers = await _uow.ServiceCenterRepository.GetAllAsync();

            var result = centers.Select(c => new ServiceCenterDto
            {
                Id = c.Id,
                Name = c.Name,
                Location = c.Location
            }).ToList();

            return Result<List<ServiceCenterDto>>.Ok(result);
        }

        // Available time slots for a specific center
        public async Task<Result<CenterSlotsResponseDto>>
            GetCenterSlotsAsync(int centerId)
        {
            var center = await _uow.ServiceCenterRepository
                .GetWithDetailsAsync(centerId);

            if (center == null)
                return Result<CenterSlotsResponseDto>
                    .Fail("Service center not found");

            // center.Slots already loaded from GetWithDetailsAsync
            var response = new CenterSlotsResponseDto
            {
                ServiceCenterId = center.Id,
                CenterName = center.Name,

                Slots = center.Slots.Select(s => new SlotDto
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                }).ToList()
            };

            return Result<CenterSlotsResponseDto>.Ok(response);
        }

        // Services for a specific center
        public async Task<Result<List<MaintenanceTypeDto>>>
            GetCenterServicesAsync(int centerId)
        {
            var providerServices = await _uow
                .ProviderServicesRepository
                .GetByCenterIdAsync(centerId);

            if (providerServices == null || !providerServices.Any())
                return Result<List<MaintenanceTypeDto>>
                    .Fail("No services found for this center");

            var result = providerServices.Select(ps =>
                new MaintenanceTypeDto
                {
                    id = ps.Service.Id,
                    Name = ps.Service.Name
                }).ToList();

            return Result<List<MaintenanceTypeDto>>.Ok(result);
        }
    }
}