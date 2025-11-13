using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Services;
using System.ComponentModel.DataAnnotations;

namespace ShopFortnite.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IApiClientService _apiClient;

    public LoginModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Se já estiver autenticado, redireciona
        if (Request.Cookies.ContainsKey("AuthToken"))
        {
            Response.Redirect("/");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new LoginRequest
        {
            Email = Email,
            Password = Password
        };

        var response = await _apiClient.LoginAsync(request);

        if (response == null)
        {
            ErrorMessage = "Email ou senha inválidos";
            return Page();
        }

        // Salvar token e informações do usuário em cookies
        Response.Cookies.Append("AuthToken", response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Mudado para false porque estamos usando HTTP
            SameSite = SameSiteMode.Lax, // Mudado de Strict para Lax para melhor compatibilidade
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        Response.Cookies.Append("UserName", response.Nome, new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        Response.Cookies.Append("UserEmail", response.Email, new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        Response.Cookies.Append("UserId", response.UserId.ToString(), new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        Response.Cookies.Append("Vbucks", response.Vbucks.ToString(), new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });

        return RedirectToPage("/Index");
    }
}
