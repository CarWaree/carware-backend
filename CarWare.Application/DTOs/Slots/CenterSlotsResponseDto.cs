using System.Collections.Generic;

namespace CarWare.Application.DTOs.Slots
{
    public class CenterSlotsResponseDto
    {
        public int ServiceCenterId { get; set; }
        public string CenterName { get; set; }
        public List<SlotDto> Slots { get; set; } = new();
    }
}