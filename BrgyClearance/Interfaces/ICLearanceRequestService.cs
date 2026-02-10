using Proj1.DTOs;

namespace Proj1.Interfaces;

public interface IClearanceRequestService
{
    Task<List<ClearanceRequestDto>> GetAllAsync();
    Task<ClearanceRequestDto?> GetByIdAsync(int id);
    Task<List<ClearanceRequestDto>> GetByResidentIdAsync(int residentId);
    Task<List<ClearanceRequestDto>> GetPendingAsync();

    Task CreateAsync(CreateClearanceRequestDto dto);
    Task ProcessAsync(ProcessClearanceRequestDto dto);
}