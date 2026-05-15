using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(
            string userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

    Task<int> GetUserNotificationsCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAndUserIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken = default);

    Task<List<Notification>> GetUnreadNotificationsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}