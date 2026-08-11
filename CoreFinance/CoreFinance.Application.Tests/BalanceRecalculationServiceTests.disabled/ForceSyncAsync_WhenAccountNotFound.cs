using CoreFinance.Application.Services;
using FluentAssertions;
using Moq;

namespace CoreFinance.Application.Tests.BalanceRecalculationServiceTests;

/// <summary>
///     Tests for ForceSyncAsync when account is not found (EN)<br/>
///     Kiểm thử ForceSyncAsync khi không tìm thấy tài khoản (VI)
/// </summary>
public partial class BalanceRecalculationServiceTests
{
    /// <summary>
    ///     T010: Verifies that ForceSyncAsync throws InvalidOperationException when account not found (EN)<br/>
    ///     T010: Xác minh ForceSyncAsync ném InvalidOperationException khi không tìm thấy tài khoản (VI)
    /// </summary>
    [Fact]
    public async Task ForceSyncAsync_WhenAccountNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentAccountId = Guid.CreateVersion7();
        var (accountRepoMock, transactionRepoMock, loggerMock) = CreateMocks();

        // Setup mock to return null for non-existent account
        accountRepoMock.Setup(r => r.GetByIdAsync(nonExistentAccountId))
            .ReturnsAsync((Domain.Entities.Account?)null);

        var service = new BalanceRecalculationService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.ForceSyncAsync(nonExistentAccountId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Account with ID '{nonExistentAccountId}' not found");

        // Verify repository was called
        accountRepoMock.Verify(r => r.GetByIdAsync(nonExistentAccountId), Times.Once);

        // Verify transaction repository was NOT called
        transactionRepoMock.Verify(r => r.GetTransactionIdsAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
