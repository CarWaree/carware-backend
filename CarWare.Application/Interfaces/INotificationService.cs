using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(SendNotificationDto dto);

        Task SendMultipleAsync(List<string> userIds, string title, string body, NotificationChannel channel);

        Task BroadcastAsync(string title, string body, NotificationChannel channel);

        Task<NotificationListResponse> GetMyNotificationsAsync(string userId, PaginationParameters param);

        Task<NotificationDetailsDto> GetByIdAsync(int  id, string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAsReadAsync(int id, string userId);

        Task MarkAllAsReadAsync(string userId);

        Task RegisterTokenAsync(string userId, string token, DevicePlatform platform);

        Task RemoveTokenAsync(string token);
    }
}