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
        public string? TechnicianId { get; set; }

        //WorkFlow
        public ServiceRequestStatus ServiceStatus { get; set; } = ServiceRequestStatus.Pending;
        public string? RejectionReason { get; set; }
        public string? TechnicianNotes { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;

        //Business Data
        public decimal TotalPrice { get; set; }
        public DateTime ServiceDate { get; set; }
        
        //Payment
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        //Navegations 
        public ApplicationUser User { get; set; }
        public ApplicationUser? Technician { get; set; }
        public Appointment Appointment { get; set; }
        public Vehicle Vehicle { get; set; }
        public ServiceCenter ServiceCenter { get; set; }
        public ICollection<ServiceRequestItem> ServiceRequestServices { get; set; }
    }
}