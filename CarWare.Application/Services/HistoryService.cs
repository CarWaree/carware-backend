using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using CarWare.Application.DTOs.service;
using CarWare.Application.helper;
using CarWare.Application.Interfaces;
using CarWare.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class HistoryService : IHistoryService
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public HistoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<Result<Pagination<HistoryCardDto>>> GetAllAsync(string userId, string status, int pageNumber, int pageSize)
        {
            var query = _context.ServiceRequest
        .Include(x => x.Vehicle)
        .Include(x => x.ServiceCenter)
        .Where(x => x.UserId == userId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => x.Status == status);

            query = query.OrderByDescending(x => x.CreatedAt);

          
            var projectedQuery = query.Select(x => new HistoryCardDto
            {
                Id = x.Id,
                CarName = x.Vehicle.Name,
                ServiceCenterName = x.ServiceCenter.Name,
                TotalPrice = x.TotalPrice,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            });

            var pagedData = await projectedQuery.ToPagedList(pageNumber, pageSize);

            return Result<Pagination<HistoryCardDto>>.Ok(pagedData);
        }
        

       
       public async Task<Result<HistoryDetailsDto>> GetByIdAsync(int id)
        {
            var request = await _context.ServiceRequest
                .Include(x => x.Vehicle)
                .Include(x => x.ServiceCenter)
                .Include(x => x.ServiceRequestServices)
                    .ThenInclude(s => s.MaintenanceType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
                return Result<HistoryDetailsDto>.Fail("Service request not found");

            var dto = new HistoryDetailsDto
            {
                Id = request.Id,
                CarName = request.Vehicle.Name,
                ServiceCenterName = request.ServiceCenter.Name,
                TotalPrice = request.TotalPrice,
                Status = request.Status,
                PaymentMethod = request.PaymentMethod.ToString(),
                PaymentStatus = request.PaymentStatus.ToString(),
                CreatedAt = request.CreatedAt,
                Services = request.ServiceRequestServices
                    .Select(s => new ServiceItemDto
                    {
                        ServiceName = s.MaintenanceType.Name,
                        Description = s.Description
                    })
                    .ToList()
            };

            return Result<HistoryDetailsDto>.Ok(dto);
        }
    }
    
}
