using CarWare.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CarWare.Domain.Entities
{
    public class ServiceRequest : BaseEntity
    {
        //FK
        public string UserId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }
        public int AppointmentId { get; set; }

        public decimal TotalPrice { get; set; }
        public DateTime ServiceDate { get; set; }
        public ServiceRequestStatus ServiceStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        //Navegations 
        public ApplicationUser User { get; set; }
        public Appointment Appointment { get; set; }
        public Vehicle Vehicle { get; set; }
        public ServiceCenter ServiceCenter { get; set; }
        public ICollection<ServiceRequestService> ServiceRequestServices { get; set; }
    }
}
