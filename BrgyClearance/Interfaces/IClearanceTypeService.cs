using Proj1.Models.ViewModels;

namespace Proj1.Interfaces;

public interface IClearanceTypeService
{
    Task<List<ClearanceTypeViewModel>> GetActiveAsync();
    
}