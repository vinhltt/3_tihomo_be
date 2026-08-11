using AutoMapper;
using CoreFinance.Application.Tests.Helpers;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

/// <summary>
///     Base partial class for RecurringTransactionTemplateService test classes, providing shared setup like the AutoMapper
///     instance. (EN)<br />
///     Lớp partial cơ sở cho các lớp kiểm thử RecurringTransactionTemplateService, cung cấp các thiết lập chung như thể
///     hiện AutoMapper. (VI)
/// </summary>
public partial class RecurringTransactionTemplateServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();
}