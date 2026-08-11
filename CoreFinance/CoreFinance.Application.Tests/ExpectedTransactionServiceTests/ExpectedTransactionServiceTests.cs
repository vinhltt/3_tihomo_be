using AutoMapper;
using CoreFinance.Application.Tests.Helpers;

namespace CoreFinance.Application.Tests.ExpectedTransactionServiceTests;

/// <summary>
///     Base partial class for ExpectedTransactionService test classes, providing shared setup like the AutoMapper
///     instance. (EN)<br />
///     Lớp partial cơ sở cho các lớp kiểm thử ExpectedTransactionService, cung cấp các thiết lập chung như thể hiện
///     AutoMapper. (VI)
/// </summary>
public partial class ExpectedTransactionServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();
}