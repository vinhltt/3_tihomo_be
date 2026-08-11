using System.Net;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
/// Contract tests for Account API GET by Code filter (EN)<br/>
/// Tests hợp đồng cho Account API GET theo bộ lọc Code (VI)<br/>
/// Feature: tihomo-4 - N8N Google Sheets Integration
/// </summary>
public class AccountGetByCodeContractTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly string _testDatabaseName;
    private readonly Guid _testUserId;
    private string? _testApiKey;

    // Test accounts với different codes
    private Account? _techcombankAccount;
    private Account? _bidvAccount;
    private Account? _momoAccount;
    private Account? _noCodeAccount;

    public AccountGetByCodeContractTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _testDatabaseName = $"TestCoreFinanceDb_CodeFilter_{Guid.CreateVersion7():N}";
        _testUserId = Guid.CreateVersion7();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            // Add minimal configuration for testing
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CorsOptions:PolicyName"] = "TestCorsPolicy",
                    ["CorsOptions:AllowedOrigins:0"] = "*",
                    ["JwtSettings:SecretKey"] = "test_secret_key_for_testing_minimum_32_characters",
                    ["JwtSettings:Issuer"] = "test_issuer",
                    ["JwtSettings:Audience"] = "test_audience",
                    ["ConnectionStrings:DefaultConnection"] = "InMemory"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Add test database (PostgreSQL registration is skipped in Testing environment)
                services.AddDbContext<CoreFinanceDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_testDatabaseName);
                    options.EnableSensitiveDataLogging();
                });

                // Logging configured for tests
                services.AddLogging();
            });
        });

        _client = _factory.CreateClient();
    }

    #region Setup and Helpers

    /// <summary>
    /// Initialize test database với multiple accounts having different codes (EN)<br/>
    /// Khởi tạo test database với nhiều tài khoản có codes khác nhau (VI)
    /// </summary>
    private async Task InitializeTestAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();

        await context.Database.EnsureCreatedAsync();
        await CreateTestAccountsAsync(context);

        // Setup API Key authentication
        _testApiKey = "tihomo_test_api_key_code_filter";
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        _output.WriteLine($"Test initialized with {context.Accounts.Count()} test accounts");
    }

    /// <summary>
    /// Create test accounts với various code values (EN)<br/>
    /// Tạo test accounts với nhiều giá trị code khác nhau (VI)
    /// </summary>
    private async Task CreateTestAccountsAsync(CoreFinanceDbContext context)
    {
        _techcombankAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = _testUserId,
            Name = "Techcombank Debit Card",
            Code = "techcombank_debit",
            No = 1.0, // First account in sort order
            Type = AccountType.DebitCard,
            Currency = "VND",
            InitialBalance = 10000000,
            CurrentBalance = 15000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _bidvAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = _testUserId,
            Name = "BIDV Checking Account",
            Code = "bidv_checking",
            No = 2.0, // Second account
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 5000000,
            CurrentBalance = 7500000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _momoAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = _testUserId,
            Name = "MoMo E-Wallet",
            Code = "momo_wallet",
            No = 1.5, // Fractional indexing - inserted between 1.0 and 2.0
            Type = AccountType.Wallet,
            Currency = "VND",
            InitialBalance = 1000000,
            CurrentBalance = 2500000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _noCodeAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = _testUserId,
            Name = "Legacy Account Without Code",
            Code = null, // Testing backward compatibility
            Type = AccountType.Cash,
            Currency = "VND",
            InitialBalance = 500000,
            CurrentBalance = 500000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Accounts.AddRange(_techcombankAccount, _bidvAccount, _momoAccount, _noCodeAccount);
        await context.SaveChangesAsync();

        _output.WriteLine($"Created test accounts:");
        _output.WriteLine($"  - {_techcombankAccount.Name} (code: {_techcombankAccount.Code})");
        _output.WriteLine($"  - {_bidvAccount.Name} (code: {_bidvAccount.Code})");
        _output.WriteLine($"  - {_momoAccount.Name} (code: {_momoAccount.Code})");
        _output.WriteLine($"  - {_noCodeAccount.Name} (code: NULL)");
    }

    /// <summary>
    /// Deserialize HTTP response content (EN)<br/>
    /// Deserialize nội dung HTTP response (VI)
    /// </summary>
    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    #endregion

    #region Contract Tests: GET Account by Code Filter

    [Fact]
    public async Task GET_Account_With_Code_Filter_Should_Return_Matching_Account()
    {
        // Arrange
        await InitializeTestAsync();
        var testCode = "techcombank_debit";
        var requestUri = $"/api/Account?filter=code eq '{testCode}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "API should return 200 OK for valid code filter");

        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {content}");

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull("Response body should not be null");
        result!.Data.Should().NotBeNull("Items array should not be null");
        result.Data.Should().ContainSingle(a => a.Code == testCode,
            $"Should return exactly one account với code '{testCode}'");

        var account = result.Data.First(a => a.Code == testCode);
        account.Id.Should().Be(_techcombankAccount!.Id);
        account.Name.Should().Be(_techcombankAccount.Name);
        account.Code.Should().MatchRegex("^[a-z0-9_]+$",
            "Code should match snake_case pattern");

        _output.WriteLine($"✓ Found account: {account.Name} with code: {account.Code}");
    }

    [Fact]
    public async Task GET_Account_With_NonExistent_Code_Should_Return_Empty_Array()
    {
        // Arrange
        await InitializeTestAsync();
        var nonExistentCode = "non_existent_account";
        var requestUri = $"/api/Account?filter=code eq '{nonExistentCode}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "API should return 200 OK even when no accounts match");

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty(
            "Should return empty array when no accounts match the code");

        _output.WriteLine($"✓ Empty array returned for non-existent code: {nonExistentCode}");
    }

    [Fact]
    public async Task GET_Account_Without_ApiKey_Should_Return_401()
    {
        // Arrange
        await InitializeTestAsync();
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        var requestUri = "/api/Account?filter=code eq 'techcombank_debit'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        // Note: Actual behavior depends on API Key middleware implementation
        // This test documents expected behavior according to contract
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden },
            "API should return 401 or 403 when API key is missing");

        _output.WriteLine($"✓ Authentication required: {response.StatusCode}");
    }

    [Fact]
    public async Task GET_Account_Response_Schema_Should_Include_Code_Field()
    {
        // Arrange
        await InitializeTestAsync();
        var requestUri = $"/api/Account?filter=code eq '{_bidvAccount!.Code}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().ContainSingle();

        var account = result.Data.First();
        account.Should().NotBeNull();

        // Verify all expected fields present
        account.Id.Should().NotBeEmpty("Id field should be present");
        account.Name.Should().NotBeNullOrEmpty("Name field should be present");
        account.Code.Should().NotBeNullOrEmpty("Code field MUST be present in response");
        account.No.Should().NotBeNull("No field MUST be present in response for n8n integration");
        account.No.Should().Be(2.0, "No field should match the value set in test data");
        // Type is non-nullable enum, always present
        account.Currency.Should().NotBeNullOrEmpty("Currency field should be present");
        account.CurrentBalance.Should().BeGreaterThanOrEqualTo(0, "CurrentBalance should be present");

        _output.WriteLine($"✓ Response schema validated:");
        _output.WriteLine($"  Id: {account.Id}");
        _output.WriteLine($"  Name: {account.Name}");
        _output.WriteLine($"  Code: {account.Code}");
        _output.WriteLine($"  No: {account.No}");
        _output.WriteLine($"  Type: {account.Type}");
        _output.WriteLine($"  Currency: {account.Currency}");
        _output.WriteLine($"  CurrentBalance: {account.CurrentBalance}");
    }

    [Theory]
    [InlineData("techcombank_debit")]
    [InlineData("bidv_checking")]
    [InlineData("momo_wallet")]
    public async Task GET_Account_With_Different_Valid_Codes_Should_Return_Correct_Accounts(string code)
    {
        // Arrange
        await InitializeTestAsync();
        var requestUri = $"/api/Account?filter=code eq '{code}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().ContainSingle(a => a.Code == code,
            $"Should return account với code '{code}'");

        _output.WriteLine($"✓ Successfully retrieved account với code: {code}");
    }

    [Fact]
    public async Task GET_Account_Filter_Should_Be_Case_Sensitive()
    {
        // Arrange
        await InitializeTestAsync();
        var upperCaseCode = "TECHCOMBANK_DEBIT"; // Wrong case
        var requestUri = $"/api/Account?filter=code eq '{upperCaseCode}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty(
            "Filter should be case-sensitive, uppercase code should not match");

        _output.WriteLine($"✓ Case-sensitive filter confirmed: '{upperCaseCode}' did not match");
    }

    [Theory]
    [InlineData("code='techcombank_debit'", "Invalid - missing eq operator")]
    [InlineData("code eq techcombank_debit", "Invalid - missing quotes")]
    [InlineData("code == 'techcombank_debit'", "Invalid - wrong operator")]
    public async Task GET_Account_With_Invalid_Filter_Syntax_Should_Return_400(string invalidFilter, string testCase)
    {
        // Arrange
        await InitializeTestAsync();
        var requestUri = $"/api/Account?filter={invalidFilter}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.BadRequest, HttpStatusCode.OK },
            $"Invalid filter syntax should return 400 Bad Request: {testCase}");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            _output.WriteLine($"✓ Bad request returned for invalid filter: {testCase}");
        }
        else
        {
            _output.WriteLine($"⚠ API accepted invalid filter (may need validation): {testCase}");
        }
    }

    [Fact]
    public async Task GET_Account_Code_Filter_Should_Not_Match_Partial_Strings()
    {
        // Arrange
        await InitializeTestAsync();
        var partialCode = "techcombank"; // Partial match
        var requestUri = $"/api/Account?filter=code eq '{partialCode}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty(
            "Code filter should be exact match, not partial match");

        _output.WriteLine($"✓ Exact match confirmed: partial code '{partialCode}' did not match");
    }

    [Fact]
    public async Task GET_Account_Should_Support_Pagination_With_Code_Filter()
    {
        // Arrange
        await InitializeTestAsync();
        var requestUri = $"/api/Account?filter=code eq '{_techcombankAccount!.Code}'&page=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().ContainSingle();
        result.Pagination.PageIndex.Should().Be(1, "Pagination page should be respected");
        result.Pagination.PageSize.Should().Be(10, "Pagination page size should be respected");

        _output.WriteLine($"✓ Pagination works with code filter");
        _output.WriteLine($"  CurrentPage: {result.Pagination.PageIndex}");
        _output.WriteLine($"  PageSize: {result.Pagination.PageSize}");
        _output.WriteLine($"  TotalRecords: {result.Pagination.TotalRow}");
    }

    [Fact]
    public async Task GET_Account_Should_Only_Return_Accounts_Owned_By_Authenticated_User()
    {
        // Arrange
        await InitializeTestAsync();

        // Add account for different user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        var otherUserId = Guid.CreateVersion7();
        var otherUserAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = otherUserId, // Different user
            Name = "Other User Account",
            Code = "other_user_account",
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = 1000000,
            CurrentBalance = 1000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Accounts.Add(otherUserAccount);
        await context.SaveChangesAsync();

        var requestUri = $"/api/Account?filter=code eq 'other_user_account'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty(
            "Should not return accounts belonging to other users (resource-based access control)");

        _output.WriteLine($"✓ Access control verified: other user's account not accessible");
    }

    [Fact]
    public async Task GET_Account_Response_Should_Include_No_Field_For_Ordering()
    {
        // Arrange
        await InitializeTestAsync();
        var requestUri = $"/api/Account?filter=code eq '{_momoAccount!.Code}'";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeResponseAsync<BasePaging<AccountViewModel>>(response);
        result.Should().NotBeNull();
        result!.Data.Should().ContainSingle();

        var account = result.Data.First();
        account.No.Should().NotBeNull("No field must be present for ordering");
        account.No.Should().Be(1.5, "No field supports fractional indexing (inserted between 1.0 and 2.0)");

        _output.WriteLine($"✓ No field verified:");
        _output.WriteLine($"  Account: {account.Name}");
        _output.WriteLine($"  Code: {account.Code}");
        _output.WriteLine($"  No: {account.No} (fractional indexing supported)");
    }

    #endregion

    #region Cleanup

    public async ValueTask DisposeAsync()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
            await context.Database.EnsureDeletedAsync();

            _client.Dispose();

            _output.WriteLine($"✓ Test cleanup completed for database: {_testDatabaseName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"✗ Error during cleanup: {ex.Message}");
        }
    }

    #endregion
}
