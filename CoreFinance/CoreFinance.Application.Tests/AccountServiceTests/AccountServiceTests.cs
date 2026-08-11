using AutoMapper;
using CoreFinance.Application.Tests.Helpers;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Base partial class for AccountService test classes, providing shared setup like the AutoMapper instance. (EN)<br />
///     Lớp partial cơ sở cho các lớp kiểm thử AccountService, cung cấp các thiết lập chung như thể hiện AutoMapper. (VI)
/// </summary>
public partial class AccountServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();
}