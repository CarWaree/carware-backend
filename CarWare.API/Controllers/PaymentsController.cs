using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Payment;
using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST /api/payments/cash
        [HttpPost("cash")]
        public async Task<IActionResult> CreateCashPayment([FromBody] CreateCashPaymentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.CreateCashPaymentAsync(dto, userId);
            return Ok(result);
        }

        // PUT /api/payments/{id}/confirm-cash (Admin only)
        [HttpPut("{id:int}/confirm-cash")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmCashPayment(int id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.ConfirmCashPaymentAsync(id, adminId);
            return Ok(result);
        }

        // PUT /api/payments/{id}/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.CancelPaymentAsync(id, userId);
            return Ok(result);
        }

        // GET /api/payments/cash (Admin only)
        [HttpGet("cash")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCashPayments([FromQuery] PaginationParameters param)
        {
            var result = await _paymentService.GetAllCashPaymentsAsync(param);
            return Ok(result);
        }

        // GET /api/payments/my-cash
        [HttpGet("my-cash")]
        public async Task<IActionResult> GetMyCashPayments([FromQuery] PaginationParameters param)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _paymentService.GetMyCashPaymentsAsync(userId, param);
            return Ok(result);
        }
    }
}
