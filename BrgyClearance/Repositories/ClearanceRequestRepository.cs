using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;

namespace Proj1.Repositories;

public class ClearanceRequestRepository : IClearanceRequestRepository
{
    private readonly ApplicationDbContext _context;

    public ClearanceRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClearanceRequest>> GetAllAsync()
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .ToListAsync();
    }

    public async Task<ClearanceRequest?> GetByIdAsync(int id)
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(ClearanceRequest request)
    {
        _context.ClearanceRequests.Add(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClearanceRequest request)
    {
        _context.ClearanceRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ClearanceRequest>> GetByResidentIdAsync(int residentId)
    {
        return await _context.ClearanceRequests
            .Include(r => r.ClearanceType)
            .Include(r => r.Resident)    
            .Where(r => r.ResidentId == residentId)
            .OrderByDescending(r => r.RequestDate) 
            .ToListAsync();
    }

    public async Task<List<ClearanceRequest>> GetPendingAsync()
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .Where(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.Pending)
            .ToListAsync();
    }
}