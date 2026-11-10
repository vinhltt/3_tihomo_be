using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for CreateAccountRequestValidator Code field validation (EN)
/// Kiểm thử đơn vị cho validation trường Code trong CreateAccountRequestValidator (VI)
/// Feature: tihomo-4 - N8N Google Sheets Integration
/// </summary>
public class CreateAccountRequestValidatorTests
{
    private readonly CreateAccountRequestValidator _validator;

    public CreateAccountRequestValidatorTests()
    {
        _validator = new CreateAccountRequestValidator();
    }

    /// <summary>
    /// Creates a valid test account create request with all required fields (EN)
    /// Tạo request tạo tài khoản kiểm thử hợp lệ với tất cả trường bắt buộc (VI)
    /// </summary>
    private AccountCreateRequest CreateValidRequest()
    {
        return new AccountCreateRequest
        {
            Name = "Test Account",
            Type = AccountType.DebitCard,
            Currency = "VND",
            InitialBalance = 0
        };
    }

    #region Code Format Validation Tests

    [Fact]
    public async Task Validator_Should_Accept_Valid_Snake_Case_Code()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = "techcombank_debit";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Valid snake_case code should pass validation");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            "Valid snake_case code should not have validation errors");
    }

    [Theory]
    [InlineData("techcombank_debit")]
    [InlineData("bidv_checking_001")]
    [InlineData("momo_wallet")]
    [InlineData("account123")]
    [InlineData("test_account_99")]
    public async Task Validator_Should_Accept_Various_Valid_Snake_Case_Codes(string validCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = validCode;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue($"Code '{validCode}' should be valid");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            $"Code '{validCode}' should not have validation errors");
    }

    [Fact]
    public async Task Validator_Should_Reject_Invalid_Code_Format()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = "Invalid-Code";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Invalid code format should fail validation");
        result.Errors.Should().Contain(e => e.PropertyName == "Code",
            "Invalid code format should have validation error");
    }

    [Theory]
    [InlineData("Techcombank_Debit", "Contains uppercase")]
    [InlineData("techcombank-debit", "Contains hyphen")]
    [InlineData("techcombank debit", "Contains space")]
    [InlineData("techcombank@debit", "Contains special char @")]
    [InlineData("techcombank.debit", "Contains dot")]
    [InlineData("", "Empty string")]
    public async Task Validator_Should_Reject_Invalid_Code_Patterns(string invalidCode, string reason)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = invalidCode;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse($"Code '{invalidCode}' should be invalid: {reason}");
        result.Errors.Should().Contain(e => e.PropertyName == "Code",
            $"Code '{invalidCode}' should have validation error: {reason}");
    }

    [Fact]
    public async Task Validator_Should_Accept_Null_Code()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = null;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Null code should be accepted (optional field)");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            "Null code should not have validation errors");
    }

    #endregion

    #region Code Length Validation Tests

    [Fact]
    public async Task Validator_Should_Reject_Code_Exceeding_50_Characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = new string('a', 51); // 51 characters

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Code exceeding 50 characters should be invalid");
        result.Errors.Should().Contain(e => e.PropertyName == "Code",
            "Code exceeding max length should have validation error");
        result.Errors.First(e => e.PropertyName == "Code")
            .ErrorMessage.Should().Contain("50", "Error message should mention max length");
    }

    [Fact]
    public async Task Validator_Should_Accept_Code_With_Exactly_50_Characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = new string('a', 50); // Exactly 50 characters

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Code with exactly 50 characters should be valid");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            "Code at max length should not have validation errors");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(49)]
    public async Task Validator_Should_Accept_Code_Within_Length_Limit(int length)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = new string('a', length);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue($"Code with {length} characters should be valid");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            $"Code with {length} characters should not have validation errors");
    }

    #endregion

    #region Code Uniqueness Validation Tests (Future Implementation)

    // NOTE: These tests will be implemented in T010 when adding uniqueness check
    // They are documented here as placeholders for TDD approach

    // [Fact]
    // public async Task Validator_Should_Check_Code_Uniqueness_Against_Database()
    // {
    //     // Test unique constraint validation
    //     // Will require IAccountRepository mock
    // }

    // [Fact]
    // public async Task Validator_Should_Accept_Code_That_Does_Not_Exist_In_Database()
    // {
    //     // Test that new unique code passes validation
    // }

    // [Fact]
    // public async Task Validator_Should_Reject_Code_That_Already_Exists_In_Database()
    // {
    //     // Test that duplicate code fails validation
    // }

    #endregion

    #region Integration with Other Validation Rules

    [Fact]
    public async Task Validator_Should_Pass_With_All_Valid_Fields_Including_Code()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = "valid_test_code";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Request with all valid fields including code should pass");
        result.Errors.Should().BeEmpty("No validation errors expected for valid request");
    }

    [Fact]
    public async Task Validator_Should_Pass_With_All_Valid_Fields_Without_Code()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = null;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue("Request with all valid fields and null code should pass");
        result.Errors.Should().BeEmpty("No validation errors expected when code is omitted");
    }

    [Fact]
    public async Task Code_Validation_Should_Not_Affect_Other_Field_Validations()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = "valid_code";
        request.Name = ""; // Invalid - required field

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Request should fail due to invalid Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Name",
            "Name validation should fail");
        result.Errors.Should().NotContain(e => e.PropertyName == "Code",
            "Code validation should pass independently");
    }

    [Fact]
    public async Task Multiple_Field_Validations_Should_Report_All_Errors()
    {
        // Arrange
        var request = new AccountCreateRequest
        {
            Name = "", // Invalid
            Code = "Invalid-Code", // Invalid
            Currency = "", // Invalid
            Type = (AccountType)99 // Invalid - not a defined enum value
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse("Multiple invalid fields should fail validation");
        result.Errors.Should().Contain(e => e.PropertyName == "Name", "Name should have error");
        result.Errors.Should().Contain(e => e.PropertyName == "Code", "Code should have error");
        result.Errors.Should().Contain(e => e.PropertyName == "Currency", "Currency should have error");
        result.Errors.Should().Contain(e => e.PropertyName == "Type", "Type should have error");
    }

    #endregion

    #region Field Rules

    /// <summary>
    /// A negative initial balance is a valid payload (EN)<br />
    /// Số dư ban đầu âm là payload hợp lệ (VI)
    /// </summary>
    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public void Validator_Should_Accept_Any_InitialBalance_Including_Negative(decimal initialBalance)
    {
        // Arrange
        var request = CreateValidRequest();
        request.InitialBalance = initialBalance;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InitialBalance);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Type Bank (enum value 0) is a valid account type (EN)<br />
    /// Type Bank (giá trị enum 0) là loại tài khoản hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Bank_Type()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Type = AccountType.Bank;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    /// <summary>
    /// Every defined account type is accepted (EN)<br />
    /// Mọi loại tài khoản đã định nghĩa đều được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData(AccountType.Bank)]
    [InlineData(AccountType.Wallet)]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.DebitCard)]
    [InlineData(AccountType.Cash)]
    public void Validator_Should_Accept_Every_Defined_Type(AccountType type)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Type = type;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    /// <summary>
    /// An undefined numeric account type is rejected (EN)<br />
    /// Loại tài khoản dạng số không được định nghĩa bị từ chối (VI)
    /// </summary>
    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    public void Validator_Should_Reject_Undefined_Type(int type)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Type = (AccountType)type;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    /// <summary>
    /// Name and Currency keep their length limits, and the payload shape sent by the frontend passes (EN)<br />
    /// Name và Currency giữ giới hạn độ dài, và payload dạng frontend hiện tại hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Enforce_Name_And_Currency_Length_Limits()
    {
        // Arrange
        var atLimit = CreateValidRequest();
        atLimit.Name = new string('n', 100);
        atLimit.Currency = new string('c', 10);
        var overLimit = CreateValidRequest();
        overLimit.Name = new string('n', 101);
        overLimit.Currency = new string('c', 11);

        // Act
        var okResult = _validator.TestValidate(atLimit);
        var badResult = _validator.TestValidate(overLimit);

        // Assert
        okResult.ShouldNotHaveValidationErrorFor(x => x.Name);
        okResult.ShouldNotHaveValidationErrorFor(x => x.Currency);
        badResult.ShouldHaveValidationErrorFor(x => x.Name);
        badResult.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    /// <summary>
    /// Frontend-shaped payload (Type Bank, negative InitialBalance) passes (EN)<br />
    /// Payload dạng frontend (Type Bank, InitialBalance âm) hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Frontend_Shaped_Payload()
    {
        // Arrange
        var request = new AccountCreateRequest
        {
            Name = "Techcombank",
            Type = AccountType.Bank,
            Currency = "VND",
            InitialBalance = -100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Custom account code messages keep their exact text (EN)<br />
    /// Message tùy biến của mã tài khoản giữ nguyên chữ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Report_Exact_Account_Code_Messages()
    {
        // Arrange
        var tooLong = CreateValidRequest();
        tooLong.Code = new string('a', 51);
        var badFormat = CreateValidRequest();
        badFormat.Code = "Invalid-Code";
        var empty = CreateValidRequest();
        empty.Code = "";

        // Act
        var tooLongResult = _validator.TestValidate(tooLong);
        var badFormatResult = _validator.TestValidate(badFormat);
        var emptyResult = _validator.TestValidate(empty);

        // Assert
        tooLongResult.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeMaxLength);
        badFormatResult.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeSnakeCase);
        emptyResult.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeNotEmpty);
    }

    #endregion

    #region Real-World Use Cases

    [Theory]
    [InlineData("techcombank_debit", "Techcombank Debit Card")]
    [InlineData("bidv_checking", "BIDV Checking Account")]
    [InlineData("momo_wallet", "MoMo E-Wallet")]
    [InlineData("vietcombank_credit", "Vietcombank Credit Card")]
    [InlineData("cash_vnd", "Cash VND")]
    public async Task Validator_Should_Support_Real_World_Account_Codes(string code, string accountName)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = code;
        request.Name = accountName;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue($"Real-world code '{code}' for '{accountName}' should be valid");
        result.Errors.Should().BeEmpty("No validation errors expected for real-world account");
    }

    #endregion
}
