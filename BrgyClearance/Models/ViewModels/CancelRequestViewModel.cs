using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.ViewModels;

/// <summary>
/// ViewModel for cancelling a clearance request.
/// Used both for the Details page cancel form AND the modal cancel form.
/// </summary>
public class CancelRequestViewModel
{
    /// <summary>
    /// ID of the request being cancelled.
    /// Hidden field in form, passed via route parameter.
    /// </summary>
    public int RequestId { get; set; }
    
    /// <summary>
    /// Reference number for display purposes.
    /// Shown to user for confirmation context.
    /// NOT submitted with form (display-only).
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Reason for cancellation - REQUIRED field.
    /// Must be between 10 and 500 characters.
    /// </summary>
    [Required(ErrorMessage = "Please provide a reason for cancellation.")]
    [StringLength(500, MinimumLength = 10, 
        ErrorMessage = "Cancellation reason must be between 10 and 500 characters.")]
    [Display(Name = "Reason for Cancellation")]
    public string Reason { get; set; } = string.Empty;
}