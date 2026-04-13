using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Notification?> GetByIdForUserAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
}