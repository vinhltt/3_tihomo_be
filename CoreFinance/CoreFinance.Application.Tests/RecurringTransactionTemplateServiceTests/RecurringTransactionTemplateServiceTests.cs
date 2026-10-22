using System.Linq.Expressions;
using AutoMapper;
using CoreFinance.Application.Services;
using CoreFinance.Application.Tests.Helpers;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

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

    /// <summary>
    ///     (EN) Default fixed "today" in UTC+7.<br />
    ///     (VI) "Hôm nay" cố định mặc định theo UTC+7.
    /// </summary>
    private static readonly DateTime Today = new(2026, 3, 15);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private static BusinessCalendar FixedCalendar(DateTime today) =>
        new(new FixedTimeProvider(new DateTimeOffset(today.AddHours(12), BusinessCalendar.Offset)));

    private static DateTime D(int year, int month, int day) => new(year, month, day);

    private static long _templateIdSequence;

    /// <summary>
    ///     (EN) Strictly increasing version-7 ids, so the service's ordering by id matches creation order.<br />
    ///     (VI) Id phiên bản 7 tăng nghiêm ngặt, để thứ tự theo id của service khớp thứ tự tạo.
    /// </summary>
    private static Guid NextTemplateId() =>
        Guid.CreateVersion7(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMilliseconds(Interlocked.Increment(ref _templateIdSequence)));

    /// <summary>
    ///     (EN) Builds the service over mocks. Persistence-only behaviour (locking, owner scoping, uniqueness, rollback) is
    ///     not provable here and is covered by real-PostgreSQL proof instead.<br />
    ///     (VI) Tạo service trên mock. Hành vi chỉ thuộc tầng lưu trữ (khóa, owner, unique, rollback) không chứng minh được ở
    ///     đây và được kiểm bằng PostgreSQL thật.
    /// </summary>
    private static RecurringTransactionTemplateService CreateService(IUnitOfWork unitOfWork,
        ILogger<RecurringTransactionTemplateService>? logger = null, DateTime? today = null,
        IServiceScopeFactory? scopeFactory = null)
    {
        return new RecurringTransactionTemplateService(TestHelpers.CreateMapper(), unitOfWork,
            logger ?? new Mock<ILogger<RecurringTransactionTemplateService>>().Object,
            FixedCalendar(today ?? Today),
            scopeFactory ?? new Mock<IServiceScopeFactory>().Object);
    }

    /// <summary>
    ///     (EN) Monthly auto-generate template owned by the account in <see cref="GenerationHarness.OwnedAccounts" />.<br />
    ///     (VI) Mẫu tự sinh hằng tháng gắn với tài khoản trong <see cref="GenerationHarness.OwnedAccounts" />.
    /// </summary>
    private static RecurringTransactionTemplate NewTemplate(DateTime startDate,
        RecurrenceFrequency frequency = RecurrenceFrequency.Monthly, DateTime? cursor = null, int daysInAdvance = 0)
    {
        return new RecurringTransactionTemplate
        {
            Id = NextTemplateId(),
            UserId = Guid.CreateVersion7(),
            AccountId = GenerationHarness.AccountId,
            Name = "Electricity",
            Amount = 100m,
            TransactionType = RecurringTransactionType.Expense,
            Frequency = frequency,
            StartDate = startDate,
            NextExecutionDate = cursor ?? startDate,
            IsActive = true,
            AutoGenerate = true,
            DaysInAdvance = daysInAdvance
        };
    }

    /// <summary>
    ///     (EN) In-memory stand-in for the unit of work, repositories. Created periods are captured in
    ///     <see cref="Created" />; <see cref="ExistingDates" /> plays already generated rows (any status, tombstones).<br />
    ///     (VI) Bản giả trong bộ nhớ cho unit of work và repository. Các kỳ được tạo nằm trong
    ///     <see cref="Created" />; <see cref="ExistingDates" /> đóng vai các kỳ đã sinh (mọi trạng thái, cả xóa mềm).
    /// </summary>
    private sealed class GenerationHarness
    {
        public static readonly Guid AccountId = Guid.CreateVersion7();

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IBaseRepository<RecurringTransactionTemplate, Guid>> TemplateRepo { get; } = new();
        public Mock<IBaseRepository<ExpectedTransaction, Guid>> ExpectedRepo { get; } = new();
        public Mock<IBaseRepository<Account, Guid>> AccountRepo { get; } = new();
        public Mock<IDbContextTransaction> Transaction { get; } = new();
        public List<ExpectedTransaction> Created { get; } = [];
        public List<DateTime> ExistingDates { get; } = [];
        public List<Account> OwnedAccounts { get; } = [new Account { Id = AccountId, Name = "Wallet" }];
        public RecurringTransactionTemplate? Template { get; set; }

        public GenerationHarness()
        {
            UnitOfWork.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(TemplateRepo.Object);
            UnitOfWork.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(ExpectedRepo.Object);
            UnitOfWork.Setup(u => u.Repository<Account, Guid>()).Returns(AccountRepo.Object);
            UnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Transaction.Object);
            Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Transaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

            UnitOfWork.Setup(u => u.AcquireOwnerResourceLockAsync("recurring", It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            AccountRepo.Setup(r => r.GetNoTrackingEntities()).Returns(() => OwnedAccounts.BuildMock());
            ExpectedRepo.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(), It.IsAny<CancellationToken>()))
                .Callback<ExpectedTransaction, CancellationToken>((e, _) => Created.Add(e))
                .ReturnsAsync(1);
            TemplateRepo.Setup(r => r.CreateAsync(It.IsAny<RecurringTransactionTemplate>(), It.IsAny<CancellationToken>()))
                .Callback<RecurringTransactionTemplate, CancellationToken>((template, _) => Template = template)
                .ReturnsAsync(1);
            TemplateRepo.Setup(r => r.UpdateAsync(It.IsAny<RecurringTransactionTemplate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            TemplateRepo.Setup(r => r.DeleteSoftAsync(It.IsAny<RecurringTransactionTemplate>()))
                .Callback<RecurringTransactionTemplate>(template => template.IsDeleted = "20260315120000")
                .ReturnsAsync(1);

            TemplateRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<RecurringTransactionTemplate, object>>[]>()))
                .ReturnsAsync(() => Template);
            TemplateRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<RecurringTransactionTemplate, object>>[]>()))
                .ReturnsAsync(() => Template);
            ExpectedRepo.Setup(r => r.GetNoTrackingEntities())
                .Returns(() => ExistingDates
                    .Select(date => new ExpectedTransaction
                    {
                        RecurringTransactionTemplateId = Template?.Id, ExpectedDate = date
                    })
                    .ToList()
                    .BuildMock());
        }

        public RecurringTransactionTemplateService Service(DateTime? today = null,
            ILogger<RecurringTransactionTemplateService>? logger = null) =>
            CreateService(UnitOfWork.Object, logger, today);

        public IEnumerable<DateTime> CreatedDates => Created.Select(e => e.ExpectedDate);
    }
}
