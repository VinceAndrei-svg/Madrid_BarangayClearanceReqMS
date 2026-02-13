namespace Proj1.DTOs;

public class ClearanceDocumentDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ResidentFullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string CivilStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string ClearanceType { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string? OfficialReceiptNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string IssuedBy { get; set; } = string.Empty;
}