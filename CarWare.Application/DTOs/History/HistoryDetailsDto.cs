using CarWare.Application.DTOs.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.History
{
    public class HistoryDetailsDto
    {
        public int Id { get; set; }
        public string CarName { get; set; }
        public string ServiceCenterName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ServiceItemDto> Services { get; set; }
    }
}
