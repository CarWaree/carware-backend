using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.History;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HistoryService : IHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public HistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<HistoryCardDto>>> GetAllAsync(string userId)
    {
        var data = await _unitOfWork.ServiceHistoryRepository
            .GetUserHistory(userId)
            .Select(x => new HistoryCardDto
            {
                Id = x.Id,
                CarName = x.Vehicle.Brand.Name + " " + x.Vehicle.Model.Name,
                ProviderName = x.ServiceCenter.Name,
                Date = x.CreatedAt,
                PaymentMethod = x.PaymentMethod.ToString(),
                ServiceName = x.ServiceRequestServices
                    .Select(s => s.MaintenanceType.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Result<List<HistoryCardDto>>.Ok(data);
    }

    public async Task<Result<HistoryDetailsDto>> GetByIdAsync(int id, string userId)
    {
        var history = await _unitOfWork.ServiceHistoryRepository
            .GetUserHistory(userId)
            .Where(x => x.Id == id)
            .Select(x => new HistoryDetailsDto
            {
                Id = x.Id,
                CarName = x.Vehicle.Brand + " " + x.Vehicle.Model,
                ProviderName = x.ServiceCenter.Name,
                Date = x.CreatedAt,
                PaymentMethod = x.PaymentMethod.ToString(),

                ServiceName = x.ServiceRequestServices
                    .Select(s => s.MaintenanceType.Name)
                    .FirstOrDefault(),

                ServiceDetails = x.ServiceRequestServices
                    .Select(s => s.Description)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (history == null)
            return Result<HistoryDetailsDto>.Fail("History not found");

        return Result<HistoryDetailsDto>.Ok(history);
    }
}