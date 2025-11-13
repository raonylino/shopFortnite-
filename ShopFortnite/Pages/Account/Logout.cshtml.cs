using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopFortnite.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        // Remove todos os cookies de autenticação
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("UserName");
        Response.Cookies.Delete("UserEmail");
        Response.Cookies.Delete("UserId");
        Response.Cookies.Delete("Vbucks");

        return RedirectToPage("/Index");
    }
}
