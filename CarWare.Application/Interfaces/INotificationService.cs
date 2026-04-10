using CarWare.Application.Common;
using CarWare.Application.DTOs.Notification;
using CarWare.Domain.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationListResponse> GetMyNotificationsAsync(string userId, PaginationParameters param);

        Task<NotificationDetailsDto> GetByIdAsync(int  id, string userId);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAsReadAsync(int id, string userId);

        Task MarkAllAsReadAsync(string userId);
    }
}
