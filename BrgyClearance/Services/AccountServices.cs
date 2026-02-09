using Microsoft.AspNetCore.Identity;
using Proj1.Persons;
using Proj1.DTOs;
using Proj1.Interfaces;

namespace Proj1.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterResidentAsync(RegisterResidentDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Resident);

        return (true, Enumerable.Empty<string>());
    }

    public async Task<(bool Succeeded, string? Error)> LoginAsync(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.Email,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return (false, "Invalid login attempt.");

        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateStaffAsync(CreateStaffDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Staff);

        return (true, Enumerable.Empty<string>());
    }
}