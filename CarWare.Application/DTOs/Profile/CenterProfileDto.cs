using CarWare.Application.DTOs.Maintenance;
using System;
using System.Collections.Generic;

namespace CarWare.Application.DTOs.Dashboard.Profile
{
    public class CenterProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TimeSpan WorkingFrom { get; set; }   
        public TimeSpan WorkingTo { get; set; }     

        public List<MaintenanceTypeDto> Services { get; set; } = new();
    }
}