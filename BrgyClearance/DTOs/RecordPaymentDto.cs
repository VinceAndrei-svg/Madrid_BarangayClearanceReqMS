namespace Proj1.DTOs;

/// <summary>
/// DTO for recording payment collection.
/// </summary>
public class RecordPaymentDto
{
    public int RequestId { get; set; }
    public string StaffUserId { get; set; } = string.Empty;
}