using Microsoft.EntityFrameworkCore;
using Proj1.Data;
using Proj1.Interfaces;
using Proj1.Models.Entities;

namespace Proj1.Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly ApplicationDbContext _context;

    public ResidentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Resident>> GetAllAsync()
    {
        return await _context.Residents.ToListAsync();
    }

    public async Task<List<Resident>> SearchAsync(string search)
    {
        return await _context.Residents
            .Where(r =>
                r.FirstName.Contains(search) ||
                r.LastName.Contains(search) ||
                r.Address.Contains(search))
            .ToListAsync();
    }

    public async Task<Resident?> GetByIdAsync(int id)
    {
        return await _context.Residents.FindAsync(id);
    }
    
    public async Task<Resident?> GetByUserIdAsync(string userId)
    {
        return await _context.Residents
            .FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task AddAsync(Resident resident)
    {
        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Resident resident)
    {
        _context.Residents.Update(resident);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Resident resident)
    {
        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Resident> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sort,
        int? minAge,
        int? maxAge)
    {
        var query = _context.Residents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.FirstName.Contains(search) ||
                r.LastName.Contains(search) ||
                r.Address.Contains(search));
        }

        // Age filter (best practice: translate age to birthdate range)
        if (minAge.HasValue)
        {
            var maxBirthDate = DateTime.Today.AddYears(-minAge.Value);
            query = query.Where(r => r.BirthDate <= maxBirthDate);
        }

        if (maxAge.HasValue)
        {
            var minBirthDate = DateTime.Today.AddYears(-maxAge.Value - 1).AddDays(1);
            query = query.Where(r => r.BirthDate >= minBirthDate);
        }

        query = sort switch
        {
            "birth_asc" => query.OrderBy(r => r.BirthDate),
            "birth_desc" => query.OrderByDescending(r => r.BirthDate),
            "name_desc" => query.OrderByDescending(r => r.LastName).ThenByDescending(r => r.FirstName),
            _ => query.OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
        };

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}