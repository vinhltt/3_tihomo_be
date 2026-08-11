using AutoMapper;
using MoneyManagement.Application.DTOs.Budget;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Mapper;

/// <summary>
///     AutoMapper profile for Budget entity mappings (EN)<br />
///     Profile AutoMapper cho ánh xạ entity Budget (VI)
/// </summary>
public class BudgetMappingProfile : Profile
{
    public BudgetMappingProfile()
    {
        // Budget entity to BudgetViewModel mapping
        CreateMap<Budget, BudgetViewModel>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.RemainingAmount))
            .ForMember(dest => dest.IsOverBudget, opt => opt.MapFrom(src => src.IsOverBudget))
            .ForMember(dest => dest.SpendingPercentage, opt => opt.MapFrom(src => src.SpendingPercentage))
            .ForMember(dest => dest.IsAlertThresholdReached, opt => opt.MapFrom(src => src.IsAlertThresholdReached));

        // CreateBudgetRequest to Budget entity mapping
        CreateMap<CreateBudgetRequest, Budget>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.SpentAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BudgetStatus.Active))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // UpdateBudgetRequest to Budget entity mapping
        CreateMap<UpdateBudgetRequest, Budget>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.SpentAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}