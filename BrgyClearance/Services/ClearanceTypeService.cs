using AutoMapper;
using Proj1.Interfaces;
using Proj1.Models.ViewModels;

namespace Proj1.Services;

public class ClearanceTypeService : IClearanceTypeService
{
    private readonly IClearanceTypeRepository _repository;
    private readonly IMapper _mapper;

    public ClearanceTypeService(IClearanceTypeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ClearanceTypeViewModel>> GetActiveAsync()
    {
        var types = await _repository.GetActiveAsync();
        return _mapper.Map<List<ClearanceTypeViewModel>>(types);
    }
}