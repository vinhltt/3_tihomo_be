using AutoMapper;
using MoneyManagement.Application.DTOs.Jar;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Mapper;

/// <summary>
///     AutoMapper profile for Jar entity mappings (EN)<br />
///     Profile AutoMapper cho ánh xạ entity Jar (VI)
/// </summary>
public class JarMappingProfile : Profile
{
    public JarMappingProfile()
    {
        // Jar entity to JarViewModel mapping
        CreateMap<Jar, JarViewModel>()
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
            .ForMember(dest => dest.TargetAmount, opt => opt.MapFrom(src => src.TargetAmount));

        // CreateJarRequest to Jar entity mapping
        CreateMap<CreateJarRequest, Jar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // UpdateJarRequest to Jar entity mapping
        CreateMap<UpdateJarRequest, Jar>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.JarType, opt => opt.Ignore()) // JarType should not be changed after creation
            .ForMember(dest => dest.Balance, opt => opt.Ignore()) // Balance is managed through add/withdraw operations
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}