namespace Proj1.Models.ViewModels;

public class ResidentIndexViewModel
{
    public string? Search { get; set; }
    public List<ResidentViewModel> Residents { get; set; } = new();

    public int Page { get; set; }
    public int TotalPages { get; set; }

    public int PageSize { get; set; }
    public string? Sort { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}