using CarWare.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class Payment : BaseEntity
    {
        //FK
        public int AppointmentId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? TransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Appointment Appointment { get; set; }
    }
}