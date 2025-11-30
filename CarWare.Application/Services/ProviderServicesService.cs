using AutoMapper;
using CarWare.Application.Common;
using CarWare.Application.DTOs.Maintenance;
using CarWare.Application.Interfaces;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Infrastructure.Context;
using CarWare.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Services
{
    public class ProviderServicesService : IProviderServicesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ProviderServicesService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }
        public async  Task<Result<IEnumerable<ProviderServiceDto>>> GetAllAsync()
        {
            var data = await _context.ProviderServices
         .Include(x => x.ServiceCenter)
         .Include(x => x.Service)
         .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ProviderServiceDto>>(data);

            return Result<IEnumerable<ProviderServiceDto>>.Ok(mapped);
        }
        public async Task<Result<IEnumerable<ProviderServiceDto>>> GetProvidersByServiceTypeAsync(int serviceTypeId)
        {
            var data = await _context.ProviderServices
                .Include(x => x.ServiceCenter)
                .Include(x => x.Service)
                .Where(x => x.ServiceId == serviceTypeId) 
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ProviderServiceDto>>(data);

            return Result<IEnumerable<ProviderServiceDto>>.Ok(mapped);
        }
    }
}
