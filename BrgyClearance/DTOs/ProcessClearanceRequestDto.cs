namespace Proj1.DTOs;

public class ProcessClearanceRequestDto
{
    public int Id { get; set; }
    public bool Approve { get; set; }
    public string? Remarks { get; set; }
    public string ProcessedByUserId { get; set; } = string.Empty;
}