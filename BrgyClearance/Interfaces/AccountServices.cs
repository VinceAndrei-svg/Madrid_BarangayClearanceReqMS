using Proj1.DTOs;

namespace Proj1.Interfaces;

public interface IAccountService
{
    Task<ServiceResult> RegisterResidentAsync(RegisterResidentDto dto);
    Task<ServiceResult> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<ServiceResult> CreateStaffAsync(CreateStaffDto dto);
    Task<ServiceResult<List<StaffListDto>>> GetStaffListAsync();
    Task<ServiceResult> ToggleStaffStatusAsync(ToggleStaffStatusDto dto);
}