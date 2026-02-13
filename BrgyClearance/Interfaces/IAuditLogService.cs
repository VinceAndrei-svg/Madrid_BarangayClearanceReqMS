using Proj1.DTOs;

namespace Proj1.Interfaces;

/// <summary>
/// Service for managing audit log operations
/// Follows the repository pattern used throughout the application
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an action to the audit trail
    /// </summary>
    /// <param name="dto">Audit log data transfer object</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult> LogAsync(AuditLogDto dto);
    
    /// <summary>
    /// Retrieves paginated audit logs with optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="userId">Optional filter by user ID</param>
    /// <param name="entityType">Optional filter by entity type</param>
    /// <param name="action">Optional filter by action</param>
    /// <param name="startDate">Optional filter by start date (UTC)</param>
    /// <param name="endDate">Optional filter by end date (UTC)</param>
    /// <returns>Service result with paginated audit logs</returns>
    Task<ServiceResult<PagedAuditLogsDto>> GetPagedLogsAsync(
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
    /// <param name="entityType">Type of entity (e.g., "Resident", "ClearanceRequest")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <returns>Service result with list of audit logs for the entity</returns>
    Task<ServiceResult<List<AuditLogItemDto>>> GetByEntityAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets recent audit logs (last 50 entries)
    /// Useful for dashboard displays
    /// </summary>
    /// <returns>Service result with list of recent audit logs</returns>
    Task<ServiceResult<List<AuditLogItemDto>>> GetRecentLogsAsync(int count = 50);
}