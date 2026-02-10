using Proj1.Models.Entities;

namespace Proj1.Interfaces;

public interface IResidentRepository
{
    Task<List<Resident>> GetAllAsync();
    Task<List<Resident>> SearchAsync(string search);
    Task<Resident?> GetByIdAsync(int id);
    Task AddAsync(Resident resident);
    Task UpdateAsync(Resident resident);
    Task DeleteAsync(Resident resident);


    Task<(List<Resident> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sort,
        int? minAge,
        int? maxAge);    
}