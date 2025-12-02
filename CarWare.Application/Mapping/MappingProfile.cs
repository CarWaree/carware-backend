using AutoMapper;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.Provider_Center;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Domain.Entities;

namespace CarWare.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Vehicle, VehicleDTOs>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.user.UserName));

            CreateMap<VehicleCreateDTO, Vehicle>();
            CreateMap<Brand, BrandDTO>();
            CreateMap<Model, ModelDTO>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));

            CreateMap<MaintenanceType, MaintenanceTypeDto>();
            CreateMap<ServiceCenter, ServiceCenterDto>();

        }
    }

}
