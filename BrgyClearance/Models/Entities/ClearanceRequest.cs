using Proj1.Models.Common;
using Proj1.Models.Common.Enums;

namespace Proj1.Models.Entities;

public class ClearanceRequest : BaseEntity
{
    public int Id { get; set; }

    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;

    public int ClearanceTypeId { get; set; }
    public ClearanceType ClearanceType { get; set; } = null!;

    public string Purpose { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; } = RequestStatus.Submitted;

    public string? ReferenceNumber { get; set; }

    public string? ProcessedByUserId { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
}