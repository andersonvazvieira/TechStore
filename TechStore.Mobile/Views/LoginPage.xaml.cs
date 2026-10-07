using Microsoft.Extensions.DependencyInjection;
using TechStore.Mobile.Services;

namespace TechStore.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public LoginPage(
        ApiService apiService,
        AuthService authService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _apiService = apiService;
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    private async void OnLoginClicked(
        object? sender,
        EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var email =
            EmailEntry.Text?.Trim()
            ?? string.Empty;

        var password =
            PasswordEntry.Text
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ShowError(
                "Informe o e-mail e a senha.");

            return;
        }

        try
        {
            SetLoading(true);

            var result =
                await _apiService.LoginAsync(
                    email,
                    password);

            if (result == null ||
                string.IsNullOrWhiteSpace(
                    result.Token))
            {
                ShowError(
                    "E-mail ou senha inválidos.");

                return;
            }

            await _authService.SetLoginAsync(
                result.Token,
                result.User.Id,
                result.User.Name,
                result.User.Email,
                result.User.Role);

            var dashboard =
                _serviceProvider
                    .GetRequiredService<DashboardPage>();

            await Navigation.PushAsync(
                dashboard);

            PasswordEntry.Text =
                string.Empty;
        }
        catch (HttpRequestException)
        {
            ShowError(
                "Não foi possível conectar à API do TechStore.");
        }
        catch
        {
            ShowError(
                "Ocorreu um erro ao realizar o login.");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ShowError(
        string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetLoading(
        bool loading)
    {
        LoadingIndicator.IsRunning =
            loading;

        LoadingIndicator.IsVisible =
            loading;

        LoginButton.IsEnabled =
            !loading;
    }
}