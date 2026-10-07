using AutoMapper;
using Bogus;
using CoreFinance.Application.Mapper;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;

namespace CoreFinance.Application.Tests.Helpers;

/// <summary>
///     Provides helper methods for creating fake data and common utilities for application tests. (EN)<br />
///     Cung cấp các phương thức trợ giúp để tạo dữ liệu giả và các tiện ích chung cho các bài kiểm thử ứng dụng. (VI)
/// </summary>
public static class TestHelpers
{
    /// <summary>
    ///     Creates and configures an AutoMapper instance with the application's mapping profile. (EN)<br />
    ///     Tạo và cấu hình một thể hiện AutoMapper với profile mapping của ứng dụng. (VI)
    /// </summary>
    /// <returns>
    ///     An initialized AutoMapper instance. (EN)<br />
    ///     Một thể hiện AutoMapper đã được khởi tạo. (VI)
    /// </returns>
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); }, NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    /// <summary>
    ///     Generates a specified number of fake Account objects. (EN)<br />
    ///     Sinh ra một số lượng tài khoản giả được chỉ định. (VI)
    /// </summary>
    /// <param name="count">
    ///     The number of fake accounts to generate. (EN)<br />
    ///     Số lượng tài khoản giả cần sinh. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of fake Account objects. (EN)<br />
    ///     Một IQueryable các đối tượng Account giả. (VI)
    /// </returns>
    public static IQueryable<Account> GenerateFakeAccounts(int count)
    {
        var faker = new Faker<Account>()
            .RuleFor(a => a.Id, _ => Guid.CreateVersion7())
            .RuleFor(a => a.UserId, _ => Guid.CreateVersion7())
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.Type, f => f.PickRandom<AccountType>())
            .RuleFor(a => a.Currency, f => f.Finance.Currency().Code)
            .RuleFor(a => a.InitialBalance, f => f.Finance.Amount())
            .RuleFor(a => a.CurrentBalance, f => f.Finance.Amount())
            .RuleFor(a => a.IsActive, f => f.Random.Bool());
        return faker.Generate(count).BuildMock();
    }

    /// <summary>
    ///     Generates a specified number of fake ExpectedTransaction objects. (EN)<br />
    ///     Sinh ra một số lượng giao dịch dự kiến giả được chỉ định. (VI)
    /// </summary>
    /// <param name="count">
    ///     The number of fake expected transactions to generate. (EN)<br />
    ///     Số lượng giao dịch dự kiến giả cần sinh. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of fake ExpectedTransaction objects. (EN)<br />
    ///     Một IQueryable các đối tượng ExpectedTransaction giả. (VI)
    /// </returns>
    public static IQueryable<ExpectedTransaction> GenerateFakeExpectedTransactions(int count)
    {
        var faker = new Faker<ExpectedTransaction>()
            .RuleFor(et => et.Id, _ => Guid.CreateVersion7())
            .RuleFor(et => et.UserId, _ => Guid.CreateVersion7())
            .RuleFor(et => et.AccountId, _ => Guid.CreateVersion7())
            .RuleFor(et => et.RecurringTransactionTemplateId, _ => Guid.CreateVersion7())
            .RuleFor(et => et.ExpectedDate, f => f.Date.Future())
            .RuleFor(et => et.ExpectedAmount, f => f.Finance.Amount(10))
            .RuleFor(et => et.OriginalAmount, (f, et) => f.Random.Bool() ? et.ExpectedAmount : null)
            .RuleFor(et => et.Description, f => f.Lorem.Sentence())
            .RuleFor(et => et.TransactionType, f => f.PickRandom<RecurringTransactionType>())
            .RuleFor(et => et.Category, f => f.Commerce.Categories(1).First())
            .RuleFor(et => et.Status, f => f.PickRandom<ExpectedTransactionStatus>())
            .RuleFor(et => et.IsAdjusted, f => f.Random.Bool())
            .RuleFor(et => et.AdjustmentReason, f => f.Random.Bool() ? f.Lorem.Sentence() : null)
            .RuleFor(et => et.ActualTransactionId, f => f.Random.Bool() ? Guid.CreateVersion7() : null)
            .RuleFor(et => et.GeneratedAt, f => f.Date.Past())
            .RuleFor(et => et.ProcessedAt, f => f.Random.Bool() ? f.Date.Recent() : null)
            .RuleFor(et => et.CreatedAt, f => f.Date.Past(2))
            .RuleFor(et => et.UpdatedAt, f => f.Date.Recent());
        var result = faker.Generate(count);
        return result.BuildMock();
    }

    /// <summary>
    ///     Generates a specified number of fake RecurringTransactionTemplate objects. (EN)<br />
    ///     Sinh ra một số lượng mẫu giao dịch định kỳ giả được chỉ định. (VI)
    /// </summary>
    /// <param name="count">
    ///     The number of fake recurring transaction templates to generate. (EN)<br />
    ///     Số lượng mẫu giao dịch định kỳ giả cần sinh. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of fake RecurringTransactionTemplate objects. (EN)<br />
    ///     Một IQueryable các đối tượng RecurringTransactionTemplate giả. (VI)
    /// </returns>
    public static IQueryable<RecurringTransactionTemplate> GenerateFakeRecurringTransactionTemplates(int count)
    {
        var faker = new Faker<RecurringTransactionTemplate>()
            .RuleFor(rt => rt.Id, _ => Guid.CreateVersion7())
            .RuleFor(rt => rt.UserId, _ => Guid.CreateVersion7())
            .RuleFor(rt => rt.AccountId, _ => Guid.CreateVersion7())
            .RuleFor(rt => rt.Name, f => f.Commerce.ProductName())
            .RuleFor(rt => rt.Description, f => f.Lorem.Sentence())
            .RuleFor(rt => rt.Amount, f => f.Finance.Amount(10))
            .RuleFor(rt => rt.TransactionType, f => f.PickRandom<RecurringTransactionType>())
            .RuleFor(rt => rt.Category, f => f.Commerce.Categories(1).First())
            .RuleFor(rt => rt.Frequency, f => f.PickRandom<RecurrenceFrequency>())
            .RuleFor(rt => rt.CustomIntervalDays, f => f.Random.Bool() ? f.Random.Int(1, 30) : null)
            .RuleFor(rt => rt.StartDate, f => f.Date.Past())
            .RuleFor(rt => rt.EndDate, f => f.Random.Bool() ? f.Date.Future() : null)
            .RuleFor(rt => rt.NextExecutionDate, f => f.Date.Future())
            .RuleFor(rt => rt.IsActive, f => f.Random.Bool())
            .RuleFor(rt => rt.AutoGenerate, f => f.Random.Bool())
            .RuleFor(rt => rt.DaysInAdvance, f => f.Random.Int(7, 90))
            .RuleFor(rt => rt.CreatedAt, f => f.Date.Past(2))
            .RuleFor(rt => rt.UpdatedAt, f => f.Date.Recent());
        return faker.Generate(count).BuildMock();
    }

    /// <summary>
    ///     Generates a specified number of fake Transaction objects. (EN)<br />
    ///     Sinh ra một số lượng giao dịch giả được chỉ định. (VI)
    /// </summary>
    /// <param name="count">
    ///     The number of fake transactions to generate. (EN)<br />
    ///     Số lượng giao dịch giả cần sinh. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of fake Transaction objects. (EN)<br />
    ///     Một IQueryable các đối tượng Transaction giả. (VI)
    /// </returns>
    public static IQueryable<Transaction> GenerateFakeTransactions(int count)
    {
        var faker = new Faker<Transaction>()
            .RuleFor(t => t.Id, _ => Guid.CreateVersion7())
            .RuleFor(t => t.UserId, _ => Guid.CreateVersion7())
            .RuleFor(t => t.AccountId, _ => Guid.CreateVersion7())
            .RuleFor(t => t.TransactionDate, f => f.Date.Past())
            .RuleFor(t => t.RevenueAmount, f => f.Finance.Amount(0, 5000))
            .RuleFor(t => t.SpentAmount, f => f.Finance.Amount(0, 3000))
            .RuleFor(t => t.Description, f => f.Lorem.Sentence())
            .RuleFor(t => t.Balance, f => f.Finance.Amount(100, 10000))
            .RuleFor(t => t.BalanceCompare, f => f.Random.Bool() ? f.Finance.Amount(100, 10000) : null)
            .RuleFor(t => t.AvailableLimit, f => f.Random.Bool() ? f.Finance.Amount(1000, 50000) : null)
            .RuleFor(t => t.AvailableLimitCompare, f => f.Random.Bool() ? f.Finance.Amount(1000, 50000) : null)
            .RuleFor(t => t.TransactionCode, f => f.Random.AlphaNumeric(10))
            .RuleFor(t => t.SyncMisa, f => f.Random.Bool())
            .RuleFor(t => t.SyncSms, f => f.Random.Bool())
            .RuleFor(t => t.Vn, f => f.Random.Bool())
            .RuleFor(t => t.CategorySummary, f => f.Commerce.Categories(1).First())
            .RuleFor(t => t.Note, f => f.Random.Bool() ? f.Lorem.Sentence() : null)
            .RuleFor(t => t.ImportFrom, f => f.Random.Bool() ? f.Company.CompanyName() : null)
            .RuleFor(t => t.IncreaseCreditLimit, f => f.Random.Bool() ? f.Finance.Amount() : null)
            .RuleFor(t => t.UsedPercent, f => f.Random.Bool() ? f.Random.Decimal(0, 100) : null)
            .RuleFor(t => t.CategoryType, f => f.PickRandom<CategoryType>())
            .RuleFor(t => t.Group, f => f.Random.Bool() ? f.Commerce.Department() : null)
            .RuleFor(t => t.CreatedAt, f => f.Date.Past(2))
            .RuleFor(t => t.UpdatedAt, f => f.Date.Recent())
            .RuleFor(t => t.CreatedAt, f => f.Date.Past(2))
            .RuleFor(t => t.UpdatedAt, f => f.Date.Recent())
            .RuleFor(t => t.CreateBy, f => f.Person.UserName)
            .RuleFor(t => t.UpdateBy, f => f.Person.UserName);
        return faker.Generate(count).BuildMock();
    }
}