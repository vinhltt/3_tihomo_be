using AutoMapper;
using MoneyManagement.Application.DTOs.SharedExpense;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Mapper;

/// <summary>
///     AutoMapper profile for SharedExpense entity mappings (EN)<br />
///     Profile AutoMapper cho ánh xạ entity SharedExpense (VI)
/// </summary>
public class SharedExpenseMappingProfile : Profile
{
    public SharedExpenseMappingProfile()
    {
        // SharedExpense entity to SharedExpenseResponseDto mapping
        CreateMap<SharedExpense, SharedExpenseResponseDto>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.RemainingAmount))
            .ForMember(dest => dest.IsFullySettled, opt => opt.MapFrom(src => src.IsFullySettled))
            .ForMember(dest => dest.SettlementPercentage, opt => opt.MapFrom(src => src.SettlementPercentage));

        // CreateSharedExpenseRequestDto to SharedExpense entity mapping
        CreateMap<CreateSharedExpenseRequestDto, SharedExpense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore()) // Will be set from user context
            .ForMember(dest => dest.SettledAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SharedExpenseStatus.Pending))
            .ForMember(dest => dest.Participants, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // UpdateSharedExpenseRequestDto to SharedExpense entity mapping
        CreateMap<UpdateSharedExpenseRequestDto, SharedExpense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.SettledAmount, opt => opt.Ignore()) // Managed through payment operations
            .ForMember(dest => dest.Status, opt => opt.Ignore()) // Managed through business logic
            .ForMember(dest => dest.Participants, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted,
                opt => opt.Ignore()); // SharedExpenseParticipant entity to SharedExpenseParticipantResponseDto mapping
        CreateMap<SharedExpenseParticipant, SharedExpenseParticipantResponseDto>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.OwedAmount))
            .ForMember(dest => dest.IsSettled, opt => opt.MapFrom(src => src.IsSettled))
            .ForMember(dest => dest.PaymentPercentage, opt => opt.MapFrom(src => src.PaymentPercentage));

        // CreateSharedExpenseParticipantRequestDto to SharedExpenseParticipant entity mapping
        CreateMap<CreateSharedExpenseParticipantRequestDto, SharedExpenseParticipant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.SettledDate, opt => opt.Ignore())
            .ForMember(dest => dest.SharedExpense, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // UpdateSharedExpenseParticipantRequestDto to SharedExpenseParticipant entity mapping
        CreateMap<UpdateSharedExpenseParticipantRequestDto, SharedExpenseParticipant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SharedExpenseId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAmount, opt => opt.Ignore()) // Managed through payment operations
            .ForMember(dest => dest.SettledDate, opt => opt.Ignore()) // Managed through payment operations
            .ForMember(dest => dest.SharedExpense, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // SharedExpense entity to SharedExpenseSummaryDto mapping
        CreateMap<SharedExpense, SharedExpenseSummaryDto>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.RemainingAmount))
            .ForMember(dest => dest.IsFullySettled, opt => opt.MapFrom(src => src.IsFullySettled))
            .ForMember(dest => dest.ParticipantCount, opt => opt.MapFrom(src => src.Participants.Count));

        // SharedExpense entity to UserSharedExpenseStatsDto mapping (for statistics)
        CreateMap<SharedExpense, UserSharedExpenseStatsDto>()
            .ForMember(dest => dest.TotalExpenses, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.TotalOwedAmount, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.TotalPaidAmount, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.SettledExpenses, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.PendingExpenses, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.LastExpenseDate, opt => opt.Ignore()) // Will be calculated in service
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Will be set from context

        // EqualSplitCalculationDto to SharedExpenseParticipant mapping
        CreateMap<EqualSplitCalculationDto, SharedExpenseParticipant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SharedExpenseId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => src.OwedAmount))
            .ForMember(dest => dest.SettledDate, opt => opt.Ignore())
            .ForMember(dest => dest.SharedExpense, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}