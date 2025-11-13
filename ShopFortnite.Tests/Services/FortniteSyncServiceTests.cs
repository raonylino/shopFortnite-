using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ShopFortnite.Infrastructure.ExternalServices;
using ShopFortnite.Domain.Interfaces;
using System.Net;
using System.Net.Http;
using Moq.Protected;

namespace ShopFortnite.Tests.Services;

public class FortniteSyncServiceTests
{
    [Fact]
    public void FortniteSyncService_ShouldBeCreated()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLogger = new Mock<ILogger<FortniteSyncService>>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://fortnite-api.com/")
        };

        mockHttpClientFactory.Setup(f => f.CreateClient("FortniteApi")).Returns(httpClient);

        services.AddScoped<IUnitOfWork>(sp => new Mock<IUnitOfWork>().Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var service = new FortniteSyncService(
            serviceProvider,
            mockLogger.Object,
            mockHttpClientFactory.Object
        );

        // Assert
        Assert.NotNull(service);
    }
}
