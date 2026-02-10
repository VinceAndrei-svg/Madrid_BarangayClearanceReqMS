using Proj1.Models.ViewModels;

namespace Proj1.Interfaces;

public interface IPdfService
{
    byte[] GenerateResidentListPdf(List<ResidentViewModel> residents);
}