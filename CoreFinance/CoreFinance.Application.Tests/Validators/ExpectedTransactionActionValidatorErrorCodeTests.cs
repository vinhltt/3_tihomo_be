using System.Reflection;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Validators;
using CoreFinance.Domain.Constants;
using FluentAssertions;
using FluentValidation.Results;

namespace CoreFinance.Application.Tests.Validators;

/// <summary>
/// Invariant: every error code produced by the confirm, cancel and adjust validators goes straight into the client
/// <c>{code,message}</c> body, so it must be a public <see cref="CoreFinanceErrorCodes" /> constant and the documented
/// code for that rule, never a FluentValidation default such as <c>NotEmptyValidator</c>. (EN)<br />
/// Bất biến: mọi mã lỗi do validator xác nhận, hủy và điều chỉnh tạo ra đi thẳng vào body <c>{code,message}</c> gửi client,
/// nên phải là hằng public của <see cref="CoreFinanceErrorCodes" /> và đúng mã quy định cho rule đó, không bao giờ là mã mặc định
/// của FluentValidation như <c>NotEmptyValidator</c>. (VI)
/// </summary>
public class ExpectedTransactionActionValidatorErrorCodeTests
{
    private static readonly HashSet<string> CatalogueCodes = typeof(CoreFinanceErrorCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToHashSet();

    private readonly AdjustTransactionRequestValidator _adjustValidator = new();
    private readonly CancelTransactionRequestValidator _cancelValidator = new();
    private readonly ConfirmTransactionRequestValidator _confirmValidator = new();

    /// <summary>
    /// Invalid reasons shared by cancel and adjust: null, empty, whitespace-only and one character over the limit (EN)<br />
    /// Các lý do không hợp lệ dùng chung cho hủy và điều chỉnh: null, rỗng, toàn khoảng trắng và dài hơn giới hạn một ký tự (VI)
    /// </summary>
    public static IEnumerable<object?[]> InvalidReasons =>
    [
        [null],
        [string.Empty],
        ["   "],
        [new string('r', CoreFinanceMessages.MaxReasonLength + 1)]
    ];

    /// <summary>
    /// The catalogue helper finds the codes the validators rely on, so an empty set cannot make the tests pass vacuously (EN)<br />
    /// Helper danh mục tìm thấy các mã mà validator dùng, nên tập rỗng không thể làm test pass một cách vô nghĩa (VI)
    /// </summary>
    [Fact]
    public void ErrorCodeCatalogue_Should_Contain_Lifecycle_Validation_Codes()
    {
        // Assert
        CatalogueCodes.Should().Contain([
            CoreFinanceErrorCodes.InvalidActualTransaction,
            CoreFinanceErrorCodes.InvalidReason,
            CoreFinanceErrorCodes.InvalidAmount
        ]);
        CatalogueCodes.Should().NotContain("NotEmptyValidator");
    }

    /// <summary>
    /// An empty actual transaction ID is rejected only with the invalid-actual-transaction catalogue code (EN)<br />
    /// ID giao dịch thực rỗng chỉ bị từ chối với mã invalid-actual-transaction trong danh mục (VI)
    /// </summary>
    [Fact]
    public void Confirm_Should_ReportCatalogueInvalidActualTransactionCode_When_ActualTransactionIdIsEmpty()
    {
        // Arrange
        var request = new ConfirmTransactionRequest { ActualTransactionId = Guid.Empty };

        // Act
        var result = _confirmValidator.Validate(request);

        // Assert
        AssertEveryErrorCode(result, CoreFinanceErrorCodes.InvalidActualTransaction);
    }

    /// <summary>
    /// A null, empty, whitespace-only or over-long reason is rejected only with the invalid-reason catalogue code (EN)<br />
    /// Lý do null, rỗng, toàn khoảng trắng hoặc quá dài chỉ bị từ chối với mã invalid-reason trong danh mục (VI)
    /// </summary>
    [Theory]
    [MemberData(nameof(InvalidReasons))]
    public void Cancel_Should_ReportCatalogueInvalidReasonCode_When_ReasonIsInvalid(string? reason)
    {
        // Arrange
        var request = new CancelTransactionRequest { Reason = reason! };

        // Act
        var result = _cancelValidator.Validate(request);

        // Assert
        AssertEveryErrorCode(result, CoreFinanceErrorCodes.InvalidReason);
    }

    /// <summary>
    /// A zero or negative new amount is rejected only with the invalid-amount catalogue code (EN)<br />
    /// Số tiền mới bằng 0 hoặc âm chỉ bị từ chối với mã invalid-amount trong danh mục (VI)
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000.5)]
    public void Adjust_Should_ReportCatalogueInvalidAmountCode_When_NewAmountIsNotPositive(double newAmount)
    {
        // Arrange
        var request = new AdjustTransactionRequest { NewAmount = (decimal)newAmount, Reason = "valid reason" };

        // Act
        var result = _adjustValidator.Validate(request);

        // Assert
        AssertEveryErrorCode(result, CoreFinanceErrorCodes.InvalidAmount);
    }

    /// <summary>
    /// A null, empty, whitespace-only or over-long reason is rejected only with the invalid-reason catalogue code (EN)<br />
    /// Lý do null, rỗng, toàn khoảng trắng hoặc quá dài chỉ bị từ chối với mã invalid-reason trong danh mục (VI)
    /// </summary>
    [Theory]
    [MemberData(nameof(InvalidReasons))]
    public void Adjust_Should_ReportCatalogueInvalidReasonCode_When_ReasonIsInvalid(string? reason)
    {
        // Arrange
        var request = new AdjustTransactionRequest { NewAmount = 100m, Reason = reason! };

        // Act
        var result = _adjustValidator.Validate(request);

        // Assert
        AssertEveryErrorCode(result, CoreFinanceErrorCodes.InvalidReason);
    }

    private static void AssertEveryErrorCode(ValidationResult result, string expectedCode)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().OnlyContain(error => CatalogueCodes.Contains(error.ErrorCode));
        result.Errors.Should().OnlyContain(error => error.ErrorCode == expectedCode);
    }
}
