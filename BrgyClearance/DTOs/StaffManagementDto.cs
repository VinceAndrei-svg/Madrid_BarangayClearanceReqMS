namespace Proj1.DTOs;

public class StaffListDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class ToggleStaffStatusDto
{
    public string StaffId { get; set; } = string.Empty;
    public string CurrentUserId { get; set; } = string.Empty;
    public bool IsCurrentUserAdmin { get; set; }
}