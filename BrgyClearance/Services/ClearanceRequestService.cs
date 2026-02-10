using AutoMapper;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Entities;
using Proj1.Models.Common.Enums;


namespace Proj1.Services;

public class ClearanceRequestService : IClearanceRequestService
{
    private readonly IClearanceRequestRepository _repository;
    private readonly IMapper _mapper;

    public ClearanceRequestService(IClearanceRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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

    public async Task CreateAsync(CreateClearanceRequestDto dto)
    {
        var request = _mapper.Map<ClearanceRequest>(dto);
        request.ReferenceNumber = $"CLR-{DateTime.UtcNow:yyyyMMddHHmmss}";
        request.Status = RequestStatus.Submitted;

        await _repository.AddAsync(request);
    }

    public async Task ProcessAsync(ProcessClearanceRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(dto.Id);
        if (request == null) return;

        request.Status = dto.Approve ? RequestStatus.Approved : RequestStatus.Rejected;
        request.ProcessedByUserId = dto.ProcessedByUserId;
        request.ProcessedDate = DateTime.UtcNow;
        request.Remarks = dto.Remarks;

        await _repository.UpdateAsync(request);
    }
}