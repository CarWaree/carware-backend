using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using System.Threading.Tasks;

public class NotificationJobs
{
    private readonly INotificationService _notificationService;

    public NotificationJobs(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Send(SendNotificationDto dto)
    {
        await _notificationService.SendAsync(dto);
    }
}