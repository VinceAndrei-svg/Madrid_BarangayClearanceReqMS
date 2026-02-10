namespace Proj1.DTOs;

public class ClearanceRequestDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public int ClearanceTypeId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}