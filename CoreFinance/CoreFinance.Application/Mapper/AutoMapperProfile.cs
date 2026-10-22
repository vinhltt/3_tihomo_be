using AutoMapper;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.Mapper;

/// <summary>
///     (EN) AutoMapper profile for mapping between domain entities and DTOs.<br />
///     (VI) Cấu hình AutoMapper để ánh xạ giữa các entity miền (domain) và DTO.
/// </summary>
public class AutoMapperProfile : Profile
{
    /// <summary>
    ///     (EN) Initializes a new instance of the <see cref="AutoMapperProfile" /> class.<br />
    ///     (VI) Khởi tạo một phiên bản mới của lớp <see cref="AutoMapperProfile" />.
    /// </summary>
    public AutoMapperProfile()
    {
        CreateMap<Account, AccountCreateRequest>().ReverseMap();
        CreateMap<Account, AccountUpdateRequest>().ReverseMap();
        CreateMap<Account, AccountViewModel>().ReverseMap();
        CreateMap<Account, AccountSelectionViewModel>().ReverseMap();
        CreateMap<Transaction, TransactionCreateRequest>().ReverseMap();
        CreateMap<Transaction, TransactionUpdateRequest>().ReverseMap();
        CreateMap<Transaction, TransactionViewModel>().ReverseMap();
        CreateMap<ExpectedTransaction, ExpectedTransactionCreateRequest>().ReverseMap()
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
        CreateMap<ExpectedTransaction, ExpectedTransactionUpdateRequest>();
        // Expected queries always supply this named parameter from BusinessCalendar, including CRUD responses.
        // Project navigation fields in SQL rather than materializing financial data owned by another user.
        DateTime todayUtc7 = default;
        CreateMap<ExpectedTransaction, ExpectedTransactionViewModel>()
            .ForMember(dest => dest.TemplateName,
                opt => opt.MapFrom(src =>
                    src.RecurringTransactionTemplate != null &&
                    src.RecurringTransactionTemplate.UserId == src.UserId
                        ? src.RecurringTransactionTemplate.Name : string.Empty))
            .ForMember(dest => dest.AccountName,
                opt => opt.MapFrom(src =>
                    src.Account != null && src.Account.UserId == src.UserId ? src.Account.Name : string.Empty))
            .ForMember(dest => dest.AccountType,
                opt => opt.MapFrom(src =>
                    src.Account != null && src.Account.UserId == src.UserId ? (AccountType?)src.Account.Type : null))
            .ForMember(dest => dest.HasActualTransaction, opt => opt.MapFrom(src => src.ActualTransactionId.HasValue))
            .ForMember(dest => dest.DisplayStatus,
                opt => opt.MapFrom(src => src.Status == ExpectedTransactionStatus.Pending &&
                                         src.ExpectedDate.Date < todayUtc7 ? 4 : (int)src.Status))
            .ForMember(dest => dest.DaysUntilDue,
                opt => opt.MapFrom(src => (src.ExpectedDate.Date - todayUtc7).Days));
        CreateMap<RecurringTransactionTemplate, RecurringTransactionTemplateCreateRequest>().ReverseMap();
        // (EN) Omitted schedule/required fields retain their stored values. Optional EndDate/Description/Category/Notes/
        // CronExpression use full-form clear-on-omission semantics because the Nuxt form omits empty values.
        // (VI) Trường lịch/bắt buộc bị bỏ qua giữ giá trị đã lưu. EndDate/Description/Category/Notes/CronExpression tùy chọn
        // dùng ngữ nghĩa full-form: bỏ trống thì xóa, vì form Nuxt không gửi giá trị rỗng.
        CreateMap<RecurringTransactionTemplate, RecurringTransactionTemplateUpdateRequest>().ReverseMap()
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
            .ForMember(dest => dest.CustomIntervalDays, opt => opt.Condition(src => src.CustomIntervalDays.HasValue))
            .ForMember(dest => dest.Amount, opt => opt.Condition(src => src.Amount.HasValue))
            .ForMember(dest => dest.TransactionType, opt => opt.Condition(src => src.TransactionType.HasValue))
            .ForMember(dest => dest.Frequency, opt => opt.Condition(src => src.Frequency.HasValue))
            .ForMember(dest => dest.StartDate, opt => opt.Condition(src => src.StartDate.HasValue))
            .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue))
            .ForMember(dest => dest.AutoGenerate, opt => opt.Condition(src => src.AutoGenerate.HasValue))
            .ForMember(dest => dest.DaysInAdvance, opt => opt.Condition(src => src.DaysInAdvance.HasValue));
        // Account details are exposed only when the linked account belongs to the template owner.
        CreateMap<RecurringTransactionTemplate, RecurringTransactionTemplateViewModel>()
            .ForMember(dest => dest.AccountName,
                opt => opt.MapFrom(src =>
                    src.Account != null && src.Account.UserId == src.UserId ? src.Account.Name : string.Empty))
            .ForMember(dest => dest.AccountType,
                opt => opt.MapFrom(src =>
                    src.Account != null && src.Account.UserId == src.UserId ? (AccountType?)src.Account.Type : null))
            .ForMember(dest => dest.ExpectedTransactionCount, opt => opt.MapFrom(src => 0)) // Default value for test
            .ReverseMap()
            .ForMember(dest => dest.Account, opt => opt.Ignore());
    }
}