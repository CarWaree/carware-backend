using CarWare.Application.DTOs.Payment;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using CarWare.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CarWare.Application.Common.helper;

namespace CarWare.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentDto> CreateCashPaymentAsync(CreateCashPaymentDto dto, string userId)
        {
            var appointmentRepo = _unitOfWork.Repository<Appointment>();
            var appointment = await appointmentRepo.Query()
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                throw new Exception("Appointment not found");

            var payment = new Payment
            {
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var repo = _unitOfWork.Repository<Payment>();
            await repo.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            return MapToDto(payment);
        }

        public async Task<PaymentDto> ConfirmCashPaymentAsync(int paymentId, string adminId)
        {
            var repo = _unitOfWork.Repository<Payment>();
            var payment = await repo.Query()
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Status != PaymentStatus.Pending)
                throw new Exception("Payment is not in Pending status");

            payment.Status = PaymentStatus.Paid;
            payment.Appointment.Status = AppointmentStatus.Confirmed;

            repo.Update(payment);
            await _unitOfWork.CompleteAsync();

            return MapToDto(payment);
        }

        public async Task<PaymentDto> CancelPaymentAsync(int paymentId, string userId)
        {
            var repo = _unitOfWork.Repository<Payment>();
            var payment = await repo.Query()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new Exception("Payment not found");

            if (payment.Status == PaymentStatus.Paid)
                throw new Exception("Cannot cancel a paid payment");

            payment.Status = PaymentStatus.Cancelled;
            repo.Update(payment);
            await _unitOfWork.CompleteAsync();

            return MapToDto(payment);
        }

        public async Task<PaymentListResponse> GetAllCashPaymentsAsync(PaginationParameters param)
        {
            var repo = _unitOfWork.Repository<Payment>();
            var query = repo.Query()
                .Where(p => p.Method == PaymentMethod.Cash)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((param.PageIndex - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new PaymentListResponse
            {
                Items = items.Select(MapToDto),
                Page = param.PageIndex,
                PageSize = param.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaymentListResponse> GetMyCashPaymentsAsync(string userId, PaginationParameters param)
        {
            var repo = _unitOfWork.Repository<Payment>();
            var query = repo.Query()
                .Include(p => p.Appointment)
                .Where(p => p.Method == PaymentMethod.Cash
                         && p.Appointment.UserId == userId)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((param.PageIndex - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new PaymentListResponse
            {
                Items = items.Select(MapToDto),
                Page = param.PageIndex,
                PageSize = param.PageSize,
                TotalCount = totalCount
            };
        }

        // Helper
        private PaymentDto MapToDto(Payment p) => new()
        {
            PaymentId = p.Id,
            Status = p.Status.ToString(),
            Method = p.Method.ToString(),
            Amount = p.Amount,
            AppointmentId = p.AppointmentId,
            CreatedAt = p.CreatedAt
        };
    }

}
