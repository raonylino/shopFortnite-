using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Services;

namespace ShopFortnite.Pages.Cosmetics;

public class CosmeticDetailsModel : PageModel
{
    private readonly IApiClientService _apiClient;

    public CosmeticDetailsModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public CosmeticDto? Cosmetic { get; set; }
    public bool IsOwned { get; set; }
    public int UserVbucks { get; set; }
    public bool IsAuthenticated { get; set; }
    public int RefundAmount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // Obter detalhes do cosmético
        Cosmetic = await _apiClient.GetCosmeticByIdAsync(id);

        if (Cosmetic == null)
        {
            return NotFound();
        }

        // Calcular reembolso (80% do preço)
        RefundAmount = (int)(Cosmetic.Price * 0.8m);

        // Verificar autenticação
        var token = Request.Cookies["AuthToken"];
        IsAuthenticated = !string.IsNullOrEmpty(token);

        if (IsAuthenticated)
        {
            _apiClient.SetAuthToken(token!);

            // Obter saldo do usuário
            if (int.TryParse(Request.Cookies["Vbucks"], out var vbucks))
            {
                UserVbucks = vbucks;
            }

            // Verificar se o usuário possui este cosmético
            var userIdCookie = Request.Cookies["UserId"];
            if (Guid.TryParse(userIdCookie, out var userId))
            {
                var userCosmetics = await _apiClient.GetUserCosmeticsAsync(userId);
                IsOwned = userCosmetics.Any(uc => uc.CosmeticId == Cosmetic.Id);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostPurchaseAsync(Guid cosmeticId)
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = $"/Cosmetics/Details/{cosmeticId}" });
        }

        _apiClient.SetAuthToken(token);

        try
        {
            var result = await _apiClient.PurchaseCosmeticAsync(cosmeticId);

            // Atualizar saldo nos cookies
            Response.Cookies.Append("Vbucks", result.NewBalance.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

            SuccessMessage = $"Compra realizada com sucesso! Novo saldo: {result.NewBalance} V-Bucks";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return await OnGetAsync(cosmeticId);
    }

    public async Task<IActionResult> OnPostReturnAsync(Guid cosmeticId)
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = $"/Cosmetics/Details/{cosmeticId}" });
        }

        _apiClient.SetAuthToken(token);

        try
        {
            var result = await _apiClient.ReturnCosmeticAsync(cosmeticId);

            // Atualizar saldo nos cookies
            Response.Cookies.Append("Vbucks", result.NewBalance.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

            SuccessMessage = $"Devolução realizada com sucesso! Novo saldo: {result.NewBalance} V-Bucks";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return await OnGetAsync(cosmeticId);
    }
}
