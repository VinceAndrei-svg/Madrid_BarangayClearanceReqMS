namespace Proj1.Models.ViewModels;

public class CreateClearanceRequestViewModel
{
    public int ResidentId { get; set; }
    public int ClearanceTypeId { get; set; }
    public string Purpose { get; set; } = string.Empty;

    public List<ClearanceTypeViewModel> ClearanceTypes { get; set; } = new();
}