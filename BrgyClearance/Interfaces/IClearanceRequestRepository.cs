using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;

namespace Proj1.Interfaces;

/// <summary>
/// Repository interface for clearance request data access.
/// </summary>
public interface IClearanceRequestRepository
{
    // === READ OPERATIONS ===
    Task<List<ClearanceRequest>> GetAllAsync();
    Task<ClearanceRequest?> GetByIdAsync(int id);
    Task<List<ClearanceRequest>> GetByResidentIdAsync(int residentId);
    Task<List<ClearanceRequest>> GetPendingAsync();
    Task<List<ClearanceRequest>> GetByStatusAsync(RequestStatus status);
    Task<ClearanceRequest?> GetByReferenceNumberAsync(string referenceNumber);
    
    /// <summary>
    /// Get clearances that are Released but past expiry date
    /// </summary>
    Task<List<ClearanceRequest>> GetExpiredAsync();
    
    // === WRITE OPERATIONS ===
    Task<int> AddAsync(ClearanceRequest request);
    Task UpdateAsync(ClearanceRequest request);
    Task SaveChangesAsync();
}