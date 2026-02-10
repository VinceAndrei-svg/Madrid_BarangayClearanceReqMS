using Proj1.Models.Entities;

namespace Proj1.Interfaces;

public interface IClearanceRequestRepository
{
    Task<List<ClearanceRequest>> GetAllAsync();
    Task<ClearanceRequest?> GetByIdAsync(int id);
    Task AddAsync(ClearanceRequest request);
    Task UpdateAsync(ClearanceRequest request);

    Task<List<ClearanceRequest>> GetByResidentIdAsync(int residentId);
    Task<List<ClearanceRequest>> GetPendingAsync();
}