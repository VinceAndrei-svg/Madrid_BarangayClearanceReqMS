using Proj1.Models.Entities;

namespace Proj1.Interfaces;

/// <summary>
/// Repository interface for audit log data access
/// Follows the repository pattern used throughout the application
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds a new audit log entry
    /// </summary>
    Task AddAsync(AuditLog auditLog);
    
    /// <summary>
    /// Gets paginated audit logs with optional filtering
    /// </summary>
    Task<(List<AuditLog> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    /// <summary>
    /// Gets audit logs for a specific entity
    /// </summary>
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets recent audit logs
    /// </summary>
    Task<List<AuditLog>> GetRecentAsync(int count);
}