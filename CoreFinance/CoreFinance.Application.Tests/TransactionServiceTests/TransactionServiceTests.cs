using AutoMapper;
using CoreFinance.Application.Tests.Helpers;

namespace CoreFinance.Application.Tests.TransactionServiceTests;

/// <summary>
///     Base partial class for TransactionService test classes, providing shared setup like the AutoMapper instance. (EN)
///     <br />
///     Lớp partial cơ sở cho các lớp kiểm thử TransactionService, cung cấp các thiết lập chung như thể hiện AutoMapper.
///     (VI)
/// </summary>
public partial class TransactionServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();
}