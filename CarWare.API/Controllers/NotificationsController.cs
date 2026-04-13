using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using CarWare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        [HttpGet("test-fcm")]
        public async Task<IActionResult> TestFcm()
        {
            var userID = GetUserId();
            var dto = new SendNotificationDto
            {
                UserId = userID,
                Title = "Test 🔥",
                Body = "Hello from backend",
                Channel = NotificationChannel.Push
            };

            await _notificationService.SendAsync(dto);

            return Ok("Sent");
        }

        [HttpPost("send")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
        {
            await _notificationService.SendAsync(dto);
            return Ok(new { message = "Notification sent successfully" });
        }

        [HttpPost("send-multiple")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendMultiple([FromBody] SendMultipleDto dto)
        {
            await _notificationService.SendMultipleAsync(
                dto.UserIds,
                dto.Title,
                dto.Body,
                dto.Channel);

            return Ok(new { message = "Notifications sent successfully" });
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastDto dto)
        {
            await _notificationService.BroadcastAsync(
                dto.Title,
                dto.Body,
                dto.Channel);

            return Ok(new { message = "Broadcast sent successfully" });
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenDto dto)
        {
            var userId = GetUserId();

            await _notificationService.RegisterTokenAsync(
                userId,
                dto.Token,
                dto.Platform);

            return Ok(new { message = "Token registered successfully" });
        }

        [HttpDelete("remove-token")]
        public async Task<IActionResult> RemoveToken([FromQuery] string token)
        {
            await _notificationService.RemoveTokenAsync(token);
            return Ok(new { message = "Token removed successfully" });
        }

        //  Get My Notifications (Paginated)
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationParameters param)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetMyNotificationsAsync(userId, param);

            return Ok(result);
        }

        //  Get Notification By Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetByIdAsync(id, userId);

            return Ok(result);
        }

        //  Get Unread Count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(count);
        }

        //  Mark As Read
        [HttpPut("{id:int}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok(new { message = "Notification marked as read" });
        }

        //  Mark All As Read
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All notifications marked as read" });
        }
    }
}