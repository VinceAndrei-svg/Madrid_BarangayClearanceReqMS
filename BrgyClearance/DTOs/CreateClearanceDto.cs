namespace Proj1.DTOs;

public class CreateClearanceRequestDto
{
    public int ResidentId { get; set; }
    public int ClearanceTypeId { get; set; }
    public string Purpose { get; set; } = string.Empty;
}