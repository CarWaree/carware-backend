using CarWare.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class Maintenance : BaseEntity
    {
        public TypeOfMaintenance TypeOfMaintenance { get; set; }
        [Required]
        public DateTime NotificationDate { get; set; }
        [Required]
        public DateTime NextDueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}