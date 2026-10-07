using Microsoft.Extensions.DependencyInjection;
using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    public DashboardPage(
        AuthService authService,
        ApiService apiService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _authService = authService;
        _apiService = apiService;
        _serviceProvider = serviceProvider;

        WelcomeLabel.Text =
            $"Bem-vindo, {_authService.UserName}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var dashboard =
                await _apiService.GetDashboardAsync();

            if (dashboard == null)
                return;

            ProductsLabel.Text =
                dashboard.TotalProducts.ToString();

            CategoriesLabel.Text =
                dashboard.TotalCategories.ToString();

            StockLabel.Text =
                dashboard.TotalStock.ToString();

            LowStockLabel.Text =
                dashboard.LowStockProducts.ToString();
        }
        catch (UnauthorizedAccessException)
        {
            await HandleExpiredSessionAsync();
        }
        catch (HttpRequestException)
        {
            await DisplayAlertAsync(
                "TechStore",
                "Não foi possível conectar à API.",
                "OK");
        }
        catch
        {
            await DisplayAlertAsync(
                "TechStore",
                "Não foi possível carregar o Dashboard.",
                "OK");
        }
    }

    private async Task HandleExpiredSessionAsync()
    {
        _authService.Logout();

        await DisplayAlertAsync(
            "Sessão expirada",
            "Sua sessão expirou. Faça login novamente.",
            "OK");

        var loginPage =
            _serviceProvider
                .GetRequiredService<LoginPage>();

        var navigation =
            new NavigationPage(loginPage);

        if (Window != null)
        {
            Window.Page = navigation;
        }
    }

    private async void OnProductsClicked(
        object? sender,
        EventArgs e)
    {
        var page =
            _serviceProvider
                .GetRequiredService<ProductsPage>();

        await Navigation.PushAsync(page);
    }

    private async void OnLowStockClicked(
        object? sender,
        EventArgs e)
    {
        var page =
            _serviceProvider
                .GetRequiredService<LowStockPage>();

        await Navigation.PushAsync(page);
    }

    private async void OnEntryClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new StockMovementPage(
                _apiService,
                "Entrada"));
    }

    private async void OnExitClicked(
        object? sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new StockMovementPage(
                _apiService,
                "Saida"));
    }

    private async void OnMovementsClicked(
        object? sender,
        EventArgs e)
    {
        var page =
            _serviceProvider
                .GetRequiredService<MovementsPage>();

        await Navigation.PushAsync(page);
    }

    private async void OnLogoutClicked(
        object? sender,
        EventArgs e)
    {
        _authService.Logout();

        var loginPage =
            _serviceProvider
                .GetRequiredService<LoginPage>();

        if (Window != null)
        {
            Window.Page =
                new NavigationPage(loginPage);
        }

        await Task.CompletedTask;
    }
}