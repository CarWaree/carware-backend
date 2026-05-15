using AutoMapper;
using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFcmService _fcmService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IFcmService fcmService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task SendAsync(
            SendNotificationDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<Notification>(dto);

            entity.CreatedAt = DateTime.UtcNow;
            entity.IsSent = false;

            await _unitOfWork.NotificationRepository
                .AddAsync(entity);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation(
                "Sending {Channel} notification {NotificationId} to user {UserId}",
                dto.Channel,
                entity.Id,
                dto.UserId);

            try
            {
                if (dto.Channel == NotificationChannel.Push)
                {
                    var tokenList = await _unitOfWork.DeviceTokenRepository
                        .GetActiveTokensByUserIdAsync(entity.UserId);

                    if (tokenList.Any())
                    {
                        await _fcmService.SendMulticastAsync(
                            tokenList,
                            entity.Title,
                            entity.Body,
                            dto.Data);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "No active tokens found for user {UserId}",
                            entity.UserId);
                    }
                }

                entity.IsSent = true;
                entity.SentAt = DateTime.UtcNow;

                _unitOfWork.NotificationRepository.Update(entity);

                await _unitOfWork.CompleteAsync(cancellationToken);

                _logger.LogInformation(
                    "Notification {NotificationId} delivered successfully",
                    entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send notification {NotificationId}",
                    entity.Id);

                throw;
            }
        }

        public async Task SendMultipleAsync(
            List<string> userIds,
            string title,
            string body,
            NotificationChannel channel,
            CancellationToken cancellationToken = default)
        {
            if (userIds == null || !userIds.Any())
            {
                _logger.LogWarning(
                    "SendMultipleAsync called with empty userIds list");

                return;
            }

            const int batchSize = 100;

            foreach (var batch in userIds.Chunk(batchSize))
            {
                var tasks = batch.Select(userId =>
                    SendAsync(
                        new SendNotificationDto
                        {
                            UserId = userId,
                            Title = title,
                            Body = body,
                            Channel = channel
                        },
                        cancellationToken));

                await Task.WhenAll(tasks);
            }
        }

        public async Task BroadcastAsync(
            string title,
            string body,
            NotificationChannel channel,
            CancellationToken cancellationToken = default)
        {
            var userIds = await _unitOfWork.DeviceTokenRepository
                .GetAllActiveUserIdsAsync();

            _logger.LogInformation(
                "Broadcasting notification to {Count} users via {Channel}",
                userIds.Count,
                channel);

            await SendMultipleAsync(
                userIds,
                title,
                body,
                channel,
                cancellationToken);
        }

        public async Task RegisterTokenAsync(
            string userId,
            string token,
            DevicePlatform platform,
            CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.DeviceTokenRepository
                .GetByTokenAsync(token);

            if (existing == null)
            {
                var deviceToken = new DeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = platform,
                    IsActive = true,
                    RegisteredAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow
                };

                await _unitOfWork.DeviceTokenRepository
                    .AddAsync(deviceToken);
            }
            else
            {
                existing.IsActive = true;
                existing.UserId = userId;
                existing.LastUsedAt = DateTime.UtcNow;

                _unitOfWork.DeviceTokenRepository
                    .Update(existing);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public async Task RemoveTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.DeviceTokenRepository
                .GetByTokenAsync(token);

            if (existing == null)
            {
                _logger.LogWarning(
                    "RemoveTokenAsync: token not found");

                return;
            }

            existing.IsActive = false;

            _unitOfWork.DeviceTokenRepository
                .Update(existing);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public async Task<NotificationDetailsDto> GetByIdAsync(
            int id,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _unitOfWork.NotificationRepository
                .GetByIdAndUserIdAsync(
                    id,
                    userId,
                    cancellationToken);

            if (notification == null)
            {
                throw new Exception("Notification not found");
            }

            return _mapper.Map<NotificationDetailsDto>(notification);
        }

        public async Task<NotificationListResponse> GetMyNotificationsAsync(
            string userId,
            PaginationParameters param,
            CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.NotificationRepository
                .GetUserNotificationsAsync(
                    userId,
                    param.PageIndex,
                    param.PageSize,
                    cancellationToken);

            var totalCount = await _unitOfWork.NotificationRepository
                .GetUserNotificationsCountAsync(
                    userId,
                    cancellationToken);

            return new NotificationListResponse
            {
                Items = _mapper.Map<IReadOnlyList<NotificationDto>>(items),
                Page = param.PageIndex,
                PageSize = param.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetUnreadCountAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.NotificationRepository
                .GetUnreadCountAsync(
                    userId,
                    cancellationToken);
        }

        public async Task MarkAllAsReadAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var notifications = await _unitOfWork.NotificationRepository
                .GetUnreadNotificationsAsync(
                    userId,
                    cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(
            int id,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _unitOfWork.NotificationRepository
                .GetByIdAndUserIdAsync(
                    id,
                    userId,
                    cancellationToken);

            if (notification == null)
            {
                throw new Exception("Notification not found");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                _unitOfWork.NotificationRepository
                    .Update(notification);

                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }
    }
}