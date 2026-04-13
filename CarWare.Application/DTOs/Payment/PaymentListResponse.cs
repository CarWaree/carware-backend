using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.DTOs.Payment
{
    public class PaymentListResponse
    {
        public IEnumerable<PaymentDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

}
