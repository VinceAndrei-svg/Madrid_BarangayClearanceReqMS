using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;

namespace Proj1.Repositories;

/// <summary>
/// Repository for clearance request data access.
/// Uses eager loading to include related entities.
/// </summary>
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
            .OrderByDescending(r => r.RequestDate)  // Latest first
            .ToListAsync();
    }

    public async Task<ClearanceRequest?> GetByIdAsync(int id)
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .FirstOrDefaultAsync(r => r.Id == id);
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
            .OrderBy(r => r.RequestDate)  // Oldest first for processing
            .ToListAsync();
    }

    public async Task<List<ClearanceRequest>> GetByStatusAsync(RequestStatus status)
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<ClearanceRequest?> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await _context.ClearanceRequests
            .Include(r => r.Resident)
            .Include(r => r.ClearanceType)
            .FirstOrDefaultAsync(r => r.ReferenceNumber == referenceNumber);
    }

    public async Task<List<ClearanceRequest>> GetExpiredAsync()
    {
        return await _context.ClearanceRequests
            .Where(r => r.Status == RequestStatus.Released 
                        && r.ExpiryDate.HasValue 
                        && r.ExpiryDate.Value < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> AddAsync(ClearanceRequest request)
    {
        _context.ClearanceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request.Id;  // Return the generated ID
    }

    public async Task UpdateAsync(ClearanceRequest request)
    {
        _context.ClearanceRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}