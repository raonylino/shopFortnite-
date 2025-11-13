using Xunit;
using Moq;
using AutoMapper;
using ShopFortnite.Application.UseCases;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Domain.Interfaces;
using ShopFortnite.Domain.Entities;

namespace ShopFortnite.Tests.UseCases;

public class PurchaseServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PurchaseService _purchaseService;

    public PurchaseServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _purchaseService = new PurchaseService(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task PurchaseCosmeticAsync_WithSufficientFunds_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Vbucks = 5000
        };

        var cosmetic = new Cosmetic
        {
            Id = cosmeticId,
            Name = "Cool Skin",
            Price = 1000,
            IsForSale = true
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mockCosmeticRepo = new Mock<ICosmeticRepository>();
        mockCosmeticRepo.Setup(r => r.GetByIdAsync(cosmeticId)).ReturnsAsync(cosmetic);

        var mockUserCosmeticRepo = new Mock<IUserCosmeticRepository>();
        mockUserCosmeticRepo.Setup(r => r.GetAsync(userId, cosmeticId)).ReturnsAsync((UserCosmetic?)null);
        mockUserCosmeticRepo.Setup(r => r.CreateAsync(It.IsAny<UserCosmetic>())).ReturnsAsync((UserCosmetic uc) => uc);

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.CreateAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => t);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.Cosmetics).Returns(mockCosmeticRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserCosmetics).Returns(mockUserCosmeticRepo.Object);
        _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _purchaseService.PurchaseCosmeticAsync(userId, cosmeticId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(4000, result.RemainingVbucks);
    }

    [Fact]
    public async Task PurchaseCosmeticAsync_WithInsufficientFunds_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Vbucks = 500
        };

        var cosmetic = new Cosmetic
        {
            Id = cosmeticId,
            Name = "Expensive Skin",
            Price = 1000,
            IsForSale = true
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var mockCosmeticRepo = new Mock<ICosmeticRepository>();
        mockCosmeticRepo.Setup(r => r.GetByIdAsync(cosmeticId)).ReturnsAsync(cosmetic);

        var mockUserCosmeticRepo = new Mock<IUserCosmeticRepository>();
        mockUserCosmeticRepo.Setup(r => r.GetAsync(userId, cosmeticId)).ReturnsAsync((UserCosmetic?)null);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.Cosmetics).Returns(mockCosmeticRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserCosmetics).Returns(mockUserCosmeticRepo.Object);

        // Act
        var result = await _purchaseService.PurchaseCosmeticAsync(userId, cosmeticId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("insuficientes", result.Message);
    }

    [Fact]
    public async Task ReturnCosmeticAsync_WithValidPurchase_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cosmeticId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Vbucks = 3000
        };

        var userCosmetic = new UserCosmetic
        {
            UserId = userId,
            CosmeticId = cosmeticId,
            PriceAtPurchase = 1000,
            PurchaseDate = DateTime.UtcNow,
            ReturnedDate = null
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mockUserCosmeticRepo = new Mock<IUserCosmeticRepository>();
        mockUserCosmeticRepo.Setup(r => r.GetAsync(userId, cosmeticId)).ReturnsAsync(userCosmetic);
        mockUserCosmeticRepo.Setup(r => r.UpdateAsync(It.IsAny<UserCosmetic>())).Returns(Task.CompletedTask);

        var mockTransactionRepo = new Mock<ITransactionRepository>();
        mockTransactionRepo.Setup(r => r.CreateAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => t);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.UserCosmetics).Returns(mockUserCosmeticRepo.Object);
        _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _purchaseService.ReturnCosmeticAsync(userId, cosmeticId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(4000, result.RemainingVbucks);
    }
}
