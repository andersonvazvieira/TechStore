using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class MovementsPage : ContentPage
{
    private readonly ApiService _apiService;

    public MovementsPage(
        ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadMovementsAsync();
    }

    private async Task LoadMovementsAsync()
    {
        try
        {
            var movements =
                await _apiService
                    .GetStockMovementsAsync();

            MovementsCollection.ItemsSource =
                movements;
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
                "Não foi possível carregar as movimentações.",
                "OK");
        }
    }

    private async void OnRefreshing(
        object? sender,
        EventArgs e)
    {
        try
        {
            await LoadMovementsAsync();
        }
        finally
        {
            RefreshView.IsRefreshing = false;
        }
    }
}