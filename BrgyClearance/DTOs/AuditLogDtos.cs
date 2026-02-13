namespace Proj1.DTOs;

/// <summary>
/// Data transfer object for creating audit log entries
/// </summary>
public class AuditLogDto
{
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
}

/// <summary>
/// Data transfer object for displaying audit log items
/// </summary>
public class AuditLogItemDto
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    
    /// <summary>
    /// Formatted timestamp for display
    /// </summary>
    public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>
/// Data transfer object for paginated audit logs with filtering
/// </summary>
public class PagedAuditLogsDto
{
    public List<AuditLogItemDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    
    // Applied filters
    public string? FilterUserId { get; set; }
    public string? FilterEntityType { get; set; }
    public string? FilterAction { get; set; }
    public DateTime? FilterStartDate { get; set; }
    public DateTime? FilterEndDate { get; set; }
}

/// <summary>
/// Data transfer object for audit log changes comparison
/// Used to highlight differences between old and new values
/// </summary>
public class AuditLogChangesDto
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public bool HasChanged { get; set; }
}