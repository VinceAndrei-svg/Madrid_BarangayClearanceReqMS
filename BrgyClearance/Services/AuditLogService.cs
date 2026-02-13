using System.Text.Json;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Services;

/// <summary>
/// Service for audit log operations
/// Follows the service pattern used throughout the application
/// Handles JSON serialization and business logic for audit logging
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    
    // JSON serialization options for consistent formatting
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false, // Compact format to save space
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Logs an action to the audit trail
    /// Safely serializes objects to JSON and handles null values
    /// </summary>
    public async Task<ServiceResult> LogAsync(AuditLogDto dto)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = dto.UserId,
                UserEmail = dto.UserEmail,
                Action = dto.Action,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                OldValues = SerializeValues(dto.OldValues),
                NewValues = SerializeValues(dto.NewValues),
                Details = dto.Details,
                IpAddress = dto.IpAddress,
                Timestamp = DateTime.UtcNow,
                ControllerName = dto.ControllerName,
                ActionName = dto.ActionName
            };

            await _auditLogRepository.AddAsync(auditLog);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail the main operation
            // Audit logging should never break the application
            Console.Error.WriteLine($"Audit logging failed: {ex.Message}");
            return ServiceResult.Failure("Audit logging failed");
        }
    }

    /// <summary>
    /// Retrieves paginated audit logs with optional filtering
    /// </summary>
    public async Task<ServiceResult<PagedAuditLogsDto>> GetPagedLogsAsync(
        int page,
        int pageSize,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Prevent excessive page sizes

            var (items, totalItems) = await _auditLogRepository.GetPagedAsync(
                page, 
                pageSize, 
                userId, 
                entityType, 
                action, 
                startDate, 
                endDate);

            var result = new PagedAuditLogsDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                FilterUserId = userId,
                FilterEntityType = entityType,
                FilterAction = action,
                FilterStartDate = startDate,
                FilterEndDate = endDate
            };

            return ServiceResult<PagedAuditLogsDto>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedAuditLogsDto>.Failure($"Failed to retrieve audit logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets audit logs for a specific entity
    /// </summary>
    public async Task<ServiceResult<List<AuditLogItemDto>>> GetByEntityAsync(string entityType, string entityId)
    {
        try
        {
            var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
            var dtos = logs.Select(MapToDto).ToList();
            
            return ServiceResult<List<AuditLogItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<AuditLogItemDto>>.Failure($"Failed to retrieve entity audit logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets recent audit logs for dashboard displays
    /// </summary>
    public async Task<ServiceResult<List<AuditLogItemDto>>> GetRecentLogsAsync(int count = 50)
    {
        try
        {
            // Validate count
            if (count < 1) count = 10;
            if (count > 200) count = 200; // Reasonable limit for recent logs

            var logs = await _auditLogRepository.GetRecentAsync(count);
            var dtos = logs.Select(MapToDto).ToList();
            
            return ServiceResult<List<AuditLogItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<AuditLogItemDto>>.Failure($"Failed to retrieve recent audit logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely serializes objects to JSON
    /// Filters out sensitive properties like passwords
    /// </summary>
    private string? SerializeValues(object? values)
    {
        if (values == null)
            return null;

        try
        {
            // Handle anonymous objects and DTOs
            var jsonString = JsonSerializer.Serialize(values, _jsonOptions);
            
            // Remove sensitive fields from the JSON
            var jsonDoc = JsonDocument.Parse(jsonString);
            var sanitized = SanitizeSensitiveData(jsonDoc.RootElement);
            
            return JsonSerializer.Serialize(sanitized, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to serialize audit values: {ex.Message}");
            return "[Serialization Error]";
        }
    }

    /// <summary>
    /// Removes sensitive data from JSON before logging
    /// Prevents passwords and other sensitive fields from being stored
    /// </summary>
    private object? SanitizeSensitiveData(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var result = new Dictionary<string, object?>();
            
            foreach (var property in element.EnumerateObject())
            {
                // List of sensitive property names to exclude
                var sensitiveFields = new[] { "password", "passwordhash", "securitystamp", 
                    "concurrencystamp", "token", "secret", "key" };
                
                var propertyNameLower = property.Name.ToLowerInvariant();
                
                if (sensitiveFields.Any(field => propertyNameLower.Contains(field)))
                {
                    result[property.Name] = "[REDACTED]";
                }
                else
                {
                    result[property.Name] = SanitizeSensitiveData(property.Value);
                }
            }
            
            return result;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray()
                .Select(SanitizeSensitiveData)
                .ToList();
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }
        else if (element.ValueKind == JsonValueKind.Number)
        {
            return element.GetDecimal();
        }
        else if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
        {
            return element.GetBoolean();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Maps AuditLog entity to DTO
    /// </summary>
    private AuditLogItemDto MapToDto(AuditLog log)
    {
        return new AuditLogItemDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserEmail = log.UserEmail,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Details = log.Details,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp,
            ControllerName = log.ControllerName,
            ActionName = log.ActionName
        };
    }
}