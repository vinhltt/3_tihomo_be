using AutoMapper;
using CoreFinance.Application.Services;
using CoreFinance.Application.Tests.Helpers;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    ///     Builds a <see cref="TransactionService" /> wired with the real create and update validators. (EN)<br />
    ///     Dựng <see cref="TransactionService" /> với validator tạo và cập nhật thật. (VI)
    /// </summary>
    private static TransactionService CreateService(IMapper mapper, IUnitOfWork unitOfWork,
        ILogger<TransactionService> logger)
    {
        return new TransactionService(mapper, unitOfWork, logger, new CreateTransactionRequestValidator(),
            new UpdateTransactionRequestValidator());
    }
}
