using System;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Domain.Entities
{
    public class MaintenanceReminder : BaseEntity
    {
        [Required]
        public DateTime NotificationDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //Relations 
        [Required]
        public int TypeId { get; set; }
        public MaintenanceType Type { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}