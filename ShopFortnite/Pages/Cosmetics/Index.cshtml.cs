using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Services;

namespace ShopFortnite.Pages.Cosmetics;

public class CosmeticsIndexModel : PageModel
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<CosmeticsIndexModel> _logger;

    public CosmeticsIndexModel(IApiClientService apiClient, ILogger<CosmeticsIndexModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public PagedResult<CosmeticDto>? Cosmetics { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Rarity { get; set; }
    public bool? IsForSale { get; set; }
    public bool IsNew { get; set; }

    public async Task OnGetAsync(
        [FromQuery] int page = 1,
        [FromQuery] string? name = null,
        [FromQuery] string? type = null,
        [FromQuery] string? rarity = null,
        [FromQuery] bool? isForSale = null,
        [FromQuery] bool isNew = false)
    {
        _logger.LogInformation($"========== OnGetAsync INICIO ==========");
        _logger.LogInformation($"URL QueryString: {Request.QueryString}");
        _logger.LogInformation($"Parametro page recebido: {page}");
        _logger.LogInformation($"=========================================");

        Name = name;
        Type = type;
        Rarity = rarity;
        IsForSale = isForSale;
        IsNew = isNew;

        var parameters = new CosmeticQueryParameters
        {
            Page = page,
            PageSize = 20,
            Name = name,
            Type = type,
            Rarity = rarity,
            IsForSale = isForSale,
            IsNew = isNew ? true : null
        };

        _logger.LogInformation($"Chamando API - IsNew={isNew}, IsForSale={isForSale}");

        // Escolhe o endpoint correto baseado nos filtros
        if (isNew)
        {
            _logger.LogInformation("Usando endpoint /api/cosmetics/new");
            Cosmetics = await _apiClient.GetNewCosmeticsAsync(parameters);
        }
        else if (isForSale == true)
        {
            _logger.LogInformation("Usando endpoint /api/shop");
            Cosmetics = await _apiClient.GetShopCosmeticsAsync(parameters);
        }
        else
        {
            _logger.LogInformation("Usando endpoint /api/cosmetics");
            Cosmetics = await _apiClient.GetCosmeticsAsync(parameters);
        }

        _logger.LogInformation($"API retornou {Cosmetics?.Items.Count() ?? 0} itens, página {Cosmetics?.Page}");
    }
}
