using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj1.Models.Entities;

/// <summary>
/// Represents an audit log entry tracking system actions and changes
/// Immutable by design - audit logs should never be modified after creation
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// User who performed the action (foreign key to AspNetUsers)
    /// Nullable for system-initiated actions
    /// </summary>
    [MaxLength(450)]
    public string? UserId { get; set; }
    
    /// <summary>
    /// Email of the user at the time of action (denormalized for audit history)
    /// Preserved even if user is deleted
    /// </summary>
    [MaxLength(256)]
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Action performed (e.g., "Create", "Update", "Delete", "Approve", "Reject")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity type affected (e.g., "Resident", "ClearanceRequest", "User")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary key of the affected entity (stored as string for flexibility)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON representation of values before the change
    /// NULL for Create operations
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON representation of values after the change
    /// NULL for Delete operations
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Additional context or description of the action
    /// </summary>
    [MaxLength(1000)]
    public string? Details { get; set; }
    
    /// <summary>
    /// IP address of the user who performed the action
    /// </summary>
    [MaxLength(45)] // IPv6 max length
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Timestamp when the action occurred (UTC)
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Controller name where the action originated
    /// </summary>
    [MaxLength(100)]
    public string? ControllerName { get; set; }
    
    /// <summary>
    /// Action method name where the action originated
    /// </summary>
    [MaxLength(100)]
    public string? ActionName { get; set; }
}