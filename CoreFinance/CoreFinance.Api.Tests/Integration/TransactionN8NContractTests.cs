using System.Net;
using System.Text;
using System.Text.Json;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
/// Contract tests for Transaction API POST endpoint for n8n integration (EN)<br/>
/// Tests hợp đồng cho Transaction API POST endpoint cho tích hợp n8n (VI)<br/>
/// Feature: tihomo-4 - N8N Google Sheets Integration
/// </summary>
public class TransactionN8NContractTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly string _testDatabaseName;
    private readonly Guid _testUserId;
    private string? _testApiKey;
    private Account? _testAccount;

    public TransactionN8NContractTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _testDatabaseName = $"TestCoreFinanceDb_TransactionN8N_{Guid.CreateVersion7():N}";
        _testUserId = Guid.CreateVersion7();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CoreFinanceDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add test database
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
    /// Initialize test database với account for transactions (EN)<br/>
    /// Khởi tạo test database với tài khoản cho transactions (VI)
    /// </summary>
    private async Task InitializeTestAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();

        await context.Database.EnsureCreatedAsync();
        await CreateTestAccountAsync(context);

        // Setup API Key authentication (simulating n8n workflow)
        _testApiKey = "tihomo_n8n_workflow_api_key";
        _client.DefaultRequestHeaders.Add("X-API-Key", _testApiKey);

        _output.WriteLine($"Test initialized với Account: {_testAccount!.Name}");
    }

    /// <summary>
    /// Create test account for transaction tests (EN)<br/>
    /// Tạo test account cho transaction tests (VI)
    /// </summary>
    private async Task CreateTestAccountAsync(CoreFinanceDbContext context)
    {
        _testAccount = new Account
        {
            Id = Guid.CreateVersion7(),
            UserId = _testUserId,
            Name = "Techcombank Debit Card",
            Code = "techcombank_debit",
            Type = AccountType.DebitCard,
            Currency = "VND",
            InitialBalance = 10000000,
            CurrentBalance = 10000000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Accounts.Add(_testAccount);
        await context.SaveChangesAsync();

        _output.WriteLine($"Created test account: {_testAccount.Name} (ID: {_testAccount.Id})");
    }

    /// <summary>
    /// Create JSON content for HTTP request (EN)<br/>
    /// Tạo JSON content cho HTTP request (VI)
    /// </summary>
    private static StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserialize HTTP response (EN)<br/>
    /// Deserialize HTTP response (VI)
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

    #region Contract Tests: POST Transaction with Valid Data

    [Fact]
    public async Task POST_Transaction_With_Valid_Income_Data_Should_Return_201()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.Parse("2025-10-10T14:30:00Z").ToUniversalTime(),
            RevenueAmount = 5000000.00m,
            SpentAmount = 0.00m,
            Description = "Salary payment October 2025",
            Balance = 15000000.00m,
            BalanceCompare = 15000000.00m,
            TransactionCode = "SAL202510",
            SyncMisa = false,
            Vn = true,
            CategorySummary = "",
            Note = "Monthly salary",
            ImportFrom = "n8n_google_sheets",
            CategoryType = CategoryType.Income,
            Group = "2025-10"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "API should return 201 Created for valid transaction");

        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {content}");

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result.Should().NotBeNull("Response body should not be null");
        result!.Id.Should().NotBeEmpty("Transaction ID should be returned");
        result.Success.Should().BeTrue("Success flag should be true");
        result.Message.Should().NotBeNullOrEmpty("Message should be present");

        _output.WriteLine($"✓ Created transaction ID: {result.Id}");
    }

    [Fact]
    public async Task POST_Transaction_With_Valid_Expense_Data_Should_Return_201()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.Parse("2025-10-11T09:15:00Z").ToUniversalTime(),
            RevenueAmount = 0.00m,
            SpentAmount = 350000.00m,
            Description = "Grab ride to office",
            Balance = 14650000.00m,
            TransactionCode = "GRAB20251011",
            SyncMisa = false,
            Vn = true,
            Note = "",
            ImportFrom = "n8n_google_sheets",
            CategoryType = CategoryType.Expense,
            Group = "2025-10"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        _output.WriteLine($"✓ Created expense transaction ID: {result.Id}");
    }

    #endregion

    #region Contract Tests: Validation Errors

    [Fact]
    public async Task POST_Transaction_Without_AccountId_Should_Return_400()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = Guid.Empty, // Invalid
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 1000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Missing AccountId should return 400 Bad Request");

        _output.WriteLine($"✓ Validation error returned for missing AccountId");
    }

    [Fact]
    public async Task POST_Transaction_Without_TransactionDate_Should_Return_400()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = default, // Invalid - required field
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 1000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _output.WriteLine($"✓ Validation error returned for missing TransactionDate");
    }

    [Fact]
    public async Task POST_Transaction_With_Invalid_AccountId_Should_Return_400()
    {
        // Arrange
        await InitializeTestAsync();

        var nonExistentAccountId = Guid.CreateVersion7();
        var request = new TransactionCreateRequest
        {
            AccountId = nonExistentAccountId,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 1000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Non-existent AccountId should return 400");

        _output.WriteLine($"✓ Validation error for non-existent Account");
    }

    [Fact]
    public async Task POST_Transaction_With_Both_Amounts_Zero_Should_Return_400()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 0, // Invalid - both zero
            SpentAmount = 0,   // Invalid - both zero
            Balance = 1000000,
            CategoryType = CategoryType.Other,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "At least one amount must be greater than 0");

        _output.WriteLine($"✓ Validation error for both amounts being zero");
    }

    [Fact]
    public async Task POST_Transaction_With_Negative_Amounts_Should_Return_400()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = -1000000, // Invalid - negative
            SpentAmount = 0,
            Balance = 1000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _output.WriteLine($"✓ Validation error for negative amounts");
    }

    #endregion

    #region Contract Tests: ImportFrom Field

    [Fact]
    public async Task POST_Transaction_Should_Store_ImportFrom_Correctly()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 11000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets" // Critical for n8n integration
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result!.Id.Should().NotBeEmpty();

        // Verify ImportFrom was stored
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        var transaction = await context.Transactions.FindAsync(result.Id);

        transaction.Should().NotBeNull();
        transaction!.ImportFrom.Should().Be("n8n_google_sheets",
            "ImportFrom field must be stored exactly as provided");

        _output.WriteLine($"✓ ImportFrom stored correctly: {transaction.ImportFrom}");
    }

    [Theory]
    [InlineData("n8n_google_sheets")]
    [InlineData("manual_import")]
    [InlineData("api_integration")]
    [InlineData(null)]
    public async Task POST_Transaction_Should_Accept_Various_ImportFrom_Values(string? importFrom)
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 11000000,
            CategoryType = CategoryType.Income,
            ImportFrom = importFrom
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"ImportFrom value '{importFrom}' should be accepted");

        _output.WriteLine($"✓ ImportFrom '{importFrom ?? "NULL"}' accepted");
    }

    #endregion

    #region Contract Tests: Authentication

    [Fact]
    public async Task POST_Transaction_Without_ApiKey_Should_Return_401()
    {
        // Arrange
        await InitializeTestAsync();
        _client.DefaultRequestHeaders.Remove("X-API-Key");

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 11000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden },
            "Missing API key should return 401 or 403");

        _output.WriteLine($"✓ Authentication required: {response.StatusCode}");
    }

    [Fact]
    public async Task POST_Transaction_With_Invalid_ApiKey_Should_Return_401()
    {
        // Arrange
        await InitializeTestAsync();
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        _client.DefaultRequestHeaders.Add("X-API-Key", "invalid_api_key_12345");

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 11000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });

        _output.WriteLine($"✓ Invalid API key rejected: {response.StatusCode}");
    }

    #endregion

    #region Contract Tests: Response Schema

    [Fact]
    public async Task POST_Transaction_Response_Schema_Should_Match_Contract()
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 2000000,
            SpentAmount = 0,
            Balance = 12000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result.Should().NotBeNull("Response should have body");

        // Verify schema fields
        result!.Id.Should().NotBeEmpty("Id field is required");
        result.Success.Should().BeTrue("Success field should be true");
        result.Message.Should().NotBeNullOrEmpty("Message field should be string");

        _output.WriteLine($"✓ Response schema validated:");
        _output.WriteLine($"  Id: {result.Id}");
        _output.WriteLine($"  Success: {result.Success}");
        _output.WriteLine($"  Message: {result.Message}");
    }

    #endregion

    #region Contract Tests: N8N Integration Scenarios

    [Fact]
    public async Task POST_Transaction_With_All_N8N_Fields_Should_Succeed()
    {
        // Arrange
        await InitializeTestAsync();

        // Simulating full n8n workflow payload
        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.Parse("2025-10-11T12:30:00Z").ToUniversalTime(),
            RevenueAmount = 0,
            SpentAmount = 85000,
            Description = "Lunch com tam",
            Balance = 14565000,
            BalanceCompare = 14565000,
            TransactionCode = "LUNCH20251011",
            SyncMisa = false,
            Vn = true,
            CategorySummary = "",
            Note = "",
            ImportFrom = "n8n_google_sheets",
            CategoryType = CategoryType.Expense,
            Group = "2025-10",
            No = 15.0 // Google Sheet row number
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result!.Success.Should().BeTrue();

        // Verify all fields stored
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        var transaction = await context.Transactions.FindAsync(result.Id);

        transaction.Should().NotBeNull();
        transaction!.Description.Should().Be(request.Description);
        transaction.TransactionCode.Should().Be(request.TransactionCode);
        transaction.Balance.Should().Be(request.Balance);
        transaction.SyncMisa.Should().Be(request.SyncMisa);
        transaction.Vn.Should().Be(request.Vn);
        transaction.Group.Should().Be(request.Group);
        transaction.ImportFrom.Should().Be("n8n_google_sheets");
        transaction.No.Should().Be(15.0, "No field MUST store Google Sheet row number");

        _output.WriteLine($"✓ All n8n fields stored correctly:");
        _output.WriteLine($"  No: {transaction.No} (Google Sheet row number)");
        _output.WriteLine($"  ImportFrom: {transaction.ImportFrom}");
        _output.WriteLine($"  Group: {transaction.Group}");
    }

    [Fact]
    public async Task POST_Transaction_CategoryType_Should_Match_N8N_Calculation()
    {
        // Arrange
        await InitializeTestAsync();

        // Test all CategoryType values
        var testCases = new[]
        {
            new { Revenue = 5000000m, Spent = 0m, Expected = CategoryType.Income },
            new { Revenue = 0m, Spent = 350000m, Expected = CategoryType.Expense },
            new { Revenue = 1000000m, Spent = 1000000m, Expected = CategoryType.Transfer },
            new { Revenue = 0m, Spent = 0m, Expected = CategoryType.Other }
        };

        foreach (var testCase in testCases)
        {
            var request = new TransactionCreateRequest
            {
                AccountId = _testAccount!.Id,
                TransactionDate = DateTime.UtcNow,
                RevenueAmount = testCase.Revenue,
                SpentAmount = testCase.Spent,
                Balance = 10000000,
                CategoryType = testCase.Expected,
                ImportFrom = "n8n_google_sheets"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Transaction", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                $"Transaction với Revenue={testCase.Revenue}, Spent={testCase.Spent} should succeed");

            _output.WriteLine($"✓ CategoryType {testCase.Expected} accepted for Revenue={testCase.Revenue}, Spent={testCase.Spent}");
        }
    }

    [Fact]
    public async Task POST_Transaction_With_Fractional_No_For_Row_Insertion()
    {
        // Arrange
        await InitializeTestAsync();

        // Test fractional indexing: Google Sheet row 15.5 (inserted between row 15 and 16)
        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 1000000,
            SpentAmount = 0,
            Balance = 11000000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets",
            No = 15.5 // Fractional row number for insertion
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Transaction với fractional No should be accepted");

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result!.Id.Should().NotBeEmpty();

        // Verify fractional No stored correctly
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        var transaction = await context.Transactions.FindAsync(result.Id);

        transaction.Should().NotBeNull();
        transaction!.No.Should().Be(15.5, "Fractional indexing must be supported for row insertion");

        _output.WriteLine($"✓ Fractional No verified:");
        _output.WriteLine($"  Transaction ID: {transaction.Id}");
        _output.WriteLine($"  No: {transaction.No} (supports insertion between rows)");
    }

    [Theory]
    [InlineData(1.0, "First row")]
    [InlineData(15.0, "Row 15")]
    [InlineData(15.5, "Row 15.5 - fractional")]
    [InlineData(15.25, "Row 15.25 - fractional")]
    [InlineData(100.0, "Row 100")]
    public async Task POST_Transaction_Should_Accept_Various_No_Values(double noValue, string description)
    {
        // Arrange
        await InitializeTestAsync();

        var request = new TransactionCreateRequest
        {
            AccountId = _testAccount!.Id,
            TransactionDate = DateTime.UtcNow,
            RevenueAmount = 500000,
            SpentAmount = 0,
            Balance = 10500000,
            CategoryType = CategoryType.Income,
            ImportFrom = "n8n_google_sheets",
            No = noValue
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"No={noValue} ({description}) should be accepted");

        var result = await DeserializeResponseAsync<TransactionCreateResponse>(response);
        result!.Id.Should().NotBeEmpty();

        _output.WriteLine($"✓ No={noValue} ({description}) accepted");
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

/// <summary>
/// Response DTO for Transaction creation (EN)<br/>
/// Response DTO cho tạo Transaction (VI)
/// </summary>
public class TransactionCreateResponse
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
