using Proj1.Models.Common.Enums;

namespace Proj1.DTOs;

/// <summary>
/// DTO for returning clearance request data from service layer.
/// Contains all data needed for display, including related entity info.
/// </summary>
public class ClearanceRequestDto
{
    public int Id { get; set; }
    
    // Resident info
    public int ResidentId { get; set; }
    public string ResidentFirstName { get; set; } = string.Empty;
    public string ResidentLastName { get; set; } = string.Empty;
    public string ResidentFullName => $"{ResidentFirstName} {ResidentLastName}";
    public string ResidentAddress { get; set; } = string.Empty;
    
    // Clearance type info
    public int ClearanceTypeId { get; set; }
    public string ClearanceTypeName { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    
    // Request info
    public string Purpose { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public RequestStatus Status { get; set; }
    public string? ReferenceNumber { get; set; }
    
    // Processing info
    public string? ProcessedByUserId { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
    
    // Payment info
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? CollectedByUserId { get; set; }
    
    // Release info
    public DateTime? ReleasedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Cancellation info
    public string? CancelledBy { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }
    
    // Audit fields from BaseEntity
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}