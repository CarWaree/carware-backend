using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWare.Domain.Entities
{
    [Table("Slots")]
    public class Slot : BaseEntity
    {
        public int ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; }

        public DayOfWeek DayOfWeek { get; set; } 
        public TimeSpan StartTime { get; set; }     
        public TimeSpan EndTime { get; set; }   
        public bool IsActive { get; set; } = true;
    }
}