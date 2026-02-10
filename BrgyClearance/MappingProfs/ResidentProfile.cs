using AutoMapper;
using Proj1.DTOs;
using Proj1.Models.Entities;
using Proj1.Models.ViewModels;

namespace Proj1.MappingProfiles;

public class ResidentProfile : Profile
{
    public ResidentProfile()
    {
        CreateMap<Resident, ResidentDto>();
        CreateMap<CreateResidentDto, Resident>();
        CreateMap<UpdateResidentDto, Resident>();

        CreateMap<ResidentDto, ResidentViewModel>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<ResidentDto, EditResidentViewModel>();
        CreateMap<EditResidentViewModel, UpdateResidentDto>();
        CreateMap<CreateResidentViewModel, CreateResidentDto>();

        CreateMap<ResidentDto, ResidentDetailsViewModel>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}