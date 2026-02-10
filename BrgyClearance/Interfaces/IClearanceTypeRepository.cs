using Proj1.Models.Entities;

namespace Proj1.Interfaces;

public interface IClearanceTypeRepository
{
    Task<List<ClearanceType>> GetActiveAsync();
}