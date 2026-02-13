using Proj1.DTOs;

namespace Proj1.Models.ViewModels;

/// <summary>
/// ViewModel for the audit log list page
/// </summary>
public class AuditLogListViewModel
{
    public PagedAuditLogsDto Logs { get; set; } = new();
    
    // Filter options
    public List<UserSelectItem> AvailableUsers { get; set; } = new();
    public List<string> AvailableEntityTypes { get; set; } = new();
    public List<string> AvailableActions { get; set; } = new();
}

/// <summary>
/// Simple user item for select dropdowns
/// </summary>
public class UserSelectItem
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for entity-specific audit history
/// </summary>
public class EntityAuditHistoryViewModel
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public List<AuditLogItemDto> Logs { get; set; } = new();
}