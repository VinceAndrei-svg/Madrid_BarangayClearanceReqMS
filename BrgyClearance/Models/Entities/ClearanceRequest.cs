using Proj1.Models.Common;
using Proj1.Models.Common.Enums;

namespace Proj1.Models.Entities;

/// <summary>
/// Represents a clearance request submitted by a resident.
/// Tracks the full lifecycle from submission to release/expiry.
/// </summary>
public class ClearanceRequest : BaseEntity
{
    public int Id { get; set; }

    // === RESIDENT & TYPE INFO ===
    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;

    public int ClearanceTypeId { get; set; }
    public ClearanceType ClearanceType { get; set; } = null!;

    public string Purpose { get; set; } = string.Empty;

    // === REQUEST INFO ===
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public string? ReferenceNumber { get; set; }

    // === PROCESSING INFO ===
    /// <summary>
    /// User ID (from AspNetUsers) of the staff who approved/rejected
    /// </summary>
    public string? ProcessedByUserId { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }

    // === RELEASE INFO ===
    /// <summary>
    /// Date when clearance was physically given to resident
    /// </summary>
    public DateTime? ReleasedDate { get; set; }
    
    /// <summary>
    /// Date when clearance expires (typically 6 months from release)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    // === CANCELLATION INFO ===
    /// <summary>
    /// User ID of who cancelled (resident or staff)
    /// </summary>
    public string? CancelledBy { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    // === PAYMENT INFO (for cash-only workflow) ===
    /// <summary>
    /// Whether payment has been collected (cash only)
    /// </summary>
    public bool IsPaid { get; set; } = false;
    
    /// <summary>
    /// Date when payment was collected
    /// </summary>
    public DateTime? PaidDate { get; set; }
    
    /// <summary>
    /// User ID of staff who collected payment
    /// </summary>
    public string? CollectedByUserId { get; set; }

    // === DOCUMENT GENERATION INFO (NEW) ===
    /// <summary>
    /// Web path to the generated PDF clearance document (e.g., "/clearances/Clearance_ABC123_20240213.pdf")
    /// </summary>
    public string? ClearanceDocumentPath { get; set; }
    
    /// <summary>
    /// Timestamp when the PDF document was generated
    /// </summary>
    public DateTime? DocumentGeneratedDate { get; set; }
    
    /// <summary>
    /// User ID of staff who generated the document (if manually triggered)
    /// </summary>
    public string? DocumentGeneratedByUserId { get; set; }

    // === PAYMENT DETAILS (ENHANCED - OPTIONAL) ===
    /// <summary>
    /// Official Receipt Number for the payment
    /// </summary>
    public string? OfficialReceiptNumber { get; set; }
    
    /// <summary>
    /// Amount paid (should match ClearanceType.Fee, but stored for historical accuracy)
    /// </summary>
    public decimal? AmountPaid { get; set; }
}