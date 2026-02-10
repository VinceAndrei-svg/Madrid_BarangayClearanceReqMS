namespace Proj1.Models.ViewModels;

public class ProcessClearanceRequestViewModel
{
    public int Id { get; set; }
    public bool Approve { get; set; }
    public string? Remarks { get; set; }
}