using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Unit tests for UpdateAccountRequestValidator (EN)<br />
/// Kiểm thử đơn vị cho UpdateAccountRequestValidator (VI)
/// </summary>
public class UpdateAccountRequestValidatorTests
{
    private readonly UpdateAccountRequestValidator _validator = new();

    private static AccountUpdateRequest CreateValidRequest() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Account",
        Type = "Bank",
        Currency = "VND"
    };

    /// <summary>
    /// Frontend-shaped payload (Type is the enum name) passes (EN)<br />
    /// Payload dạng frontend (Type là tên enum) hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Frontend_Shaped_Payload()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Name is required and limited to 100 characters (EN)<br />
    /// Name bắt buộc và tối đa 100 ký tự (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_Should_Reject_Missing_Name(string? name)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = name;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// A non-blank Name is accepted (EN)<br />
    /// Name không rỗng được chấp nhận (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Non_Blank_Name()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = "x";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Name accepts exactly 100 characters and rejects 101 (EN)<br />
    /// Name chấp nhận đúng 100 ký tự và từ chối 101 (VI)
    /// </summary>
    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Validator_Should_Enforce_Name_Max_Length(int length, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = new string('n', length);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Name);
        else result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Currency is required and limited to 10 characters (EN)<br />
    /// Currency bắt buộc và tối đa 10 ký tự (VI)
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("VND", true)]
    [InlineData("0123456789", true)]
    [InlineData("01234567890", false)]
    public void Validator_Should_Require_Currency_Within_Max_Length(string? currency, bool valid)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        if (valid) result.ShouldNotHaveValidationErrorFor(x => x.Currency);
        else result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    /// <summary>
    /// Type accepts defined enum names in any casing (EN)<br />
    /// Type chấp nhận tên enum đã định nghĩa với mọi kiểu chữ (VI)
    /// </summary>
    [Theory]
    [InlineData("Bank")]
    [InlineData("bank")]
    [InlineData("WALLET")]
    [InlineData("Wallet")]
    [InlineData("CreditCard")]
    [InlineData("DebitCard")]
    [InlineData("Cash")]
    [InlineData(" Bank ")]
    public void Validator_Should_Accept_Defined_Type_Names(string type)
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
    /// Type rejects null, blank, numeric strings, comma-joined names and unknown names with exactly one error
    /// carrying the dedicated message (EN)<br />
    /// Type từ chối null, rỗng, chuỗi số, tên ghép dấu phẩy và tên lạ với đúng một lỗi mang message riêng (VI)
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("99")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("Foo")]
    [InlineData("Bank,Wallet")]
    public void Validator_Should_Reject_Missing_Or_Undefined_Type_With_Single_Error(string? type)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Type = type;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        var typeErrors = result.Errors.Where(error => error.PropertyName == nameof(AccountUpdateRequest.Type)).ToList();
        typeErrors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(CoreFinanceMessages.AccountTypeInvalid);
    }

    /// <summary>
    /// Code is optional: null passes (EN)<br />
    /// Code là tùy chọn: null hợp lệ (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Null_Code()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    /// <summary>
    /// A valid snake_case code passes (EN)<br />
    /// Code snake_case hợp lệ được chấp nhận (VI)
    /// </summary>
    [Theory]
    [InlineData("techcombank_debit")]
    [InlineData("momo_wallet")]
    [InlineData("account123")]
    public void Validator_Should_Accept_Snake_Case_Code(string code)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = code;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    /// <summary>
    /// An empty or whitespace code is rejected with the exact message (EN)<br />
    /// Code rỗng hoặc toàn khoảng trắng bị từ chối với đúng message (VI)
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_Should_Reject_Empty_Code_With_Exact_Message(string code)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = code;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeNotEmpty);
    }

    /// <summary>
    /// A code longer than 50 characters is rejected with the exact message (EN)<br />
    /// Code dài hơn 50 ký tự bị từ chối với đúng message (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Reject_Code_Over_50_Characters_With_Exact_Message()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = new string('a', 51);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeMaxLength);
    }

    /// <summary>
    /// A code with exactly 50 characters is accepted (EN)<br />
    /// Code đúng 50 ký tự được chấp nhận (VI)
    /// </summary>
    [Fact]
    public void Validator_Should_Accept_Code_With_50_Characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = new string('a', 50);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    /// <summary>
    /// A non snake_case code is rejected with the exact message (EN)<br />
    /// Code không phải snake_case bị từ chối với đúng message (VI)
    /// </summary>
    [Theory]
    [InlineData("Invalid-Code")]
    [InlineData("UPPER_CASE")]
    [InlineData("has space")]
    public void Validator_Should_Reject_Non_Snake_Case_Code_With_Exact_Message(string code)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Code = code;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage(CoreFinanceMessages.AccountCodeSnakeCase);
    }
}
