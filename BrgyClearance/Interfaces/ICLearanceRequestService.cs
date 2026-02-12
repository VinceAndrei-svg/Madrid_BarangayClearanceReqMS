using Proj1.DTOs;
using Proj1.Models.Common.Enums;

namespace Proj1.Interfaces;

/// <summary>
/// Service interface for managing clearance requests.
/// Handles business logic for the clearance request lifecycle.
/// </summary>
public interface IClearanceRequestService
{
    // === READ OPERATIONS ===
    Task<List<ClearanceRequestDto>> GetAllAsync();
    Task<ClearanceRequestDto?> GetByIdAsync(int id);
    Task<List<ClearanceRequestDto>> GetByResidentIdAsync(int residentId);
    Task<List<ClearanceRequestDto>> GetPendingAsync();
    Task<List<ClearanceRequestDto>> GetByStatusAsync(RequestStatus status);
    
    // === CREATE OPERATIONS ===
    Task<int> CreateAsync(CreateClearanceRequestDto dto);
    
    // === WORKFLOW OPERATIONS ===
    /// <summary>
    /// Approve or reject a clearance request
    /// </summary>
    Task ProcessAsync(ProcessClearanceRequestDto dto);
    
    /// <summary>
    /// Cancel a request (can only cancel if Submitted or Pending)
    /// </summary>
    Task<bool> CancelAsync(int requestId, string userId, string reason);
    
    /// <summary>
    /// Record payment and mark request as ForRelease
    /// </summary>
    Task<bool> RecordPaymentAsync(int requestId, string staffUserId);
    
    /// <summary>
    /// Mark clearance as released and set expiry date
    /// </summary>
    Task<bool> MarkAsReleasedAsync(int requestId, string staffUserId);
    
    /// <summary>
    /// Check and mark expired clearances (background job)
    /// </summary>
    Task MarkExpiredAsync();
}