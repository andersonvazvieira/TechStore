using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class LowStockPage : ContentPage
{
    private readonly ApiService _apiService;

    public LowStockPage(
        ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products =
                await _apiService
                    .GetLowStockProductsAsync();

            ProductsCollection.ItemsSource =
                products;
        }
        catch
        {
            await DisplayAlertAsync(
                "TechStore",
                "Não foi possível carregar os produtos com estoque baixo.",
                "OK");
        }
    }

    private async void OnRefreshing(
        object? sender,
        EventArgs e)
    {
        await LoadProductsAsync();

        RefreshView.IsRefreshing = false;
    }
}