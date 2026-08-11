using Identity.Application.Common.Interfaces;
using Identity.Application.Services.Users;
using Identity.Domain.Dtos.Users;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Moq;

namespace Identity.Application.Tests.Integration;

/// <summary>
///     Integration tests to validate the complete Identity service functionality
/// </summary>
public class IdentityServiceIntegrationTests
{
    [Fact]
    public void UserService_ServiceInstantiation_ShouldSucceed()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockRoleRepository = new Mock<IRoleRepository>();

        // Act & Assert - This should not throw any exceptions
        var userService = new UserService(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockRoleRepository.Object);

        Assert.NotNull(userService);
    }

    [Fact]
    public async Task UserService_CreateUser_ShouldReturnUserResponse()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockRoleRepository = new Mock<IRoleRepository>();

        var createRequest = new CreateUserRequest(
            "test@example.com",
            "testuser",
            "Test User",
            "Test",
            "User",
            null,
            "TestPassword123!"
        );

        // Setup mocks
        mockUserRepository.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Email doesn't exist yet

        mockUserRepository.Setup(x => x.IsUsernameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        var userService = new UserService(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockRoleRepository.Object);

        // Act
        var result = await userService.CreateAsync(createRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createRequest.Email, result.Email);
        Assert.Equal(createRequest.Username, result.Username);
        Assert.Equal(createRequest.FullName, result.FullName);
        Assert.NotEqual(Guid.Empty, result.Id);

        // Verify repository calls
        mockUserRepository.Verify(x => x.IsEmailExistsAsync(createRequest.Email, It.IsAny<CancellationToken>()),
            Times.Once);
        mockUserRepository.Verify(x => x.IsUsernameExistsAsync(createRequest.Username, It.IsAny<CancellationToken>()),
            Times.Once);
        mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        mockPasswordHasher.Verify(x => x.HashPassword(createRequest.Password), Times.Once);
    }

    [Fact]
    public async Task UserService_CreateDuplicateUser_ShouldThrowException()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();

        var createRequest = new CreateUserRequest(
            "existing@example.com",
            "existinguser",
            "Existing User",
            "Existing",
            "User",
            null,
            "TestPassword123!"
        );

        // Setup mock to return that email already exists
        mockUserRepository.Setup(x => x.IsEmailExistsAsync(createRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var mockRoleRepository = new Mock<IRoleRepository>();

        var userService = new UserService(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockRoleRepository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => userService.CreateAsync(createRequest));

        Assert.Equal("Email is already taken", exception.Message);

        // Verify that AddAsync was never called since user already exists
        mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}