using AutoMapper;
using CoreFinance.Application.Tests.Helpers;
using CoreFinance.Application.Interfaces;
using System.Linq.Expressions;
using CoreFinance.Application.Services;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using Shared.EntityFramework.BaseEfModels;

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

    private static readonly DateTime Today = new(2026, 10, 9);
    private static readonly DateTimeOffset ClockInstant = new(2026, 10, 8, 17, 30, 0, TimeSpan.Zero);
    private static readonly Guid OwnerId = Id(900);
    private static readonly Guid OtherOwnerId = Id(999);
    private static readonly Guid AccountId = Id(901);
    private static readonly Guid TemplateId = Id(902);
    private static readonly Guid ActualId = Id(903);

    private static Guid Id(int value) => new(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    private static ExpectedTransaction Expected(int id, int days = 0,
        ExpectedTransactionStatus status = ExpectedTransactionStatus.Pending, decimal amount = 100m,
        RecurringTransactionType type = RecurringTransactionType.Expense) => new()
    {
        Id = Id(id),
        UserId = OwnerId,
        AccountId = AccountId,
        RecurringTransactionTemplateId = TemplateId,
        ExpectedDate = Today.AddDays(days),
        ExpectedAmount = amount,
        Status = status,
        TransactionType = type,
        Description = "Electricity bill",
        Category = "Utilities",
        Notes = "Keep the receipt",
        GeneratedAt = Today.AddDays(-10),
        CreatedAt = Today.AddDays(-10),
        UpdatedAt = Today.AddDays(-10),
        IsAdjusted = false,
        OriginalAmount = null,
        AdjustmentReason = null,
        ActualTransactionId = null,
        ProcessedAt = null,
        IsDeleted = null
    };

    private ServiceFixture CreateFixture(params ExpectedTransaction[] rows) => new(_mapper, rows);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    /// <summary>
    ///     (EN) Persistence-boundary mocks for service rules, not database isolation, SQL, or rollback proof.<br />
    ///     (VI) Mock biên lưu trữ để kiểm tra quy tắc dịch vụ, không chứng minh cách ly, SQL hay rollback cơ sở dữ liệu.
    /// </summary>
    private sealed class ServiceFixture
    {
        public List<ExpectedTransaction> ExpectedTransactions { get; }
        public List<Account> Accounts { get; } =
        [
            new()
            {
                Id = AccountId, UserId = OwnerId, Name = "Daily account", CurrentBalance = 5000m, IsActive = true
            }
        ];
        public List<RecurringTransactionTemplate> Templates { get; } =
        [
            new() { Id = TemplateId, UserId = OwnerId, Name = "Electricity" }
        ];
        public List<Transaction> ActualTransactions { get; } = [];
        public List<Transaction> CreatedActualTransactions { get; } = [];
        public List<ExpectedTransaction> CreatedExpectedTransactions { get; } = [];
        public Mock<IBaseRepository<ExpectedTransaction, Guid>> ExpectedRepository { get; }
        public Mock<IBaseRepository<Account, Guid>> AccountRepository { get; }
        public Mock<IBaseRepository<RecurringTransactionTemplate, Guid>> TemplateRepository { get; }
        public Mock<IBaseRepository<Transaction, Guid>> ActualRepository { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IDbContextTransaction> Transaction { get; } = new();
        public ExpectedTransactionService Service { get; }

        public ServiceFixture(IMapper mapper, IEnumerable<ExpectedTransaction> rows, DateTimeOffset? utcNow = null,
            ITransactionService? transactionService = null)
        {
            ExpectedTransactions = rows.ToList();
            ExpectedRepository = RepositoryMock(ExpectedTransactions);
            AccountRepository = RepositoryMock(Accounts);
            TemplateRepository = RepositoryMock(Templates);
            ActualRepository = RepositoryMock(ActualTransactions);

            ExpectedRepository.Setup(r => r.CreateAsync(It.IsAny<ExpectedTransaction>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ExpectedTransaction, CancellationToken>((entity, _) => CaptureExpected(entity))
                .ReturnsAsync(1);
            ExpectedRepository.Setup(r => r.CreateAsync(It.IsAny<List<ExpectedTransaction>>()))
                .Callback<List<ExpectedTransaction>>(entities =>
                {
                    foreach (var entity in entities) CaptureExpected(entity);
                })
                .ReturnsAsync((List<ExpectedTransaction> entities) => entities.Count);
            ExpectedRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpectedTransaction>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            ActualRepository.Setup(r => r.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .Callback<Transaction, CancellationToken>((entity, _) =>
                {
                    entity.Id = Id(2000 + CreatedActualTransactions.Count);
                    entity.UserId = OwnerId;
                    CreatedActualTransactions.Add(entity);
                    ActualTransactions.Add(entity);
                })
                .ReturnsAsync(1);

            UnitOfWork.Setup(u => u.Repository<ExpectedTransaction, Guid>()).Returns(ExpectedRepository.Object);
            UnitOfWork.Setup(u => u.Repository<Account, Guid>()).Returns(AccountRepository.Object);
            UnitOfWork.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(TemplateRepository.Object);
            UnitOfWork.Setup(u => u.Repository<Transaction, Guid>()).Returns(ActualRepository.Object);
            UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            UnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Transaction.Object);
            Transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            Transaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);

            transactionService ??= new TransactionService(mapper, UnitOfWork.Object,
                new Mock<ILogger<TransactionService>>().Object);
            Service = new ExpectedTransactionService(mapper, UnitOfWork.Object,
                new Mock<ILogger<ExpectedTransactionService>>().Object, transactionService,
                new BusinessCalendar(new FixedTimeProvider(utcNow ?? ClockInstant)));
        }

        private void CaptureExpected(ExpectedTransaction entity)
        {
            // Identity/owner stamps model the persistence boundary, not authorization or durable atomicity.
            entity.Id = Id(1000 + CreatedExpectedTransactions.Count);
            entity.UserId = OwnerId;
            CreatedExpectedTransactions.Add(entity);
            ExpectedTransactions.Add(entity);
        }

        private static Mock<IBaseRepository<TEntity, Guid>> RepositoryMock<TEntity>(List<TEntity> rows)
            where TEntity : UserOwnedEntity<Guid>
        {
            var repository = new Mock<IBaseRepository<TEntity, Guid>>();
            repository.Setup(r => r.GetNoTrackingEntities(It.IsAny<Expression<Func<TEntity, object>>[]>()))
                .Returns(() => rows.BuildMock());
            repository.Setup(r => r.GetQueryableTable(It.IsAny<Expression<Func<TEntity, object>>[]>()))
                .Returns(() => rows.BuildMock());
            repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<TEntity, object>>[]>()))
                .ReturnsAsync((Guid id, Expression<Func<TEntity, object>>[] _) =>
                    rows.SingleOrDefault(row => row.Id == id));
            repository.Setup(r => r.GetByIdNoTrackingAsync(It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<TEntity, object>>[]>()))
                .ReturnsAsync((Guid id, Expression<Func<TEntity, object>>[] _) =>
                    rows.SingleOrDefault(row => row.Id == id));
            return repository;
        }
    }
}