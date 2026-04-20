using System;

namespace CarWare.Application.DTOs.History
{
    public class HistoryCardDto
    {
        public int Id { get; set; }

        public string CarName { get; set; }

        public string ServiceName { get; set; }

        public string ProviderName { get; set; }

        public ServiceRequestStatus Status { get; set; }

        public DateTime Date { get; set; }

        public decimal TotalPrice { get; set; }

        public string PaymentMethod { get; set; }
    }
}
