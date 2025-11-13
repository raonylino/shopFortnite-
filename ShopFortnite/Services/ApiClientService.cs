using Microsoft.Extensions.Logging;
using ShopFortnite.Application.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShopFortnite.Services;

public interface IApiClientService
{
    void SetAuthToken(string token);
    void ClearAuthToken();
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<PagedResult<CosmeticDto>?> GetCosmeticsAsync(CosmeticQueryParameters parameters);
    Task<PagedResult<CosmeticDto>?> GetNewCosmeticsAsync(CosmeticQueryParameters parameters);
    Task<PagedResult<CosmeticDto>?> GetShopCosmeticsAsync(CosmeticQueryParameters parameters);
    Task<CosmeticDto?> GetCosmeticByIdAsync(Guid id);
    Task<PurchaseResponse> PurchaseCosmeticAsync(Guid cosmeticId);
    Task<PurchaseResponse>  ReturnCosmeticAsync(Guid cosmeticId);
    Task<List<UserWithCosmeticsDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<List<UserCosmeticDto>> GetUserCosmeticsAsync(Guid userId);
    Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId);
}

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ApiClientService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClientService(IHttpClientFactory httpClientFactory, ILogger<ApiClientService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void EnsureAuthToken()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
        _logger.LogInformation($"Token do cookie: {(string.IsNullOrEmpty(token) ? "VAZIO/NULL" : "PRESENTE")}");

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation($"Authorization header definido: Bearer {token.Substring(0, Math.Min(20, token.Length))}...");
        }
        else
        {
            _logger.LogWarning("Nenhum token encontrado no cookie AuthToken");
        }
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await PostAsync<RegisterRequest, AuthResponse>("/api/auth/register", request);
        return response;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await PostAsync<LoginRequest, AuthResponse>("/api/auth/login", request);
        return response;
    }

    public async Task<PagedResult<CosmeticDto>?> GetCosmeticsAsync(CosmeticQueryParameters parameters)
    {
        return await GetCosmeticsFromEndpointAsync("/api/cosmetics", parameters);
    }

    public async Task<PagedResult<CosmeticDto>?> GetNewCosmeticsAsync(CosmeticQueryParameters parameters)
    {
        return await GetCosmeticsFromEndpointAsync("/api/cosmetics/new", parameters);
    }

    public async Task<PagedResult<CosmeticDto>?> GetShopCosmeticsAsync(CosmeticQueryParameters parameters)
    {
        return await GetCosmeticsFromEndpointAsync("/api/shop", parameters);
    }

    private async Task<PagedResult<CosmeticDto>?> GetCosmeticsFromEndpointAsync(string endpoint, CosmeticQueryParameters parameters)
    {
        _logger.LogInformation($"ApiClientService chamando endpoint: {endpoint}");
        _logger.LogInformation($"Parametros - Page={parameters.Page}, IsForSale={parameters.IsForSale}, IsNew={parameters.IsNew}");

        var queryString = BuildQueryString(parameters);
        var fullUrl = $"{endpoint}{queryString}";
        _logger.LogInformation($"URL completa: {fullUrl}");

        var response = await _httpClient.GetAsync(fullUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<CosmeticDto>>(content, _jsonOptions);
        _logger.LogInformation($"API retornou resultado com Page={result?.Page}, TotalCount={result?.TotalCount}");
        return result;
    }

    public async Task<CosmeticDto?> GetCosmeticByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/cosmetics/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CosmeticDto>(content, _jsonOptions);
    }

    public async Task<PurchaseResponse> PurchaseCosmeticAsync(Guid cosmeticId)
    {
        EnsureAuthToken();
        _logger.LogInformation($"Tentando comprar cosmético: {cosmeticId}");
        var response = await _httpClient.PostAsync($"/api/cosmetics/{cosmeticId}/purchase", null);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"Status da resposta: {response.StatusCode}");
        _logger.LogInformation($"Conteúdo da resposta: {content}");

        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Erro ao realizar compra. Status: {response.StatusCode}");
            }

            var error = JsonSerializer.Deserialize<PurchaseResponse>(content, _jsonOptions);
            throw new Exception(error?.Message ?? "Erro ao realizar compra");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new Exception("Resposta vazia da API");
        }

        return JsonSerializer.Deserialize<PurchaseResponse>(content, _jsonOptions)
            ?? throw new Exception("Erro ao processar resposta");
    }

    public async Task<PurchaseResponse> ReturnCosmeticAsync(Guid cosmeticId)
    {
        EnsureAuthToken();
        _logger.LogInformation($"Tentando devolver cosmético: {cosmeticId}");
        var response = await _httpClient.PostAsync($"/api/cosmetics/{cosmeticId}/return", null);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"Status da resposta: {response.StatusCode}");
        _logger.LogInformation($"Conteúdo da resposta: {content}");

        if (!response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception($"Erro ao realizar devolução. Status: {response.StatusCode}");
            }

            var error = JsonSerializer.Deserialize<PurchaseResponse>(content, _jsonOptions);
            throw new Exception(error?.Message ?? "Erro ao realizar devolução");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new Exception("Resposta vazia da API");
        }

        return JsonSerializer.Deserialize<PurchaseResponse>(content, _jsonOptions)
            ?? throw new Exception("Erro ao processar resposta");
    }    public async Task<List<UserWithCosmeticsDto>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("/api/users");

        if (!response.IsSuccessStatusCode)
            return new List<UserWithCosmeticsDto>();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<UserWithCosmeticsDto>>(content, _jsonOptions) ?? new List<UserWithCosmeticsDto>();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/users/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
    }

    public async Task<List<UserCosmeticDto>> GetUserCosmeticsAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/cosmetics");

        if (!response.IsSuccessStatusCode)
            return new List<UserCosmeticDto>();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<UserCosmeticDto>>(content, _jsonOptions) ?? new List<UserCosmeticDto>();
    }

    public async Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/transactions");

        if (!response.IsSuccessStatusCode)
            return new List<TransactionDto>();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<TransactionDto>>(content, _jsonOptions) ?? new List<TransactionDto>();
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
            return default;

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }

    private string BuildQueryString(CosmeticQueryParameters parameters)
    {
        var queryParams = new List<string>
        {
            $"page={parameters.Page}",
            $"pageSize={parameters.PageSize}"
        };

        if (!string.IsNullOrEmpty(parameters.Name))
            queryParams.Add($"name={Uri.EscapeDataString(parameters.Name)}");

        if (!string.IsNullOrEmpty(parameters.Type))
            queryParams.Add($"type={Uri.EscapeDataString(parameters.Type)}");

        if (!string.IsNullOrEmpty(parameters.Rarity))
            queryParams.Add($"rarity={Uri.EscapeDataString(parameters.Rarity)}");

        if (parameters.IsNew.HasValue)
            queryParams.Add($"isNew={parameters.IsNew.Value}");

        if (parameters.IsForSale.HasValue)
            queryParams.Add($"isForSale={parameters.IsForSale.Value}");

        if (parameters.FromDate.HasValue)
            queryParams.Add($"fromDate={parameters.FromDate.Value:yyyy-MM-dd}");

        return "?" + string.Join("&", queryParams);
    }
}
