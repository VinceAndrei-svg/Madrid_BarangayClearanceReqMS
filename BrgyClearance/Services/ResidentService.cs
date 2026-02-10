using AutoMapper;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Common;
using Proj1.Models.Entities;

namespace Proj1.Services;

public class ResidentService : IResidentService
{
    private readonly IResidentRepository _repository;
    private readonly IMapper _mapper;

    public ResidentService(IResidentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ResidentDto>> GetAllAsync()
    {
        var residents = await _repository.GetAllAsync();
        return _mapper.Map<List<ResidentDto>>(residents);
    }

    public async Task<List<ResidentDto>> SearchAsync(string search)
    {
        var residents = await _repository.SearchAsync(search);
        return _mapper.Map<List<ResidentDto>>(residents);
    }

    public async Task<ResidentDto?> GetByIdAsync(int id)
    {
        var resident = await _repository.GetByIdAsync(id);
        return resident == null ? null : _mapper.Map<ResidentDto>(resident);
    }

    public async Task CreateAsync(CreateResidentDto dto)
    {
        var resident = _mapper.Map<Resident>(dto);
        await _repository.AddAsync(resident);
    }

    public async Task<bool> UpdateAsync(UpdateResidentDto dto)
    {
        var resident = await _repository.GetByIdAsync(dto.Id);
        if (resident == null) return false;

        _mapper.Map(dto, resident);
        await _repository.UpdateAsync(resident);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var resident = await _repository.GetByIdAsync(id);
        if (resident == null) return false;

        await _repository.DeleteAsync(resident);
        return true;
    }

    public async Task<PagedResult<ResidentDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? sort,
        int? minAge,
        int? maxAge)
    {
        var (items, totalItems) = await _repository.GetPagedAsync(page, pageSize, search, sort, minAge, maxAge);

        return new PagedResult<ResidentDto>
        {
            Items = _mapper.Map<List<ResidentDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}