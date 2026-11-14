using AutoMapper;
using CoreFinance.Application.Services;
using CoreFinance.Application.Tests.Helpers;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Base partial class for AccountService test classes, providing shared setup like the AutoMapper instance. (EN)<br />
///     Lớp partial cơ sở cho các lớp kiểm thử AccountService, cung cấp các thiết lập chung như thể hiện AutoMapper. (VI)
/// </summary>
public partial class AccountServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();

    /// <summary>
    ///     Builds an <see cref="AccountService" /> wired with the real create and update validators. (EN)<br />
    ///     Dựng <see cref="AccountService" /> với validator tạo và cập nhật thật. (VI)
    /// </summary>
    private static AccountService CreateService(IMapper mapper, IUnitOfWork unitOfWork,
        ILogger<AccountService> logger)
    {
        return new AccountService(mapper, unitOfWork, logger, new CreateAccountRequestValidator(),
            new UpdateAccountRequestValidator());
    }
}
