using CarWare.Application.Common;
using CarWare.Application.DTOs.Dashboard.Setup;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class SetupService : ISetupService
    {
        private readonly IUnitOfWork _uow;

        public SetupService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<SetupResponseDto>> CompleteSetupAsync(string adminUserId, SetupRequestDto dto)
        {
            // 1. Find this admin's ServiceCenter
            var center = await _uow.ServiceCenterRepository
                .GetByAdminUserIdAsync(adminUserId);

            if (center == null)
                return Result<SetupResponseDto>
                    .Fail("No ServiceCenter linked to this admin account.");

            if (dto.WorkingTo <= dto.WorkingFrom)
            {
                return Result<SetupResponseDto>.Fail(
                    "WorkingTo must be greater than WorkingFrom");
            }

            // 2. Update basic info
            center.Name = dto.Name;
            center.WorkingFrom = dto.WorkingFrom;  
            center.WorkingTo = dto.WorkingTo;  

            _uow.ServiceCenterRepository.Update(center);

            await _uow.CompleteAsync();

            // 3. Sync ProviderServices
            await _uow.ProviderServicesRepository
                .DeleteByCenterIdAsync(center.Id);

            var selectedTypes = await _uow
                .MaintenanceTypeRepository
                .GetByIdsAsync(dto.MaintenanceTypeIds);

            var providerServices = selectedTypes
                .Select(mt => new ProviderServices
                {
                    ServiceCenterId = center.Id,
                    ServiceId = mt.Id
                }).ToList();

            // Handle custom service
            if (!string.IsNullOrWhiteSpace(dto.OtherServiceName))
            {
                var customType = new MaintenanceType
                {
                    Name = dto.OtherServiceName
                };

                await _uow.MaintenanceTypeRepository
                    .AddAsync(customType);

                await _uow.CompleteAsync();

                providerServices.Add(new ProviderServices
                {
                    ServiceCenterId = center.Id,
                    ServiceId = customType.Id
                });
            }

            if (providerServices.Any())
            {
                await _uow.ProviderServicesRepository
                    .AddRangeAsync(providerServices);
            }

            // 4. Regenerate Slots
            await _uow.SlotRepository
                .DeleteByCenterIdAsync(center.Id);

            var slots = GenerateSlots(
                center.Id,
                dto.WorkingFrom,
                dto.WorkingTo);

            if (slots.Any())
            {
                await _uow.SlotRepository
                    .AddRangeAsync(slots);
            }

            await _uow.CompleteAsync();

            var response = new SetupResponseDto
            {
                ServiceCenterId = center.Id,
                CenterName = center.Name,
                Message = "Setup completed successfully."
            };

            return Result<SetupResponseDto>.Ok(response);
        }

        //Helper
        // Generates one slot per hour Mon–Sat within working hours
        private static List<Slot> GenerateSlots(
            int centerId,
            TimeSpan from,
            TimeSpan to)
        {
            var slots = new List<Slot>();

            var workDays = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };

            var interval = TimeSpan.FromHours(1);

            foreach (var day in workDays)
            {
                var cursor = from;

                while (cursor + interval <= to)
                {
                    slots.Add(new Slot
                    {
                        ServiceCenterId = centerId,
                        DayOfWeek = day,
                        StartTime = cursor,
                        EndTime = cursor + interval,
                        IsActive = true
                    });

                    cursor += interval;
                }
            }

            return slots;
        }
    }
}