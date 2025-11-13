using Microsoft.AspNetCore.Mvc;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Application.UseCases;

namespace ShopFortnite.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly ICosmeticService _cosmeticService;
    private readonly ILogger<ShopController> _logger;

    public ShopController(ICosmeticService cosmeticService, ILogger<ShopController> logger)
    {
        _cosmeticService = cosmeticService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CosmeticDto>>> GetShopItems([FromQuery] CosmeticQueryParameters parameters)
    {
        _logger.LogInformation($"[GET /api/shop] Page={parameters.Page}, PageSize={parameters.PageSize}");

        // Força o filtro IsForSale=true
        parameters.IsForSale = true;

        var result = await _cosmeticService.GetCosmeticsAsync(parameters);
        _logger.LogInformation($"[GET /api/shop] Retornando Page={result.Page}, TotalCount={result.TotalCount}");
        return Ok(result);
    }
}
