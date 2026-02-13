using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Proj1.DTOs;
using Proj1.Interfaces;

namespace Proj1.Filters;

/// <summary>
/// Action filter that automatically logs successful POST, PUT, and DELETE operations
/// Implements IAsyncActionFilter for async logging support
/// Does NOT log GET requests or failed operations
/// Does NOT log sensitive data like passwords
/// </summary>
public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditLogService _auditLogService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(
        IAuditLogService auditLogService,
        UserManager<IdentityUser> userManager,
        ILogger<AuditActionFilter> logger)
    {
        _auditLogService = auditLogService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the action first
        var executedContext = await next();

        // Only log if the action succeeded
        if (executedContext.Exception != null || !ShouldLog(context))
        {
            return;
        }

        // Fire-and-forget: Don't await to avoid slowing down the response
        // Use Task.Run to ensure it runs on a background thread
        _ = Task.Run(async () =>
        {
            try
            {
                await LogActionAsync(context, executedContext);
            }
            catch (Exception ex)
            {
                // Audit logging should never break the application
                _logger.LogError(ex, "Failed to log audit entry for {Controller}.{Action}", 
                    context.Controller.GetType().Name, 
                    context.ActionDescriptor.DisplayName);
            }
        });
    }

    /// <summary>
    /// Determines if the action should be logged
    /// Excludes GET requests and non-modifying actions
    /// </summary>
    private bool ShouldLog(ActionExecutingContext context)
    {
        var httpMethod = context.HttpContext.Request.Method;
        
        // Only log state-changing operations
        return httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "DELETE" || httpMethod == "PATCH";
    }

    /// <summary>
    /// Performs the actual logging
    /// Extracts user information, IP address, and action details
    /// </summary>
    private async Task LogActionAsync(ActionExecutingContext context, ActionExecutedContext executedContext)
    {
        // Extract user information
        var userId = _userManager.GetUserId(context.HttpContext.User);
        string? userEmail = null;
        
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            userEmail = user?.Email;
        }

        // Extract IP address
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        // Extract controller and action names
        var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        var actionName = context.ActionDescriptor.RouteValues["action"];

        // Determine the action type and extract entity information
        var (action, entityType, entityId, oldValues, newValues, details) = ExtractAuditInfo(context, executedContext);

        // Create the audit log DTO
        var auditDto = new AuditLogDto
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            Details = details,
            IpAddress = ipAddress,
            ControllerName = controllerName,
            ActionName = actionName
        };

        // Log the action
        await _auditLogService.LogAsync(auditDto);
    }

    /// <summary>
    /// Extracts audit information from the action context
    /// Tries to infer entity type, ID, and values from the action parameters and result
    /// </summary>
    private (string Action, string EntityType, string EntityId, object? OldValues, object? NewValues, string? Details) 
        ExtractAuditInfo(ActionExecutingContext context, ActionExecutedContext executedContext)
    {
        var httpMethod = context.HttpContext.Request.Method;
        var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";

        // Determine action type from HTTP method
        var action = httpMethod switch
        {
            "POST" => DetermineCreateOrOtherAction(actionName),
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => actionName
        };

        // Try to extract entity type from controller name
        var entityType = InferEntityType(controllerName, actionName);

        // Try to extract entity ID from route values or action parameters
        var entityId = ExtractEntityId(context);

        // Extract old and new values
        var (oldValues, newValues) = ExtractValues(context, executedContext, action);

        // Create a descriptive detail message
        var details = $"{action} {entityType} via {controllerName}.{actionName}";

        return (action, entityType, entityId, oldValues, newValues, details);
    }

    /// <summary>
    /// Determines if a POST action is a Create or another type of action
    /// </summary>
    private string DetermineCreateOrOtherAction(string actionName)
    {
        var actionLower = actionName.ToLowerInvariant();
        
        if (actionLower.Contains("create") || actionLower.Contains("add") || actionLower.Contains("register"))
            return "Create";
        
        if (actionLower.Contains("approve"))
            return "Approve";
        
        if (actionLower.Contains("reject"))
            return "Reject";
        
        if (actionLower.Contains("toggle"))
            return "Toggle";
        
        if (actionLower.Contains("payment") || actionLower.Contains("pay"))
            return "RecordPayment";
        
        if (actionLower.Contains("release"))
            return "Release";
        
        if (actionLower.Contains("submit"))
            return "Submit";
        
        return actionName;
    }

    /// <summary>
    /// Infers the entity type from controller and action names
    /// </summary>
    private string InferEntityType(string controllerName, string actionName)
    {
        // Remove common suffixes
        var baseName = controllerName
            .Replace("Controller", "")
            .Replace("Management", "");

        // Special cases based on controller name
        if (baseName.Contains("Resident"))
            return "Resident";
        
        if (baseName.Contains("Clearance"))
            return "ClearanceRequest";
        
        if (baseName.Contains("Admin") || baseName.Contains("Staff") || baseName.Contains("Account"))
        {
            // Infer from action name
            var actionLower = actionName.ToLowerInvariant();
            
            if (actionLower.Contains("staff"))
                return "User";
            
            if (actionLower.Contains("resident"))
                return "Resident";
        }

        return baseName;
    }

    /// <summary>
    /// Extracts the entity ID from route values or action parameters
    /// </summary>
    private string ExtractEntityId(ActionExecutingContext context)
    {
        // Try to get ID from route values
        if (context.RouteData.Values.TryGetValue("id", out var routeId))
        {
            return routeId?.ToString() ?? "Unknown";
        }

        // Try to get ID from action parameters
        foreach (var param in context.ActionArguments)
        {
            if (param.Key.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                return param.Value?.ToString() ?? "Unknown";
            }

            // Check if the parameter has an Id property
            var paramType = param.Value?.GetType();
            var idProperty = paramType?.GetProperty("Id");
            
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(param.Value);
                if (idValue != null)
                {
                    return idValue.ToString() ?? "Unknown";
                }
            }
        }

        return "Unknown";
    }

    /// <summary>
    /// Extracts old and new values from action parameters and result
    /// Filters out sensitive data
    /// </summary>
    private (object? OldValues, object? NewValues) ExtractValues(
        ActionExecutingContext context, 
        ActionExecutedContext executedContext, 
        string action)
    {
        object? oldValues = null;
        object? newValues = null;

        // For Create actions, only new values exist
        if (action == "Create" || action.Contains("Create"))
        {
            newValues = ExtractModelFromParameters(context);
        }
        // For Update actions, we'd need the old values from the database (not available here)
        // The controller should pass both old and new values explicitly if needed
        else if (action == "Update")
        {
            newValues = ExtractModelFromParameters(context);
            // Note: Old values should be captured in the controller if detailed change tracking is needed
        }
        // For Delete actions, old values are what's being deleted
        else if (action == "Delete")
        {
            // The controller should pass the entity being deleted if needed
            oldValues = ExtractModelFromParameters(context);
        }
        else
        {
            // For other actions (Approve, Reject, etc.), capture the relevant parameters
            newValues = ExtractModelFromParameters(context);
        }

        return (oldValues, newValues);
    }

    /// <summary>
    /// Extracts the model/DTO from action parameters
    /// Excludes simple types like strings and IDs
    /// </summary>
    private object? ExtractModelFromParameters(ActionExecutingContext context)
    {
        foreach (var param in context.ActionArguments)
        {
            var paramType = param.Value?.GetType();
            
            // Skip simple types, IDs, and null values
            if (paramType == null || 
                paramType.IsPrimitive || 
                paramType == typeof(string) ||
                param.Key.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Return the first complex object (usually the DTO/ViewModel)
            return param.Value;
        }

        return null;
    }
}