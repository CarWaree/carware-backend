using System;

namespace CarWare.Application.DTOs.ServiceRequests
{
    public class AcceptServiceRequestDto
    {
        public string TechnicianId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
    }
}