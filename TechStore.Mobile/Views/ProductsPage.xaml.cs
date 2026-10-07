using TechStore.Mobile.Models;
using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class ProductsPage : ContentPage
{
    private readonly ApiService _apiService;

    private List<Product> _products = [];

    public ProductsPage(ApiService apiService)
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
            _products =
                await _apiService.GetProductsAsync();

            ApplyFilter();
        }
        catch
        {
            await DisplayAlertAsync(
                "TechStore",
                "Não foi possível carregar os produtos.",
                "OK");
        }
    }

    private void OnSearchTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search =
            SearchBar.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(search))
        {
            ProductsCollection.ItemsSource =
                _products;

            return;
        }

        ProductsCollection.ItemsSource =
            _products
                .Where(p =>
                    p.Name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    p.Code.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
    }

    private async void OnRefreshing(
        object? sender,
        EventArgs e)
    {
        await LoadProductsAsync();

        RefreshView.IsRefreshing = false;
    }
}