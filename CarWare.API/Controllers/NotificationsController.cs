using CarWare.Application.Interfaces;
using CarWare.Domain.helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        //  Get My Notifications (Paginated)
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationParameters param)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _notificationService.GetMyNotificationsAsync(userId, param);

            return Ok(result);
        }

        //  Get Notification By Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _notificationService.GetByIdAsync(id, userId);

            return Ok(result);
        }

        //  Get Unread Count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(count);
        }

        //  Mark As Read
        [HttpPut("{id:int}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok(new { message = "Notification marked as read" });
        }

        //  Mark All As Read
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All notifications marked as read" });
        }
    }
}
