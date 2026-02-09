using Proj1.DTOs;

namespace Proj1.Interfaces;

public interface IAccountService
{
    Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterResidentAsync(RegisterResidentDto dto);
    Task<(bool Succeeded, string? Error)> LoginAsync(LoginDto dto);

    Task LogoutAsync();

    Task<(bool Succeeded, IEnumerable<string> Errors)> CreateStaffAsync(CreateStaffDto dto);
}