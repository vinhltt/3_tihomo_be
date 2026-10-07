using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanningInvestment.Domain.Entities;
using PlanningInvestment.Domain.Enums;
using PlanningInvestment.Infrastructure;
using Tests.Shared.Helpers;

namespace PlanningInvestment.Application.Tests;

/// <summary>
///     Characterization: Debt persistence through PlanningInvestmentDbContext on real PostgreSQL (EN)<br/>
///     Characterization: lưu Debt qua PlanningInvestmentDbContext trên PostgreSQL thật (VI)
/// </summary>
/// <remarks>There is no Debt API in the baseline; this does not claim one.</remarks>
[Trait("Category", "Characterization")]
public class DebtPersistenceParityTests
{
    private static PlanningInvestmentDbContext NewContext() => new(
        new DbContextOptionsBuilder<PlanningInvestmentDbContext>()
            .UseNpgsql(ExternalBackend.Database("TIHOMO_TEST_DB_PLANNING"))
            .UseSnakeCaseNamingConvention()
            .Options,
        new ConfigurationBuilder().Build());

    [Fact]
    public async Task Saved_Debt_Should_Be_Read_Back_By_A_New_Context_With_Derived_Values()
    {
        var userId = Guid.CreateVersion7();
        var dueDate = new DateTime(2027, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2026, 10, 5, 1, 2, 3, DateTimeKind.Utc);
        // No Debt API and no SaveChanges audit hook in the baseline: the caller owns audit fields (UTC for timestamptz)
        var debt = new Debt
        {
            Id = Guid.CreateVersion7(), UserId = userId, Name = "Characterization loan", DebtType = DebtType.PersonalLoan,
            OriginalAmount = 1000m, CurrentBalance = 750m, InterestRate = 12.50m, DueDate = dueDate,
            CreatedAt = createdAt, UpdatedAt = createdAt, CreateBy = userId.ToString()
        };
        await using (var write = NewContext())
        {
            write.Debts.Add(debt);
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var stored = await read.Debts.AsNoTracking().SingleAsync(d => d.Id == debt.Id);

        stored.UserId.Should().Be(userId);
        stored.Name.Should().Be("Characterization loan");
        stored.DebtType.Should().Be(DebtType.PersonalLoan);
        stored.OriginalAmount.Should().Be(1000m);
        stored.CurrentBalance.Should().Be(750m);
        stored.InterestRate.Should().Be(12.50m);
        stored.DueDate.Should().Be(dueDate);
        stored.DueDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        stored.IsActive.Should().BeTrue();
        stored.CreateBy.Should().Be(userId.ToString());
        stored.CreatedAt.Should().Be(createdAt);
        stored.AmountPaid.Should().Be(250m);
        stored.PaymentProgress.Should().Be(25m);
        stored.IsFullyPaid.Should().BeFalse();
    }
}
