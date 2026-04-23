using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MaintenanceReminderJob
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public MaintenanceReminderJob(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task Execute()
    {
        var now = DateTime.UtcNow;

        var reminders = await _uow.MaintenanceRepository.GetDueRemindersAsync(now);

        if (!reminders.Any())
            return;

        foreach (var reminder in reminders)
        {
            try
            {
                await _notificationService.SendAsync(new SendNotificationDto
                {
                    UserId = reminder.Vehicle.UserId,
                    Title = "Maintenance Reminder",
                    Body = reminder.Note ?? "You have maintenance",
                    Channel = NotificationChannel.Push
                });

                reminder.IsNotified = true;
            }
            catch
            {
                // log error if needed
            }
        }

        await _uow.CompleteAsync();
    }
}