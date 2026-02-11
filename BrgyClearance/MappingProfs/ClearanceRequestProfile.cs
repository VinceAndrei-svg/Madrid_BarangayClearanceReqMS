using AutoMapper;
using Proj1.DTOs;
using Proj1.Models.Entities;
using Proj1.Models.ViewModels;

namespace Proj1.MappingProfiles;

public class ClearanceRequestProfile : Profile
{
    public ClearanceRequestProfile()
    {
        CreateMap<CreateClearanceRequestDto, ClearanceRequest>();

        CreateMap<ClearanceRequest, ClearanceRequestDto>()
            .ForMember(d => d.ResidentName, o => o.MapFrom(s => 
                $"{s.Resident.FirstName} {s.Resident.LastName}"))
            .ForMember(d => d.ClearanceTypeName, o => o.MapFrom(s => 
                s.ClearanceType.TypeName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<ClearanceRequestDto, ClearanceRequestViewModel>();
        
        CreateMap<ClearanceType, ClearanceTypeViewModel>();
    }
}