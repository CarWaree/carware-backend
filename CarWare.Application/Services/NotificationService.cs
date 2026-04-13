using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFcmService _fcmService;
        private readonly ILogger<Notification> _log;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper
            ,IFcmService fcmService, ILogger<Notification> log)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fcmService = fcmService;
            _log = log;
        }

        public async Task SendAsync(SendNotificationDto dto)
        {
            var entity = _mapper.Map<Notification>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsSent = false;

            await _unitOfWork.NotificationRepository.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            _log.LogInformation("Sending {Channel} notification {Id} to user {UserId}",
                dto.Channel, entity.Id, dto.UserId);

            try
            {
                if (dto.Channel == NotificationChannel.Push)
                {
                    var tokenList = await _unitOfWork.DeviceTokenRepository
                            .GetActiveTokensByUserIdAsync(entity.UserId);

                    if (tokenList.Any())
                        await _fcmService.SendMulticastAsync(tokenList, entity.Title, entity.Body, dto.Data);
                    else
                        _log.LogWarning("No active tokens found for user {UserId}", entity.UserId);
                }

                entity.IsSent = true;
                entity.SentAt = DateTime.UtcNow;
                _unitOfWork.NotificationRepository.Update(entity);
                await _unitOfWork.CompleteAsync();

                _log.LogInformation("Notification {Id} delivered successfully", entity.Id);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send notification {Id}", entity.Id);
                throw;
            }
        }

        public async Task SendMultipleAsync(List<string> userIds, string title,
            string body, NotificationChannel channel)
        {
            if (userIds == null || !userIds.Any())
            {
                _log.LogWarning("SendMultipleAsync called with empty userIds list");
                return;
            }

            var tasks = userIds.Select(userId => SendAsync(new SendNotificationDto
            {
                UserId = userId,
                Title = title,
                Body = body,
                Channel = channel
            }));

            await Task.WhenAll(tasks);
        }

        public async Task BroadcastAsync(string title, string body, NotificationChannel channel)
        {
            var userIds = await _unitOfWork.DeviceTokenRepository
                .GetAllActiveUserIdsAsync();

            _log.LogWarning("Broadcasting to {Count} users via {Channel}", userIds.Count, channel);

            await SendMultipleAsync(userIds, title, body, channel);
        }

        public async Task RegisterTokenAsync(string userId, string token, DevicePlatform platform)
        {
            var existing = await _unitOfWork.DeviceTokenRepository
                .GetByTokenAsync(token);

            if (existing == null)
            {
                await _unitOfWork.DeviceTokenRepository.AddAsync(new DeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = platform,
                    IsActive = true,
                    RegisteredAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.IsActive = true;
                existing.UserId = userId;
                existing.LastUsedAt = DateTime.UtcNow;
                _unitOfWork.DeviceTokenRepository.Update(existing);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveTokenAsync(string token)
        {
            var existing = await _unitOfWork.DeviceTokenRepository
                .GetByTokenAsync(token);

            if (existing == null)
            {
                _log.LogWarning("RemoveToken: token not found");
                return;
            }

            existing.IsActive = false;
            _unitOfWork.DeviceTokenRepository.Update(existing);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<NotificationDetailsDto> GetByIdAsync(int id, string userId)
        {
            var repo = _unitOfWork.Repository<Notification>();

            var notification = await repo.Query()
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                throw new Exception("Notification not found");

            return _mapper.Map<NotificationDetailsDto>(notification);
        }

        public async Task<NotificationListResponse> GetMyNotificationsAsync(string userId, PaginationParameters param)
        {

            var repo = _unitOfWork.Repository<Notification>();

            var query = repo.Query()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((param.PageIndex - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new NotificationListResponse
            {
                Items = _mapper.Map<IReadOnlyList<NotificationDto>>(items),
                Page = param.PageIndex,
                PageSize = param.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var repo = _unitOfWork.Repository<Notification>();

            return await repo.Query()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var repo = _unitOfWork.Repository<Notification>();

            var notifications = await repo.Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkAsReadAsync(int id, string userId)
        {
            var repo = _unitOfWork.Repository<Notification>();

            var notification = await repo.Query()
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                throw new Exception("Notification not found");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                repo.Update(notification);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}