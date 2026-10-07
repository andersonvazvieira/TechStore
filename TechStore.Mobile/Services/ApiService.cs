using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechStore.Mobile.Models;

namespace TechStore.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ApiService(AuthService authService)
    {
        _authService = authService;

        _httpClient = new HttpClient
        {
            BaseAddress =
                new Uri("http://localhost:5221/")
        };
    }

    public async Task<LoginResponse?> LoginAsync(
        string email,
        string password)
    {
        var loginRequest =
            new LoginRequest
            {
                Email = email.Trim(),
                Password = password
            };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                loginRequest);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<DashboardResponse?>
        GetDashboardAsync()
    {
        SetAuthorizationHeader();

        var response =
            await _httpClient.GetAsync(
                "api/dashboard");

        await CheckAuthenticationAsync(
            response);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<DashboardResponse>();
    }

    public async Task<List<Product>>
        GetProductsAsync()
    {
        SetAuthorizationHeader();

        var response =
            await _httpClient.GetAsync(
                "api/products");

        await CheckAuthenticationAsync(
            response);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<Product>>()
            ?? [];
    }

    public async Task<List<Product>>
        GetLowStockProductsAsync()
    {
        SetAuthorizationHeader();

        var response =
            await _httpClient.GetAsync(
                "api/products/low-stock");

        await CheckAuthenticationAsync(
            response);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<Product>>()
            ?? [];
    }

    public async Task<List<StockMovement>>
        GetStockMovementsAsync()
    {
        SetAuthorizationHeader();

        var response =
            await _httpClient.GetAsync(
                "api/stockmovements");

        await CheckAuthenticationAsync(
            response);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<StockMovement>>()
            ?? [];
    }

    public async Task<(bool Success, string Message)>
        CreateStockMovementAsync(
            StockMovementRequest movement)
    {
        SetAuthorizationHeader();

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/stockmovements",
                movement);

        await CheckAuthenticationAsync(
            response);

        if (response.IsSuccessStatusCode)
        {
            return (
                true,
                "Movimentação realizada com sucesso.");
        }

        var message =
            await response.Content
                .ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(
            message))
        {
            message =
                "Não foi possível realizar a movimentação.";
        }

        message =
            message.Trim().Trim('"');

        return (false, message);
    }

    private void SetAuthorizationHeader()
    {
        _httpClient
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _authService.Token);
    }

    private Task CheckAuthenticationAsync(
        HttpResponseMessage response)
    {
        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            _authService.Logout();

            throw new UnauthorizedAccessException(
                "Sua sessão expirou. Faça login novamente.");
        }

        return Task.CompletedTask;
    }
}