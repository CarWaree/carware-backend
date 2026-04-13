using AutoMapper;
using CarWare.Application.DTOs.Appointment;
using CarWare.Application.DTOs.History;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.DTOs.maintenanceReminder;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.DTOs.Profile;
using CarWare.Application.DTOs.Provider_Center;
using CarWare.Application.DTOs.Vehicle;
using CarWare.Domain.Entities;
using System.Linq;

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
                .ForMember(dest => dest.VehicleName,
                    opt => opt.MapFrom(src => src.Vehicle.Name))
                .ForMember(dest => dest.ProviderName,
                    opt => opt.MapFrom(src => src.ServiceCenter.Name))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<ApplicationUser, EditProfileResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl));

            //History Profile
            CreateMap<ServiceRequest, HistoryDetailsDto>()
                .ForMember(dest => dest.CarName,
                    opt => opt.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.ServiceRequestServices
                        .Select(s => s.MaintenanceType.Name)
                        .FirstOrDefault()))
                .ForMember(dest => dest.ProviderName,
                    opt => opt.MapFrom(src => src.ServiceCenter.Name))
                .ForMember(dest => dest.Date,
                    opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.ServiceDetails,
                    opt => opt.MapFrom(src => src.ServiceRequestServices
                        .Select(s => s.Description)
                        .FirstOrDefault()));

            //Notification System
            CreateMap<Notification, NotificationDto>();

            CreateMap<SendNotificationDto, Notification>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsRead, opt => opt.Ignore())
            .ForMember(dest => dest.IsSent, opt => opt.Ignore());
        }
    }
}