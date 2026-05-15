using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(
            SendNotificationDto dto,
            CancellationToken cancellationToken = default);

        Task SendMultipleAsync(
            List<string> userIds,
            string title,
            string body,
            NotificationChannel channel,
            CancellationToken cancellationToken = default);

        Task BroadcastAsync(
            string title,
            string body,
            NotificationChannel channel,
            CancellationToken cancellationToken = default);

        Task<NotificationListResponse> GetMyNotificationsAsync(
            string userId,
            PaginationParameters param,
            CancellationToken cancellationToken = default);

        Task<NotificationDetailsDto> GetByIdAsync(
            int id,
            string userId,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task MarkAsReadAsync(
            int id,
            string userId,
            CancellationToken cancellationToken = default);

        Task MarkAllAsReadAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task RegisterTokenAsync(
            string userId,
            string token,
            DevicePlatform platform,
            CancellationToken cancellationToken = default);

        Task RemoveTokenAsync(
            string token,
            CancellationToken cancellationToken = default);
    }
}