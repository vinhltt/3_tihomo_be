using CoreFinance.Application.Services;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoreFinance.Application.Tests.AccountServiceTests;

/// <summary>
///     Contains test cases for the hard deletion methods of AccountService. (EN)<br />
///     Chứa các trường hợp kiểm thử cho các phương thức xóa cứng của AccountService. (VI)
/// </summary>
public partial class AccountServiceTests
{
    /// <summary>
    ///     Verifies that DeleteHardAsync returns the correct affected count when hard deletion is successful. (EN)<br />
    ///     Xác minh rằng DeleteHardAsync trả về số bản ghi bị ảnh hưởng chính xác khi xóa cứng thành công. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteHardAsync_ShouldReturnAffectedCount_WhenDeletionIsSuccessful()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var expectedAffectedCount = 1;

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.DeleteHardAsync(accountId))
            .ReturnsAsync(expectedAffectedCount);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteHardAsync(accountId);

        // Assert
        result.Should().Be(expectedAffectedCount);

        repoMock.Verify(r => r.DeleteHardAsync(accountId), Times.Once);
    }

    /// <summary>
    ///     Verifies that DeleteHardAsync returns zero when the entity to hard delete does not exist. (EN)<br />
    ///     Xác minh rằng DeleteHardAsync trả về không khi thực thể cần xóa cứng không tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteHardAsync_ShouldReturnZero_WhenEntityDoesNotExist()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var expectedAffectedCount = 0;

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.DeleteHardAsync(accountId))
            .ReturnsAsync(expectedAffectedCount);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(expectedAffectedCount);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        var result = await service.DeleteHardAsync(accountId);

        // Assert
        result.Should().Be(expectedAffectedCount);

        repoMock.Verify(r => r.DeleteHardAsync(accountId), Times.Once);
    }

    /// <summary>
    ///     Verifies that DeleteHardAsync throws an exception when the repository's hard delete operation throws an exception.
    ///     (EN)<br />
    ///     Xác minh rằng DeleteHardAsync ném ra một ngoại lệ khi thao tác xóa cứng của repository ném ra một ngoại lệ. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteHardAsync_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var expectedException = new InvalidOperationException("Database error");

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.DeleteHardAsync(accountId))
            .ThrowsAsync(expectedException);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.DeleteHardAsync(accountId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database error");

        repoMock.Verify(r => r.DeleteHardAsync(accountId), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Verifies that DeleteHardAsync throws an exception when the SaveChanges operation throws an exception after hard
    ///     deletion. (EN)<br />
    ///     Xác minh rằng DeleteHardAsync ném ra một ngoại lệ khi thao tác SaveChanges ném ra một ngoại lệ sau khi xóa cứng.
    ///     (VI)
    /// </summary>
    [Fact]
    public async Task DeleteHardAsync_ShouldThrowException_WhenSaveChangesThrowsException()
    {
        // Arrange
        var accountId = Guid.CreateVersion7();
        var expectedException = new InvalidOperationException("Save changes failed");

        var repoMock = new Mock<IBaseRepository<Account, Guid>>();
        repoMock.Setup(r => r.DeleteHardAsync(accountId))
            .ThrowsAsync(expectedException);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Account, Guid>()).Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<AccountService>>();
        var service = new AccountService(_mapper, unitOfWorkMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await service.DeleteHardAsync(accountId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Save changes failed");

        repoMock.Verify(r => r.DeleteHardAsync(accountId), Times.Once);
    }
}