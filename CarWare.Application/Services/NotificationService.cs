using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.Common.helper;
using CarWare.Application.DTOs.Notification;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
