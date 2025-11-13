using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Services;

namespace ShopFortnite.Pages;

public class ProfileModel : PageModel
{
    private readonly IApiClientService _apiClient;

    public ProfileModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsAuthenticated { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int Vbucks { get; set; }
    public List<UserCosmeticDto> OwnedCosmetics { get; set; } = new();
    public List<TransactionDto> Transactions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var token = Request.Cookies["AuthToken"];
        IsAuthenticated = !string.IsNullOrEmpty(token);

        if (!IsAuthenticated)
        {
            return Page();
        }

        _apiClient.SetAuthToken(token!);

        // Obter dados do usuário dos cookies
        var userIdCookie = Request.Cookies["UserId"];
        var userEmailCookie = Request.Cookies["UserEmail"];
        var vbucksCookie = Request.Cookies["Vbucks"];

        if (Guid.TryParse(userIdCookie, out var userId))
        {
            UserId = userId;
        }

        UserEmail = userEmailCookie ?? "Usuário";

        if (int.TryParse(vbucksCookie, out var vbucks))
        {
            Vbucks = vbucks;
        }

        try
        {
            // Buscar cosméticos adquiridos
            OwnedCosmetics = await _apiClient.GetUserCosmeticsAsync(UserId);

            // Buscar histórico de transações
            Transactions = await _apiClient.GetUserTransactionsAsync(UserId);

            // Atualizar saldo real do usuário (pode ter mudado)
            var user = await _apiClient.GetUserByIdAsync(UserId);
            if (user != null && user.Vbucks != Vbucks)
            {
                Vbucks = (int)user.Vbucks;
                Response.Cookies.Append("Vbucks", Vbucks.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(1)
                });
            }
        }
        catch (Exception)
        {
            // Se houver erro ao buscar dados, mantém os valores dos cookies
        }

        return Page();
    }

    public async Task<IActionResult> OnPostReturnAsync(Guid cosmeticId)
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = "/Profile" });
        }

        _apiClient.SetAuthToken(token);

        try
        {
            var result = await _apiClient.ReturnCosmeticAsync(cosmeticId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;

                // Atualizar cookie de V-Bucks
                var vbucksCookie = Request.Cookies["Vbucks"];
                if (int.TryParse(vbucksCookie, out var currentVbucks))
                {
                    // O valor exato virá do banco, mas podemos atualizar o cookie
                    Response.Cookies.Delete("Vbucks");
                }
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
