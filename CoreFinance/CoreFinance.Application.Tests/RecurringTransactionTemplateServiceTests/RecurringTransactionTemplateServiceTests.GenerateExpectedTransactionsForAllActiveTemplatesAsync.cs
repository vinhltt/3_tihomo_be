using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using CoreFinance.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable;
using Moq;

namespace CoreFinance.Application.Tests.RecurringTransactionTemplateServiceTests;

public partial class RecurringTransactionTemplateServiceTests
{
    /// <summary>
    ///     (EN) Real DI scopes own independent child services and units of work; assertions observe generated windows.<br />
    ///     (VI) Scope DI thật quản lý service con và unit of work riêng; assertion quan sát cửa sổ đã sinh.
    /// </summary>
    private sealed class AllActiveFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        public Application.Services.RecurringTransactionTemplateService Service { get; }

        public AllActiveFixture(DateTime today, params GenerationHarness[] children)
        {
            var pending = new Queue<GenerationHarness>(children);
            _provider = new ServiceCollection()
                .AddScoped<GenerationHarness>(_ => pending.Dequeue())
                .AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GenerationHarness>().UnitOfWork.Object)
                .AddScoped<IRecurringTransactionTemplateService>(provider =>
                    CreateService(provider.GetRequiredService<IUnitOfWork>(), today: today))
                .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            var templateRepo = new Mock<IBaseRepository<RecurringTransactionTemplate, Guid>>();
            templateRepo.Setup(r => r.GetNoTrackingEntities())
                .Returns(() => children.Select(child => child.Template!).ToList().BuildMock());
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.Repository<RecurringTransactionTemplate, Guid>()).Returns(templateRepo.Object);
            Service = CreateService(unitOfWork.Object,
                scopeFactory: _provider.GetRequiredService<IServiceScopeFactory>());
        }

        public ValueTask DisposeAsync() => _provider.DisposeAsync();
    }

    /// <summary>
    ///     (EN) A failing template does not stop the others; the run reports the failure only after every template ran.<br />
    ///     (VI) Một mẫu lỗi không chặn các mẫu khác; lượt chạy chỉ báo lỗi sau khi mọi mẫu đã chạy.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldIsolateFailures_AndReportAfterRun()
    {
        // Arrange
        var today = D(2026, 3, 31);
        var first = new GenerationHarness
        {
            Template = NewTemplate(D(2026, 1, 31), cursor: D(2026, 2, 28))
        };
        var failing = new GenerationHarness { Template = NewTemplate(Today, RecurrenceFrequency.Custom) };
        failing.Template.CustomIntervalDays = 0;
        var last = new GenerationHarness { Template = NewTemplate(D(2026, 3, 17), RecurrenceFrequency.Weekly) };
        await using var fixture = new AllActiveFixture(today, first, failing, last);

        // Act
        var act = () => fixture.Service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        first.CreatedDates.Should().Equal(D(2026, 2, 28), D(2026, 3, 31));
        first.Template.NextExecutionDate.Should().Be(D(2026, 4, 30));
        failing.Created.Should().BeEmpty();
        failing.Template.NextExecutionDate.Should().Be(Today);
        last.CreatedDates.Should().Equal(D(2026, 3, 17), D(2026, 3, 24), D(2026, 3, 31));
        last.Template.NextExecutionDate.Should().Be(D(2026, 4, 7));
    }

    /// <summary>
    ///     (EN) A healthy run generates every child's exact window and advances each independent cursor.<br />
    ///     (VI) Lượt chạy thành công sinh đúng cửa sổ của từng mẫu và dời từng cursor độc lập.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldGenerateEveryWindow_WhenAllChildrenAreHealthy()
    {
        // Arrange
        var first = new GenerationHarness { Template = NewTemplate(Today, RecurrenceFrequency.Daily) };
        var last = new GenerationHarness
        {
            Template = NewTemplate(Today, RecurrenceFrequency.Weekly, daysInAdvance: 7)
        };
        await using var fixture = new AllActiveFixture(Today, first, last);

        // Act
        await fixture.Service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();

        // Assert
        first.CreatedDates.Should().Equal(Today);
        first.Template.NextExecutionDate.Should().Be(Today.AddDays(1));
        last.CreatedDates.Should().Equal(Today, Today.AddDays(7));
        last.Template.NextExecutionDate.Should().Be(Today.AddDays(14));
    }

    /// <summary>
    ///     (EN) Cancellation is propagated instead of being counted as a template failure.<br />
    ///     (VI) Việc hủy được lan truyền thay vì bị tính là lỗi của mẫu.
    /// </summary>
    [Fact]
    public async Task GenerateExpectedTransactionsForAllActiveTemplatesAsync_ShouldPropagate_WhenCancelled()
    {
        // Arrange
        var first = new GenerationHarness { Template = NewTemplate(Today, RecurrenceFrequency.Daily) };
        var cancelled = new GenerationHarness { Template = NewTemplate(Today, RecurrenceFrequency.Daily) };
        var last = new GenerationHarness { Template = NewTemplate(Today, RecurrenceFrequency.Daily) };
        using var cancellation = new CancellationTokenSource();
        cancelled.UnitOfWork.Setup(unitOfWork => unitOfWork.AcquireOwnerResourceLockAsync("recurring",
                cancelled.Template.Id, It.IsAny<CancellationToken>()))
            .Returns<string, Guid, CancellationToken>((_, _, token) =>
            {
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            });
        await using var fixture = new AllActiveFixture(Today, first, cancelled, last);

        // Act
        var act = () => fixture.Service.GenerateExpectedTransactionsForAllActiveTemplatesAsync(cancellation.Token);

        // Assert
        var failure = await act.Should().ThrowAsync<OperationCanceledException>();
        failure.Which.CancellationToken.Should().Be(cancellation.Token);
        first.CreatedDates.Should().Equal(Today);
        first.Template.NextExecutionDate.Should().Be(Today.AddDays(1));
        cancelled.Created.Should().BeEmpty();
        cancelled.Template.NextExecutionDate.Should().Be(Today);
        last.Created.Should().BeEmpty();
        last.Template.NextExecutionDate.Should().Be(Today);
    }
}
