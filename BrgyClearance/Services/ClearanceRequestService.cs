using AutoMapper;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;

namespace Proj1.Services;

/// <summary>
/// Service for managing clearance request business logic.
/// Orchestrates repository calls and enforces business rules.
/// </summary>
public class ClearanceRequestService : IClearanceRequestService
{
    private readonly IClearanceRequestRepository _repository;
    private readonly IResidentRepository _residentRepository;
    private readonly IMapper _mapper;

    public ClearanceRequestService(
        IClearanceRequestRepository repository,
        IResidentRepository residentRepository,
        IMapper mapper)
    {
        _repository = repository;
        _residentRepository = residentRepository;
        _mapper = mapper;
    }

    // === READ OPERATIONS ===

    public async Task<List<ClearanceRequestDto>> GetAllAsync()
    {
        var requests = await _repository.GetAllAsync();
        return _mapper.Map<List<ClearanceRequestDto>>(requests);
    }

    public async Task<ClearanceRequestDto?> GetByIdAsync(int id)
    {
        var request = await _repository.GetByIdAsync(id);
        return request == null ? null : _mapper.Map<ClearanceRequestDto>(request);
    }

    public async Task<List<ClearanceRequestDto>> GetByResidentIdAsync(int residentId)
    {
        var requests = await _repository.GetByResidentIdAsync(residentId);
        return _mapper.Map<List<ClearanceRequestDto>>(requests);
    }

    public async Task<List<ClearanceRequestDto>> GetPendingAsync()
    {
        var requests = await _repository.GetPendingAsync();
        return _mapper.Map<List<ClearanceRequestDto>>(requests);
    }

    public async Task<List<ClearanceRequestDto>> GetByStatusAsync(RequestStatus status)
    {
        var requests = await _repository.GetByStatusAsync(status);
        return _mapper.Map<List<ClearanceRequestDto>>(requests);
    }

    // === CREATE OPERATIONS ===

    public async Task<int> CreateAsync(CreateClearanceRequestDto dto)
    {
        var request = _mapper.Map<ClearanceRequest>(dto);
        
        // Generate unique reference number
        request.ReferenceNumber = GenerateReferenceNumber();
        request.Status = RequestStatus.Submitted;
        request.RequestDate = DateTime.UtcNow;
        
        return await _repository.AddAsync(request);
    }

    // === WORKFLOW OPERATIONS ===

    public async Task ProcessAsync(ProcessClearanceRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(dto.Id);
        if (request == null) 
            throw new InvalidOperationException("Request not found.");

        // Business rule: Can only process Submitted or Pending requests
        if (request.Status != RequestStatus.Submitted && request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Request cannot be processed in current status.");

        request.Status = dto.Approve ? RequestStatus.Approved : RequestStatus.Rejected;
        request.ProcessedByUserId = dto.ProcessedByUserId;
        request.ProcessedDate = DateTime.UtcNow;
        request.Remarks = dto.Remarks;

        await _repository.UpdateAsync(request);
    }

    public async Task<bool> CancelAsync(int requestId, string userId, string reason)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null) return false;

        // Verify resident owns this request
        var resident = await _residentRepository.GetByUserIdAsync(userId);
        if (resident == null || request.ResidentId != resident.Id) 
            return false;

        // Business rule: Can only cancel Submitted or Pending requests
        if (request.Status != RequestStatus.Submitted && request.Status != RequestStatus.Pending)
            return false;

        request.Status = RequestStatus.Cancelled;
        request.CancelledBy = userId;
        request.CancelledDate = DateTime.UtcNow;
        request.CancellationReason = reason;

        await _repository.UpdateAsync(request);
        return true;
    }

    public async Task<bool> RecordPaymentAsync(int requestId, string staffUserId)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null) return false;

        // Business rule: Can only record payment for Approved requests
        if (request.Status != RequestStatus.Approved)
            return false;

        request.IsPaid = true;
        request.PaidDate = DateTime.UtcNow;
        request.CollectedByUserId = staffUserId;
        request.Status = RequestStatus.ForRelease;

        await _repository.UpdateAsync(request);
        return true;
    }

    public async Task<bool> MarkAsReleasedAsync(int requestId, string staffUserId)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null) return false;

        // Business rule: Can only release if ForRelease
        if (request.Status != RequestStatus.ForRelease)
            return false;

        request.Status = RequestStatus.Released;
        request.ReleasedDate = DateTime.UtcNow;
        request.ExpiryDate = DateTime.UtcNow.AddMonths(6); // 6 months validity

        await _repository.UpdateAsync(request);
        return true;
    }

    public async Task MarkExpiredAsync()
    {
        var expiredRequests = await _repository.GetExpiredAsync();
        
        foreach (var request in expiredRequests)
        {
            request.Status = RequestStatus.Expired;
        }

        if (expiredRequests.Any())
        {
            await _repository.SaveChangesAsync();
        }
    }

    // === PRIVATE HELPERS ===

    /// <summary>
    /// Generates a unique reference number.
    /// Format: CLR-YYYYMMDD-XXXXXXXX
    /// Example: CLR-20250212-A3B4C5D6
    /// </summary>
    private string GenerateReferenceNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"CLR-{datePart}-{randomPart}";
    }
}