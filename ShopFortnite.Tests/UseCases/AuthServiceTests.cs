using Xunit;
using Moq;
using ShopFortnite.Application.UseCases;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace ShopFortnite.Tests.UseCases;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup JWT configuration
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("ShopFortniteSecretKey2024!@#MinimumLength32Characters!!");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("ShopFortnite");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("ShopFortniteUsers");

        _authService = new AuthService(_mockUnitOfWork.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        mockUserRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(10000, result.Vbucks);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = passwordHash,
            Vbucks = 5000
        };

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = password
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Vbucks, result.Vbucks);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "wrongpassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }
}
