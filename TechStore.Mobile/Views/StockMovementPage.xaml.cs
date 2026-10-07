using TechStore.Mobile.Models;
using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class StockMovementPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly string _type;

    private List<Product> _products = [];

    public StockMovementPage(
        ApiService apiService,
        string type)
    {
        InitializeComponent();

        _apiService = apiService;
        _type = type;

        ConfigurePage();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadProductsAsync();
    }

    private void ConfigurePage()
    {
        if (_type == "Entrada")
        {
            Title = "Entrada";
            TitleLabel.Text = "Entrada de estoque";
            SubtitleLabel.Text =
                "Adicione unidades ao estoque do produto.";
            SaveButton.Text = "Registrar entrada";
        }
        else
        {
            Title = "Saída";
            TitleLabel.Text = "Saída de estoque";
            SubtitleLabel.Text =
                "Retire unidades do estoque do produto.";
            SaveButton.Text = "Registrar saída";
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            _products =
                await _apiService.GetProductsAsync();

            ProductPicker.ItemsSource =
                _products;
        }
        catch
        {
            await DisplayAlertAsync(
                "TechStore",
                "Não foi possível carregar os produtos.",
                "OK");
        }
    }

    private void OnProductSelected(
        object? sender,
        EventArgs e)
    {
        if (ProductPicker.SelectedItem
            is not Product product)
        {
            StockInfoBorder.IsVisible = false;
            return;
        }

        ProductCodeLabel.Text =
            $"Código: {product.Code}";

        CurrentStockLabel.Text =
            $"Estoque atual: {product.Quantity}";

        StockInfoBorder.IsVisible = true;
    }

    private async void OnSaveClicked(
        object? sender,
        EventArgs e)
    {
        if (ProductPicker.SelectedItem
            is not Product product)
        {
            await DisplayAlertAsync(
                "TechStore",
                "Selecione um produto.",
                "OK");

            return;
        }

        if (!int.TryParse(
                QuantityEntry.Text,
                out var quantity) ||
            quantity <= 0)
        {
            await DisplayAlertAsync(
                "TechStore",
                "Informe uma quantidade válida.",
                "OK");

            return;
        }

        try
        {
            SetLoading(true);

            var movement =
                new StockMovementRequest
                {
                    ProductId = product.Id,
                    Type = _type,
                    Quantity = quantity,
                    Observation =
                        ObservationEditor.Text?.Trim()
                        ?? string.Empty
                };

            var result =
                await _apiService
                    .CreateStockMovementAsync(
                        movement);

            if (!result.Success)
            {
                await DisplayAlertAsync(
                    "TechStore",
                    result.Message,
                    "OK");

                return;
            }

            await DisplayAlertAsync(
                "TechStore",
                $"{_type} registrada com sucesso.",
                "OK");

            await Navigation.PopAsync();
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
                "Ocorreu um erro ao registrar a movimentação.",
                "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool loading)
    {
        LoadingIndicator.IsVisible = loading;
        LoadingIndicator.IsRunning = loading;

        SaveButton.IsEnabled = !loading;
        ProductPicker.IsEnabled = !loading;
        QuantityEntry.IsEnabled = !loading;
    }
}