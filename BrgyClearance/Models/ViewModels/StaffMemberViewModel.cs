namespace Proj1.Models.ViewModels;

public class StaffMemberViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}