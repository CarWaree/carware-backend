using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public int AppointmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
