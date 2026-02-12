namespace Proj1.Models.ViewModels;

/// <summary>
/// ViewModel for displaying clearance requests in list views.
/// Used in Index (Admin/Staff view all) and MyRequests (Resident view own).
/// Simplified version of ClearanceRequestDetailsViewModel for table display.
/// </summary>
public class ClearanceRequestViewModel
{
    /// <summary>
    /// Unique identifier for the clearance request
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Auto-generated reference number (e.g., "CLR-2024-0001")
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the resident who made the request
    /// Mapped from ResidentFullName in DTO via AutoMapper
    /// </summary>
    public string ResidentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of clearance being requested (e.g., "Barangay Clearance", "Business Permit")
    /// </summary>
    public string ClearanceTypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Purpose/reason for requesting the clearance
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the request was submitted
    /// </summary>
    public DateTime RequestDate { get; set; }
    
    /// <summary>
    /// Current status as string (e.g., "Submitted", "Approved", "Rejected")
    /// Mapped from RequestStatus enum in DTO via AutoMapper
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional remarks from staff during approval/rejection
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// Fee amount for this clearance type
    /// </summary>
    public decimal Fee { get; set; }
    
    /// <summary>
    /// CSS class for Bootstrap status badge styling.
    /// Used in views to color-code status badges.
    /// 
    /// LEARNING POINT #4: Computed Properties in ViewModels
    /// ======================================================
    /// This is a COMPUTED PROPERTY (no setter, only getter). It calculates
    /// the CSS class based on the Status value. Benefits:
    /// 
    /// 1. SINGLE SOURCE OF TRUTH: Status colors defined in one place
    /// 2. CLEAN VIEWS: No if-else logic cluttering the Razor views
    /// 3. CONSISTENCY: Same status always gets same color
    /// 4. MAINTAINABILITY: Change colors in one place, affects all views
    /// 
    /// Pattern matching switch expression is cleaner than if-else chains.
    /// 
    /// FIX: Added missing statuses that were causing bugs:
    /// - Cancelled: Was missing, so it fell through to default "Submitted" styling
    /// - ForRelease: Was missing
    /// - Released: Was missing
    /// - Expired: Was missing
    /// </summary>
    public string StatusBadgeClass => Status switch
    {
        // Awaiting processing
        "Submitted" => "bg-info text-dark",      // Light blue
        "Pending" => "bg-warning text-dark",     // Yellow/orange
        
        // Approved flow
        "Approved" => "bg-success",              // Green - awaiting payment
        "ForRelease" => "bg-primary",            // Blue - paid, ready to release
        "Released" => "bg-success",              // Green - completed successfully
        
        // Terminal states
        "Rejected" => "bg-danger",               // Red - denied
        "Cancelled" => "bg-secondary",           // Grey - user cancelled
        "Expired" => "bg-dark",                  // Dark grey - validity expired
        
        // Fallback (should never happen if enum is complete)
        _ => "bg-secondary"
    };
}