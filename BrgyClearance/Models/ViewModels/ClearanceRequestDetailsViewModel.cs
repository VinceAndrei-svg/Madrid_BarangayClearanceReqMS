using Proj1.Models.Common.Enums;

namespace Proj1.Models.ViewModels;

/// <summary>
/// ViewModel for displaying full clearance request details.
/// Includes UI helpers for showing/hiding buttons based on status.
/// </summary>
public class ClearanceRequestDetailsViewModel
{
    public int Id { get; set; }
    
    // Resident info
    public string ResidentName { get; set; } = string.Empty;
    public string ResidentAddress { get; set; } = string.Empty;
    
    // Clearance info
    public string ClearanceTypeName { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string Purpose { get; set; } = string.Empty;
    
    // Request info
    public DateTime RequestDate { get; set; }
    public RequestStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;  // User-friendly status
    public string? ReferenceNumber { get; set; }
    
    // Processing info
    public DateTime? ProcessedDate { get; set; }
    public string? ProcessedByName { get; set; }
    public string? Remarks { get; set; }
    
    // Payment info
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    
    // Release info
    public DateTime? ReleasedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Cancellation info
    public string? CancellationReason { get; set; }
    public DateTime? CancelledDate { get; set; }
    
    // === UI HELPERS ===
    // These properties help the view decide what buttons to show
    
    /// <summary>
    /// True if resident can cancel (status is Submitted or Pending)
    /// </summary>
    public bool CanBeCancelled => 
        Status == RequestStatus.Submitted || Status == RequestStatus.Pending;
    
    /// <summary>
    /// True if staff can approve/reject (status is Submitted or Pending)
    /// </summary>
    public bool CanBeProcessed => 
        Status == RequestStatus.Submitted || Status == RequestStatus.Pending;
    
    /// <summary>
    /// True if staff can record payment (status is Approved and not paid)
    /// </summary>
    public bool CanRecordPayment => 
        Status == RequestStatus.Approved && !IsPaid;
    
    /// <summary>
    /// True if staff can mark as released (status is ForRelease)
    /// </summary>
    public bool CanBeReleased => 
        Status == RequestStatus.ForRelease;
    
    /// <summary>
    /// CSS class for status badge (Bootstrap colors)
    /// </summary>
    public string StatusBadgeClass => Status switch
    {
        RequestStatus.Submitted => "badge bg-info",
        RequestStatus.Pending => "badge bg-warning",
        RequestStatus.Approved => "badge bg-success",
        RequestStatus.Rejected => "badge bg-danger",
        RequestStatus.Cancelled => "badge bg-secondary",
        RequestStatus.ForRelease => "badge bg-primary",
        RequestStatus.Released => "badge bg-success",
        RequestStatus.Expired => "badge bg-dark",
        _ => "badge bg-secondary"
    };
}