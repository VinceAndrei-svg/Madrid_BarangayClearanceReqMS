using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Repositories;

/// <summary>
/// Repository for audit log data access
/// Follows the same patterns as other repositories in the project
/// Optimized for write-heavy workload (auditing) with proper indexing
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new audit log entry
    /// Designed for fire-and-forget logging with minimal performance impact
    /// </summary>
    public async Task AddAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets paginated audit logs with optional filtering
    /// Uses indexed columns for efficient querying
    /// </summary>
    public async Task<(List<AuditLog> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action.Contains(action));
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Include the entire end date (up to 23:59:59.999)
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.Timestamp <= endOfDay);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Order by most recent first
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking() // Read-only optimization
            .ToListAsync();

        return (items, totalItems);
    }

    /// <summary>
    /// Gets all audit logs for a specific entity
    /// Useful for viewing the history of a particular record
    /// </summary>
    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .AsNoTracking() // Read-only optimization
            .ToListAsync();
    }

    /// <summary>
    /// Gets the most recent audit logs
    /// Optimized for dashboard displays
    /// </summary>
    public async Task<List<AuditLog>> GetRecentAsync(int count)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .AsNoTracking() // Read-only optimization
            .ToListAsync();
    }
}