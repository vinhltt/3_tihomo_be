using System.Globalization;
using System.Net;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     (EN) Real HTTP + PostgreSQL proof that the CRUD create and update routes reject rule-breaking input with a
///     400 validation problem and write nothing, while the form payloads the frontend sends keep succeeding.<br />
///     (VI) Bằng chứng HTTP + PostgreSQL thật rằng các route tạo và cập nhật CRUD từ chối đầu vào vi phạm rule bằng
///     400 validation problem và không ghi gì, trong khi payload form mà frontend gửi vẫn thành công.
/// </summary>
[Trait("Category", "Characterization")]
public sealed class CrudValidationProblemPostgresTests
{
    private const string AccountRoute = "/api/Account";
    private const string TransactionRoute = "/api/Transaction";
    private const string TemplateRoute = "/api/RecurringTransactionTemplate";
    private const string ProblemMediaType = "application/problem+json";

    /// <summary>One invalid input: the form field to overwrite (null value removes it) and the error key expected back.</summary>
    private sealed record InvalidInput(string Field, string? Value);

    /// <summary>
    ///     (EN) Invalid account creation answers 400 problem details keyed by the broken field and inserts no row.<br />
    ///     (VI) Tạo tài khoản không hợp lệ trả 400 problem details theo field vi phạm và không thêm row nào.
    /// </summary>
    [Fact]
    public async Task Account_Post_Should_Return_400_Problem_Details_And_Insert_Nothing_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-account-post");
        var validForm = AccountForm();
        // Type "99" is out of the enum range: the 400 comes from [ApiController] model binding before the service runs;
        // the service-level IsInEnum rule is proven by unit theories.
        InvalidInput[] cases =
        [
            new("Name", ""), new("Type", "99"), new("Currency", ""), new("Code", "Bad Code")
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PostFormAsync(AccountRoute, form), validForm, cases);
        (await CountAccountsAsync(fixture)).Should().Be(0, "a rejected create must not insert an account");
    }

    /// <summary>
    ///     (EN) Invalid account update answers 400 problem details keyed by the broken field and leaves the row unchanged.<br />
    ///     (VI) Cập nhật tài khoản không hợp lệ trả 400 problem details theo field vi phạm và giữ nguyên row.
    /// </summary>
    [Fact]
    public async Task Account_Put_Should_Return_400_Problem_Details_And_Keep_The_Row_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-account-put");
        var id = await api.CreateAccountAsync("Original account");
        var before = await ReadAccountAsync(fixture, id);
        var validForm = new Dictionary<string, string>
        {
            ["Id"] = id.ToString(), ["Name"] = "Edited account", ["Type"] = "Bank", ["Currency"] = "VND"
        };
        // Type is a string on the update DTO, so both cases reach the validator (NotEmpty, then defined-enum-name):
        // an empty value binds to null and an omitted field stays null.
        InvalidInput[] cases =
        [
            new("Name", ""), new("Type", "NotAnAccountType"), new("Currency", ""), new("Code", "Bad Code"),
            new("Type", ""), new("Type", null)
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PutFormAsync($"{AccountRoute}/{id}", form), validForm, cases);
        (await ReadAccountAsync(fixture, id)).Should().BeEquivalentTo(before, "a rejected update must not change the row");
    }

    /// <summary>
    ///     (EN) Invalid transaction creation answers 400 problem details keyed by the broken field and inserts no row.<br />
    ///     (VI) Tạo giao dịch không hợp lệ trả 400 problem details theo field vi phạm và không thêm row nào.
    /// </summary>
    [Fact]
    public async Task Transaction_Post_Should_Return_400_Problem_Details_And_Insert_Nothing_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-transaction-post");
        var accountId = await api.CreateAccountAsync("Transaction account");
        var validForm = TransactionForm(accountId, fixture.Today);
        // CategoryType "99" is out of the enum range: the 400 comes from [ApiController] model binding before the
        // service runs; the service-level IsInEnum rule is proven by unit theories.
        InvalidInput[] cases =
        [
            new("AccountId", Guid.Empty.ToString()), new("TransactionDate", null), new("SpentAmount", "-1"),
            new("CategoryType", "99"), new("RevenueAmount", "-1")
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PostFormAsync(TransactionRoute, form), validForm, cases);
        (await CountTransactionsAsync(fixture)).Should().Be(0, "a rejected create must not insert a transaction");
    }

    /// <summary>
    ///     (EN) Invalid transaction update answers 400 problem details keyed by the broken field and leaves the row unchanged.<br />
    ///     (VI) Cập nhật giao dịch không hợp lệ trả 400 problem details theo field vi phạm và giữ nguyên row.
    /// </summary>
    [Fact]
    public async Task Transaction_Put_Should_Return_400_Problem_Details_And_Keep_The_Row_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-transaction-put");
        var accountId = await api.CreateAccountAsync("Transaction account");
        using var created = await api.PostFormAsync(TransactionRoute, TransactionForm(accountId, fixture.Today));
        created.StatusCode.Should().Be(HttpStatusCode.OK, await created.Content.ReadAsStringAsync());
        var id = await BackendFinanceClient.IdOf(created);
        var before = await BackendFinanceClient.ReadTransactionRowAsync(id, fixture.ConnectionString);
        before.Should().NotBeNull();
        var validForm = TransactionForm(accountId, fixture.Today);
        validForm["Id"] = id.ToString();
        validForm["Description"] = "Edited description";
        // CategoryType "99" is out of the enum range: the 400 comes from [ApiController] model binding before the
        // service runs; the service-level IsInEnum rule is proven by unit theories.
        InvalidInput[] cases =
        [
            new("AccountId", Guid.Empty.ToString()), new("TransactionDate", null), new("SpentAmount", "-1"),
            new("CategoryType", "99"), new("RevenueAmount", "-1")
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PutFormAsync($"{TransactionRoute}/{id}", form), validForm, cases);
        var after = await BackendFinanceClient.ReadTransactionRowAsync(id, fixture.ConnectionString);
        after.Should().BeEquivalentTo(before, "a rejected update must not change the row");
    }

    /// <summary>
    ///     (EN) Invalid template creation answers 400 problem details keyed by the broken field and inserts no row.<br />
    ///     (VI) Tạo mẫu không hợp lệ trả 400 problem details theo field vi phạm và không thêm row nào.
    /// </summary>
    [Fact]
    public async Task Template_Post_Should_Return_400_Problem_Details_And_Insert_Nothing_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-template-post");
        var accountId = await api.CreateAccountAsync("Template account");
        var validForm = TemplateForm(accountId, fixture.Today, "New template", daysInAdvance: "7");
        var defaultStartDate = default(DateTime).ToString("O", CultureInfo.InvariantCulture);
        // Frequency and TransactionType "99" are out of the enum range: the 400 comes from [ApiController] model
        // binding before the service runs; the service-level IsInEnum rules are proven by unit theories.
        InvalidInput[] cases =
        [
            new("Name", ""), new("Amount", "0"), new("DaysInAdvance", "0"), new("Frequency", "99"),
            new("AccountId", Guid.Empty.ToString()), new("StartDate", defaultStartDate),
            new("TransactionType", "99"), new("CustomIntervalDays", "0")
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PostFormAsync(TemplateRoute, form), validForm, cases);
        (await CountTemplatesAsync(fixture)).Should().Be(0, "a rejected create must not insert a template");
    }

    /// <summary>
    ///     (EN) Invalid template update answers 400 problem details keyed by the broken field and leaves the row unchanged.<br />
    ///     (VI) Cập nhật mẫu không hợp lệ trả 400 problem details theo field vi phạm và giữ nguyên row.
    /// </summary>
    [Fact]
    public async Task Template_Put_Should_Return_400_Problem_Details_And_Keep_The_Row_When_Input_Breaks_A_Rule()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-template-put");
        var id = await CreateTemplateAsync(api, fixture, "Original template");
        var before = await ReadTemplateAsync(fixture, id);
        var validForm = new Dictionary<string, string> { ["Id"] = id.ToString() };
        var defaultStartDate = default(DateTime).ToString("O", CultureInfo.InvariantCulture);
        // Frequency and TransactionType "99" are out of the enum range: the 400 comes from [ApiController] model
        // binding before the service runs; the service-level IsInEnum rules are proven by unit theories.
        // The update DTO has no AccountId, so there is no AccountId 400 case on PUT; the ignored field is pinned by
        // Template_Put_Should_Return_200_And_Keep_The_Account_When_AccountId_Is_Sent. StartDate and CustomIntervalDays
        // only validate when supplied, which these cases do.
        InvalidInput[] cases =
        [
            new("Name", new string('n', 201)), new("Amount", "0"), new("DaysInAdvance", "0"), new("Frequency", "99"),
            new("StartDate", defaultStartDate), new("TransactionType", "99"), new("CustomIntervalDays", "0")
        ];

        // Act + Assert
        await AssertEachRejectedAsync(form => api.PutFormAsync($"{TemplateRoute}/{id}", form), validForm, cases);
        (await ReadTemplateAsync(fixture, id)).Should().BeEquivalentTo(before, "a rejected update must not change the row");
    }

    /// <summary>
    ///     (EN) An empty or whitespace-only template name binds as null, which means "keep the name": the update succeeds
    ///     and the stored name stays.<br />
    ///     (VI) Tên mẫu rỗng hoặc toàn khoảng trắng được bind thành null, nghĩa là "giữ tên": cập nhật thành công và tên
    ///     đã lưu không đổi.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Template_Put_Should_Return_200_And_Keep_The_Name_When_Name_Is_Empty_Or_Whitespace(string name)
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-template-blank-name");
        var id = await CreateTemplateAsync(api, fixture, "Original template");

        // Act
        using var response = await api.PutFormAsync($"{TemplateRoute}/{id}",
            new Dictionary<string, string> { ["Id"] = id.ToString(), ["Name"] = name });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        (await ReadTemplateAsync(fixture, id)).Name.Should().Be("Original template");
    }

    /// <summary>
    ///     (EN) The template update DTO has no AccountId, so an AccountId sent on PUT is ignored: the update succeeds and
    ///     the template stays on its original account.<br />
    ///     (VI) DTO cập nhật mẫu không có AccountId nên AccountId gửi kèm khi PUT bị bỏ qua: cập nhật thành công và mẫu
    ///     vẫn thuộc tài khoản ban đầu.
    /// </summary>
    [Fact]
    public async Task Template_Put_Should_Return_200_And_Keep_The_Account_When_AccountId_Is_Sent()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-template-put-account-id");
        var id = await CreateTemplateAsync(api, fixture, "Original template");
        var originalAccountId = await ReadTemplateAccountIdAsync(fixture, id);
        var otherAccountId = await api.CreateAccountAsync("Other account");
        originalAccountId.Should().NotBeNull().And.NotBe(otherAccountId);

        // Act
        using var response = await api.PutFormAsync($"{TemplateRoute}/{id}", new Dictionary<string, string>
        {
            ["Id"] = id.ToString(), ["Name"] = "Edited template", ["AccountId"] = otherAccountId.ToString()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        (await ReadTemplateAsync(fixture, id)).Name.Should().Be("Edited template", "the update itself was applied");
        (await ReadTemplateAccountIdAsync(fixture, id)).Should().Be(originalAccountId,
            "AccountId is not part of the update form, so it must not move the template");
    }

    /// <summary>
    ///     (EN) Form payloads shaped like the frontend sends them (typed account, negative opening and running balances,
    ///     a 30-day template window) are accepted.<br />
    ///     (VI) Payload form có dạng frontend gửi (tài khoản có loại, số dư đầu kỳ và số dư âm, cửa sổ mẫu 30 ngày) được
    ///     chấp nhận.
    /// </summary>
    [Fact]
    public async Task Post_Should_Return_200_When_Payload_Has_The_Shape_The_Frontend_Sends()
    {
        // Arrange
        await using var fixture = await Thm3PostgresFixture.CreateAsync();
        using var api = await fixture.LoginAsync("crud-validation-frontend-shape");
        var accountForm = AccountForm();
        accountForm["InitialBalance"] = "-100";

        // Act
        using var accountResponse = await api.PostFormAsync(AccountRoute, accountForm);
        var accountContent = await accountResponse.Content.ReadAsStringAsync();
        accountResponse.StatusCode.Should().Be(HttpStatusCode.OK, accountContent);
        var accountId = await BackendFinanceClient.IdOf(accountResponse);

        var transactionForm = TransactionForm(accountId, fixture.Today);
        transactionForm["Balance"] = "-1";
        using var transactionResponse = await api.PostFormAsync(TransactionRoute, transactionForm);
        var transactionContent = await transactionResponse.Content.ReadAsStringAsync();

        var templateForm = TemplateForm(accountId, fixture.Today, "Frontend template", daysInAdvance: "30");
        using var templateResponse = await api.PostFormAsync(TemplateRoute, templateForm);
        var templateContent = await templateResponse.Content.ReadAsStringAsync();

        // Assert
        transactionResponse.StatusCode.Should().Be(HttpStatusCode.OK, transactionContent);
        templateResponse.StatusCode.Should().Be(HttpStatusCode.OK, templateContent);
        (await ReadAccountAsync(fixture, accountId)).InitialBalance.Should().Be(-100m);
        var transactionRow = await BackendFinanceClient.ReadTransactionRowAsync(
            await BackendFinanceClient.IdOf(transactionResponse), fixture.ConnectionString);
        transactionRow.Should().NotBeNull();
        transactionRow!["balance"].Should().Be(-1m);
        (await ReadTemplateAsync(fixture, await BackendFinanceClient.IdOf(templateResponse))).DaysInAdvance.Should().Be(30);
    }

    private static async Task AssertEachRejectedAsync(Func<Dictionary<string, string>, Task<HttpResponseMessage>> send,
        Dictionary<string, string> validForm, IEnumerable<InvalidInput> cases)
    {
        foreach (var invalid in cases)
        {
            var form = new Dictionary<string, string>(validForm);
            if (invalid.Value is null) form.Remove(invalid.Field);
            else form[invalid.Field] = invalid.Value;

            using var response = await send(form);
            var content = await response.Content.ReadAsStringAsync();
            var because = $"{invalid.Field}='{invalid.Value}' breaks a rule; body: {content}";

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because);
            response.Content.Headers.ContentType?.MediaType.Should().Be(ProblemMediaType, because);
            using var json = JsonDocument.Parse(content);
            json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue(because);
            errors.TryGetProperty(invalid.Field, out var messages).Should().BeTrue(because);
            messages.GetArrayLength().Should().BeGreaterThan(0, because);
        }
    }

    private static Dictionary<string, string> AccountForm() => new()
    {
        ["Name"] = "Valid account", ["Type"] = "Bank", ["Currency"] = "VND", ["InitialBalance"] = "100"
    };

    private static Dictionary<string, string> TransactionForm(Guid accountId, DateTime date) => new()
    {
        ["AccountId"] = accountId.ToString(),
        ["TransactionDate"] = date.ToString("O", CultureInfo.InvariantCulture),
        ["RevenueAmount"] = "0", ["SpentAmount"] = "10", ["Balance"] = "0",
        ["CategoryType"] = "Expense", ["Description"] = "Valid transaction"
    };

    private static Dictionary<string, string> TemplateForm(Guid accountId, DateTime date, string name,
        string daysInAdvance) => new()
    {
        ["AccountId"] = accountId.ToString(), ["Name"] = name, ["Amount"] = "100",
        ["TransactionType"] = "Expense", ["Frequency"] = "Monthly",
        ["StartDate"] = date.ToString("O", CultureInfo.InvariantCulture),
        ["NextExecutionDate"] = date.ToString("O", CultureInfo.InvariantCulture),
        ["IsActive"] = "true", ["AutoGenerate"] = "false", ["DaysInAdvance"] = daysInAdvance
    };

    private static async Task<Guid> CreateTemplateAsync(BackendFinanceClient api, Thm3PostgresFixture fixture, string name)
    {
        var accountId = await api.CreateAccountAsync("Template account");
        using var response = await api.PostFormAsync(TemplateRoute,
            TemplateForm(accountId, fixture.Today, name, daysInAdvance: "7"));
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await BackendFinanceClient.IdOf(response);
    }

    private static Task<int> CountAccountsAsync(Thm3PostgresFixture fixture) =>
        fixture.ReadAsync(db => db.Accounts.IgnoreQueryFilters().AsNoTracking().CountAsync());

    private static Task<int> CountTransactionsAsync(Thm3PostgresFixture fixture) =>
        fixture.ReadAsync(db => db.Transactions.IgnoreQueryFilters().AsNoTracking().CountAsync());

    private static Task<int> CountTemplatesAsync(Thm3PostgresFixture fixture) =>
        fixture.ReadAsync(db => db.RecurringTransactionTemplates.IgnoreQueryFilters().AsNoTracking().CountAsync());

    private static Task<AccountSnapshot> ReadAccountAsync(Thm3PostgresFixture fixture, Guid id) =>
        fixture.ReadAsync(db => db.Accounts.IgnoreQueryFilters().AsNoTracking().Where(account => account.Id == id)
            .Select(account => new AccountSnapshot(account.Name, account.Type, account.Currency, account.Code,
                account.InitialBalance, account.UpdatedAt))
            .SingleAsync());

    private static Task<TemplateSnapshot> ReadTemplateAsync(Thm3PostgresFixture fixture, Guid id) =>
        fixture.ReadAsync(db => db.RecurringTransactionTemplates.IgnoreQueryFilters().AsNoTracking()
            .Where(template => template.Id == id)
            .Select(template => new TemplateSnapshot(template.Name, template.Amount, template.Frequency,
                template.DaysInAdvance, template.StartDate, template.NextExecutionDate, template.IsActive,
                template.UpdatedAt))
            .SingleAsync());

    private static Task<Guid?> ReadTemplateAccountIdAsync(Thm3PostgresFixture fixture, Guid id) =>
        fixture.ReadAsync(db => db.RecurringTransactionTemplates.IgnoreQueryFilters().AsNoTracking()
            .Where(template => template.Id == id).Select(template => template.AccountId).SingleAsync());

    private sealed record AccountSnapshot(string? Name, AccountType Type, string? Currency, string? Code,
        decimal InitialBalance, DateTime? UpdatedAt);

    private sealed record TemplateSnapshot(string? Name, decimal Amount, RecurrenceFrequency Frequency,
        int DaysInAdvance, DateTime StartDate, DateTime NextExecutionDate, bool IsActive, DateTime? UpdatedAt);
}
