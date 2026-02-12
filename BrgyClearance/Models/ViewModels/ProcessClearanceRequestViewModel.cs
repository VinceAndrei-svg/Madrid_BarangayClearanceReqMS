using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.ViewModels;

/// <summary>
/// ViewModel for processing (approving/rejecting) clearance requests.
/// Used by Admin/Staff to review and make decisions on requests.
/// </summary>
public class ProcessClearanceRequestViewModel
{
    /// <summary>
    /// ID of the clearance request being processed
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Reference number for display purposes (read-only, populated from request)
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Resident's full name for display purposes (read-only, populated from request)
    /// </summary>
    public string ResidentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Purpose of the clearance request (read-only, populated from request)
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
    
    /// <summary>
    /// True for approval, False for rejection
    /// </summary>
    public bool Approve { get; set; }
    
    /// <summary>
    /// Staff/Admin remarks or reason for decision
    /// </summary>
    [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }
}