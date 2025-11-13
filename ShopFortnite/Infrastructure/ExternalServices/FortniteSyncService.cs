using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShopFortnite.Domain.Entities;
using ShopFortnite.Domain.Interfaces;
using System.Text.Json;

namespace ShopFortnite.Infrastructure.ExternalServices;

public class FortniteSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FortniteSyncService> _logger;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://fortnite-api.com/v2";
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(6);

    public FortniteSyncService(
        IServiceProvider serviceProvider,
        ILogger<FortniteSyncService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("FortniteApi");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("FortniteSyncService iniciado");

            // Executa imediatamente na inicialização
            await SyncFortniteDataAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_syncInterval, stoppingToken);
                    await SyncFortniteDataAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Esperado quando o serviço é interrompido
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no loop de sincronização");
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Esperado quando o serviço é interrompido durante a sincronização inicial
            _logger.LogInformation("FortniteSyncService foi cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico no FortniteSyncService");
        }
    }

    private async Task SyncFortniteDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando sincronização com API Fortnite");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Sync all cosmetics
            await SyncAllCosmeticsAsync(unitOfWork, cancellationToken);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Todos os cosméticos sincronizados");

            // Sync new cosmetics
            await SyncNewCosmeticsAsync(unitOfWork, cancellationToken);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Novos cosméticos sincronizados");

            // Sync shop
            await SyncShopAsync(unitOfWork, cancellationToken);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Loja sincronizada");

            _logger.LogInformation("Sincronização concluída com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante sincronização");
        }
    }

    private async Task SyncAllCosmeticsAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/cosmetics/br", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<FortniteCosmeticsResponse>(content);

            if (apiResponse?.Data != null)
            {
                // Remove duplicatas pelo ExternalId
                var cosmetics = apiResponse.Data
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .Select(MapToCosmetic)
                    .ToList();

                await unitOfWork.Cosmetics.CreateOrUpdateManyAsync(cosmetics);
                _logger.LogInformation($"Sincronizados {cosmetics.Count} cosméticos");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar todos os cosméticos");
        }
    }

    private async Task SyncNewCosmeticsAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/cosmetics/br/new", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<FortniteCosmeticsResponse>(content);

            if (apiResponse?.Data != null)
            {
                // Mark all existing as not new
                var allCosmetics = await unitOfWork.Cosmetics.GetAllAsync();
                foreach (var cosmetic in allCosmetics)
                {
                    cosmetic.IsNew = false;
                }

                // Remove duplicatas e mark new ones
                var newCosmetics = apiResponse.Data
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .Select(data =>
                    {
                        var cosmetic = MapToCosmetic(data);
                        cosmetic.IsNew = true;
                        return cosmetic;
                    }).ToList();

                await unitOfWork.Cosmetics.CreateOrUpdateManyAsync(newCosmetics);
                _logger.LogInformation($"Marcados {newCosmetics.Count} cosméticos como novos");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar novos cosméticos");
        }
    }

    private async Task SyncShopAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando sincronização da loja do Fortnite");
            var response = await _httpClient.GetAsync($"{BaseUrl}/shop", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<FortniteShopResponse>(content);

            if (apiResponse?.Data?.Entries != null)
            {
                _logger.LogInformation($"API retornou {apiResponse.Data.Entries.Count} entries da loja");

                // Mark all as not for sale
                var allCosmetics = await unitOfWork.Cosmetics.GetAllAsync();
                foreach (var cosmetic in allCosmetics)
                {
                    cosmetic.IsForSale = false;
                }

                var shopCosmetics = new List<Cosmetic>();
                int itemCount = 0;

                // Process all entries (nova estrutura não tem featured/daily separados)
                foreach (var entry in apiResponse.Data.Entries)
                {
                    if (entry.BrItems != null)
                    {
                        foreach (var item in entry.BrItems)
                        {
                            var cosmetic = MapToCosmetic(item);
                            cosmetic.IsForSale = true;
                            cosmetic.Price = entry.FinalPrice;
                            shopCosmetics.Add(cosmetic);
                            itemCount++;
                        }
                    }
                }

                _logger.LogInformation($"Processados {itemCount} itens da loja");

                // Remove duplicatas pelo ExternalId antes de salvar
                var uniqueShopCosmetics = shopCosmetics
                    .GroupBy(c => c.ExternalId)
                    .Select(g => g.First())
                    .ToList();

                await unitOfWork.Cosmetics.CreateOrUpdateManyAsync(uniqueShopCosmetics);
                _logger.LogInformation($"Sincronizados {uniqueShopCosmetics.Count} itens únicos da loja");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar loja");
        }
    }

    private static Cosmetic MapToCosmetic(FortniteCosmeticData data)
    {
        return new Cosmetic
        {
            ExternalId = data.Id,
            Name = data.Name,
            Description = data.Description,
            Type = data.Type.DisplayValue ?? data.Type.Value,
            Rarity = data.Rarity.DisplayValue ?? data.Rarity.Value,
            ImageUrl = data.Images.Icon ?? data.Images.Featured ?? string.Empty,
            AddedDate = data.Added ?? DateTime.UtcNow
        };
    }
}
