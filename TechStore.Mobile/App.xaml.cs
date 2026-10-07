using Microsoft.Extensions.DependencyInjection;
using TechStore.Mobile.Services;
using TechStore.Mobile.Views;

namespace TechStore.Mobile;

public partial class App : Application
{
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public App(
        AuthService authService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var loadingPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#F5F7FA"),
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 15,

                Children =
                {
                    new Label
                    {
                        Text = "TechStore",
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0D6EFD"),
                        HorizontalTextAlignment = TextAlignment.Center
                    },

                    new ActivityIndicator
                    {
                        IsRunning = true
                    },

                    new Label
                    {
                        Text = "Carregando...",
                        TextColor = Color.FromArgb("#6C757D")
                    }
                }
            }
        };

        var navigationPage =
            new NavigationPage(loadingPage);

        var window =
            new Window(navigationPage);

        _ = RestoreSessionAsync(
            navigationPage);

        return window;
    }

    private async Task RestoreSessionAsync(
        NavigationPage navigationPage)
    {
        var hasSession =
            await _authService.RestoreSessionAsync();

        Page initialPage;

        if (hasSession)
        {
            initialPage =
                _serviceProvider
                    .GetRequiredService<DashboardPage>();
        }
        else
        {
            initialPage =
                _serviceProvider
                    .GetRequiredService<LoginPage>();
        }

        await MainThread.InvokeOnMainThreadAsync(
            () =>
            {
                navigationPage.Navigation
                    .InsertPageBefore(
                        initialPage,
                        navigationPage.RootPage);

                return navigationPage.Navigation
                    .PopToRootAsync();
            });
    }
}