using AutoMapper;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.Provider_Center;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<VehicleDTOs, Vehicle>().ReverseMap(); ;
            CreateMap<MaintenanceType, MaintenanceTypeDto>();
            CreateMap<ServiceCenter, ServiceCenterDto>();

        }
    }

}
