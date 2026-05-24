using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWare.Application.DTOs.Dashboard.Setup
{
    public class SetupRequestDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required, MinLength(1)]
        public List<int> MaintenanceTypeIds { get; set; } = new();

        public string? OtherServiceName { get; set; }

        [Required]
        public TimeSpan WorkingFrom { get; set; }

        [Required]
        public TimeSpan WorkingTo { get; set; }
    }
}