using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreateCashPaymentAsync(CreateCashPaymentDto dto, string userId);
        Task<PaymentDto> ConfirmCashPaymentAsync(int paymentId, string adminId);
        Task<PaymentDto> CancelPaymentAsync(int paymentId, string userId);
        Task<PaymentListResponse> GetAllCashPaymentsAsync(PaginationParameters param);
        Task<PaymentListResponse> GetMyCashPaymentsAsync(string userId, PaginationParameters param);
    }
}
