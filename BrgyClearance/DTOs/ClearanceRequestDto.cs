namespace Proj1.DTOs;

public class ClearanceRequestDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string ResidentName { get; set; } = string.Empty;
    public int ClearanceTypeId { get; set; }
    public string ClearanceTypeName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
}