using Proj1.DTOs;
using Proj1.Models.Common;

namespace Proj1.Interfaces;

public interface IResidentService
{
    Task<List<ResidentDto>> GetAllAsync();
    Task<List<ResidentDto>> SearchAsync(string search);
    Task<ResidentDto?> GetByIdAsync(int id);
    Task CreateAsync(CreateResidentDto dto);
    Task<bool> UpdateAsync(UpdateResidentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<ResidentDto?> GetByUserIdAsync(string userId);

    Task<PagedResult<ResidentDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sort,
        int? minAge,
        int? maxAge);
}