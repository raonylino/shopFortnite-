using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Application.UseCases;
using System.Security.Claims;

namespace ShopFortnite.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CosmeticsController : ControllerBase
{
    private readonly ICosmeticService _cosmeticService;
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<CosmeticsController> _logger;

    public CosmeticsController(
        ICosmeticService cosmeticService,
        IPurchaseService purchaseService,
        ILogger<CosmeticsController> logger)
    {
        _cosmeticService = cosmeticService;
        _purchaseService = purchaseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CosmeticDto>>> GetCosmetics([FromQuery] CosmeticQueryParameters parameters)
    {
        _logger.LogInformation($"[GET /api/cosmetics] Page={parameters.Page}, PageSize={parameters.PageSize}");
        var result = await _cosmeticService.GetCosmeticsAsync(parameters);
        _logger.LogInformation($"[GET /api/cosmetics] Retornando Page={result.Page}, TotalCount={result.TotalCount}");
        return Ok(result);
    }

    [HttpGet("new")]
    public async Task<ActionResult<PagedResult<CosmeticDto>>> GetNewCosmetics([FromQuery] CosmeticQueryParameters parameters)
    {
        _logger.LogInformation($"[GET /api/cosmetics/new] Page={parameters.Page}, PageSize={parameters.PageSize}");

        // Força o filtro IsNew=true
        parameters.IsNew = true;

        var result = await _cosmeticService.GetCosmeticsAsync(parameters);
        _logger.LogInformation($"[GET /api/cosmetics/new] Retornando Page={result.Page}, TotalCount={result.TotalCount}");
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CosmeticDto>> GetCosmetic(Guid id)
    {
        var result = await _cosmeticService.GetCosmeticByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Cosmético não encontrado" });
        }
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/purchase")]
    public async Task<ActionResult<PurchaseResponse>> PurchaseCosmetic(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _purchaseService.PurchaseCosmeticAsync(userId, id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/return")]
    public async Task<ActionResult<PurchaseResponse>> ReturnCosmetic(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _purchaseService.ReturnCosmeticAsync(userId, id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
