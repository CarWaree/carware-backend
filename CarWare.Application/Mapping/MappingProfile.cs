using AutoMapper;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.maintenanceReminder;
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
            CreateMap<VehicleUpdateDTO, Vehicle>()
                .ForMember(dest => dest.BrandId, opt => opt.Ignore())
                .ForMember(dest => dest.ModelId, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Brand, BrandDTO>();
            CreateMap<Model, ModelDTO>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name));

            CreateMap<MaintenanceType, MaintenanceTypeDto>();
            CreateMap<ServiceCenter, ServiceCenterDto>();
            CreateMap<CreateMaintenanceReminderDto, MaintenanceReminder>();
            CreateMap<UpdateMaintenanceReminderDto, MaintenanceReminder>();
            CreateMap<MaintenanceReminder, MaintenanceReminderResponseDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.Name))
            .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.Vehicle.Name));




            CreateMap<CreateAppointmentDto, Appointment>();

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.user.UserName))
                .ForMember(dest => dest.VehicleName,
                    opt => opt.MapFrom(src => src.Vehicle.Name))
                .ForMember(dest => dest.ServiceCenterName,
                    opt => opt.MapFrom(src => src.ServiceCenter.Name));


        }
    }
}
