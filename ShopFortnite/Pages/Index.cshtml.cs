using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopFortnite.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserEmail { get; set; }
    public string?  UserName { get; set; }
    public decimal Vbucks { get; set; }

    public void OnGet()
    {
        IsAuthenticated = Request.Cookies.ContainsKey("AuthToken");

        if (IsAuthenticated)
        {
            UserEmail = Request.Cookies["UserEmail"];
            UserName = Request.Cookies["UserName"];
            if (decimal.TryParse(Request.Cookies["Vbucks"], out var vbucks))
            {
                Vbucks = vbucks;
            }
        }
    }
}
