using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj1.Persons;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IResidentRepository _residentRepository;

    public AccountService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IResidentRepository residentRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _residentRepository = residentRepository;
    }

    public async Task<ServiceResult> RegisterResidentAsync(RegisterResidentDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Resident);

        var resident = new Resident
        {
            UserId = user.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            BirthDate = dto.BirthDate
        };

        await _residentRepository.AddAsync(resident);

        return ServiceResult.Success("Registration successful");
    }

    public async Task<ServiceResult> LoginAsync(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.Email,
            dto.Password,
            dto.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return ServiceResult.Failure("Invalid login attempt.");

        return ServiceResult.Success();
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<ServiceResult> CreateStaffAsync(CreateStaffDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Staff);

        return ServiceResult.Success($"Staff account created for {dto.Email}");
    }

    public async Task<ServiceResult<List<StaffListDto>>> GetStaffListAsync()
    {
        var staffUsers = await _userManager.GetUsersInRoleAsync(Roles.Staff);

        var staffList = staffUsers.Select(u => new StaffListDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow,
            LockoutEnd = u.LockoutEnd
        }).OrderBy(s => s.Email).ToList();

        return ServiceResult<List<StaffListDto>>.Success(staffList);
    }

    public async Task<ServiceResult> ToggleStaffStatusAsync(ToggleStaffStatusDto dto)
    {
        // Find the staff member
        var user = await _userManager.FindByIdAsync(dto.StaffId);
        
        if (user == null)
            return ServiceResult.Failure("Staff member not found");

        // Prevent users from disabling themselves
        if (dto.StaffId == dto.CurrentUserId)
            return ServiceResult.Failure("You cannot change your own status");

        // Prevent staff from toggling admin accounts
        var targetUserRoles = await _userManager.GetRolesAsync(user);
        if (targetUserRoles.Contains(Roles.Admin) && !dto.IsCurrentUserAdmin)
            return ServiceResult.Failure("You do not have permission to modify administrator accounts");

        // Toggle the status
        var isCurrentlyActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        if (isCurrentlyActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return ServiceResult.Success($"{user.Email} has been deactivated");
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            return ServiceResult.Success($"{user.Email} has been activated");
        }
    }
}