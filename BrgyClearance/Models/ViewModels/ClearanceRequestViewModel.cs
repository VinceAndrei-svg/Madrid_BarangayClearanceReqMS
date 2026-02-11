namespace Proj1.Models.ViewModels;

public class ClearanceRequestViewModel
{
    public int Id { get; set; }
    public string ResidentName { get; set; } = string.Empty;
    public string ClearanceTypeName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
}