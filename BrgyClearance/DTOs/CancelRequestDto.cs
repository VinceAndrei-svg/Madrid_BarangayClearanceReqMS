namespace Proj1.DTOs;

/// <summary>
/// DTO for cancelling a clearance request.
/// </summary>
public class CancelRequestDto
{
    public int RequestId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}