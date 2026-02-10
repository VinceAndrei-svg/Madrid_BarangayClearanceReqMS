using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Repositories;

public class ClearanceTypeRepository : IClearanceTypeRepository
{
    private readonly ApplicationDbContext _context;

    public ClearanceTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClearanceType>> GetActiveAsync()
    {
        return await _context.ClearanceTypes
            .Where(c => c.IsActive)
            .OrderBy(c => c.TypeName)
            .ToListAsync();
    }
}