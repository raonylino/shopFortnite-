using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopFortnite.Application.DTOs;
using ShopFortnite.Services;

namespace ShopFortnite.Pages;

public class UsersModel : PageModel
{
    private readonly IApiClientService _apiClient;

    public UsersModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public List<UserWithCosmeticsDto> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            Users = await _apiClient.GetAllUsersAsync();
        }
        catch (Exception)
        {
            Users = new List<UserWithCosmeticsDto>();
        }
    }
}
