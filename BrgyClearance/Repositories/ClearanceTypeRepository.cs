using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Repositories;

/// <summary>
/// Repository for ClearanceType entity operations
/// </summary>
public class ClearanceTypeRepository : IClearanceTypeRepository
{
    private readonly ApplicationDbContext _context;

    public ClearanceTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all active clearance types ordered by name
    /// ✅ Uses Name property (which maps to TypeName column via [Column] attribute)
    /// </summary>
    public async Task<List<ClearanceType>> GetActiveAsync()
    {
        return await _context.ClearanceTypes
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)  // ✅ FIXED: Uses Name property
            .ToListAsync();
    }
}