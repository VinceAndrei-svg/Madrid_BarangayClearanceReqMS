using AutoMapper;
using Microsoft.Extensions.Logging;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;

namespace Proj1.Services;

public class ClearanceRequestService : IClearanceRequestService
{
    private readonly IClearanceRequestRepository _repository;
    private readonly IResidentRepository _residentRepository;
    private readonly IPdfClearanceService _pdfService;
    private readonly IMapper _mapper;
    private readonly ILogger<ClearanceRequestService> _logger;

    public ClearanceRequestService(
        IClearanceRequestRepository repository,
        IResidentRepository residentRepository,
        IPdfClearanceService pdfService,
        IMapper mapper,
        ILogger<ClearanceRequestService> logger)
    {
        _repository = repository;
        _residentRepository = residentRepository;
        _pdfService = pdfService;
        _mapper = mapper;
        _logger = logger;
    }

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

    public async Task<int> CreateAsync(CreateClearanceRequestDto dto)
    {
        var request = _mapper.Map<ClearanceRequest>(dto);

        request.ReferenceNumber = GenerateReferenceNumber();
        request.Status = RequestStatus.Submitted;
        request.RequestDate = DateTime.UtcNow;
        request.IsPaid = false;

        var requestId = await _repository.AddAsync(request);

        _logger.LogInformation(
            "Clearance request created: ID={RequestId}, ResidentId={ResidentId}, Type={TypeId}",
            requestId, dto.ResidentId, dto.ClearanceTypeId);

        return requestId;
    }

    public async Task ProcessAsync(ProcessClearanceRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(dto.Id);

        if (request == null)
        {
            _logger.LogWarning("Process failed: Request {RequestId} not found", dto.Id);
            throw new InvalidOperationException("Request not found.");
        }

        if (request.Status != RequestStatus.Submitted &&
            request.Status != RequestStatus.Pending)
        {
            _logger.LogWarning(
                "Process failed: Request {RequestId} cannot be processed - current status: {Status}",
                dto.Id, request.Status);

            throw new InvalidOperationException("Request cannot be processed in current status.");
        }

        request.Status = dto.Approve
            ? RequestStatus.Approved
            : RequestStatus.Rejected;

        request.ProcessedByUserId = dto.ProcessedByUserId;
        request.ProcessedDate = DateTime.UtcNow;
        request.Remarks = dto.Remarks;

        await _repository.UpdateAsync(request);

        _logger.LogInformation(
            "Request {RequestId} {Action} by {UserId}",
            dto.Id,
            dto.Approve ? "approved" : "rejected",
            dto.ProcessedByUserId);
    }

    public async Task<bool> CancelAsync(int requestId, string userId, string reason)
    {
        var request = await _repository.GetByIdAsync(requestId);

        if (request == null)
        {
            _logger.LogWarning("Cancel failed: Request {RequestId} not found", requestId);
            return false;
        }

        var resident = await _residentRepository.GetByUserIdAsync(userId);

        if (resident == null || request.ResidentId != resident.Id)
        {
            _logger.LogWarning(
                "Cancel failed: User {UserId} does not own request {RequestId}",
                userId, requestId);
            return false;
        }

        if (request.Status != RequestStatus.Submitted &&
            request.Status != RequestStatus.Pending)
        {
            _logger.LogWarning(
                "Cancel failed: Request {RequestId} cannot be cancelled - current status: {Status}",
                requestId, request.Status);
            return false;
        }

        request.Status = RequestStatus.Cancelled;
        request.CancelledBy = userId;
        request.CancelledDate = DateTime.UtcNow;
        request.CancellationReason = reason;

        await _repository.UpdateAsync(request);

        _logger.LogInformation(
            "Request {RequestId} cancelled by resident {ResidentId}. Reason: {Reason}",
            requestId, resident.Id, reason);

        return true;
    }

    public async Task<bool> RecordPaymentAsync(int requestId, string staffUserId)
    {
        var request = await _repository.GetByIdAsync(requestId);

        if (request == null)
        {
            _logger.LogWarning("Payment record failed: Request {RequestId} not found", requestId);
            return false;
        }

        if (request.Status != RequestStatus.Approved)
        {
            _logger.LogWarning(
                "Payment record failed: Request {RequestId} not in Approved status - current: {Status}",
                requestId, request.Status);
            return false;
        }

        request.IsPaid = true;
        request.PaidDate = DateTime.UtcNow;
        request.CollectedByUserId = staffUserId;
        request.Status = RequestStatus.ForRelease;

        await _repository.UpdateAsync(request);

        _logger.LogInformation(
            "Payment recorded for request {RequestId} by staff {UserId}",
            requestId, staffUserId);

        return true;
    }

    public async Task<bool> MarkAsReleasedAsync(int requestId, string staffUserId)
    {
        var request = await _repository.GetByIdAsync(requestId);

        if (request == null)
        {
            _logger.LogWarning("Release failed: Request {RequestId} not found", requestId);
            return false;
        }

        if (request.Status != RequestStatus.ForRelease)
        {
            _logger.LogWarning(
                "Release failed: Request {RequestId} not in ForRelease status - current: {Status}",
                requestId, request.Status);
            return false;
        }

        request.Status = RequestStatus.Released;
        request.ReleasedDate = DateTime.UtcNow;
        request.ExpiryDate = DateTime.UtcNow.AddMonths(6);
        request.ProcessedByUserId = staffUserId;

        await _repository.UpdateAsync(request);

        _logger.LogInformation(
            "Request {RequestId} marked as released by staff {UserId}. Expires: {ExpiryDate}",
            requestId, staffUserId, request.ExpiryDate);

        try
        {
            await _pdfService.GenerateClearancePdfAsync(requestId);

            _logger.LogInformation(
                "PDF generated successfully for request {RequestId}",
                requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "PDF generation failed for request {RequestId}",
                requestId);
        }

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

            _logger.LogInformation(
                "Marked {Count} requests as expired",
                expiredRequests.Count);
        }
    }

    private string GenerateReferenceNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"CLR-{datePart}-{randomPart}";
    }
}
