using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TechStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        // SE NÃO EXISTIR TOKEN, VAI PARA O LOGIN
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        var client =
            _httpClientFactory.CreateClient();

        // ENVIAR O TOKEN JWT PARA A API
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response = await client.GetAsync(
            "http://localhost:5221/api/dashboard");

        // TOKEN INVÁLIDO OU EXPIRADO
        if (response.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Remove("JWToken");

            return RedirectToAction(
                "Login",
                "Account");
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar o Dashboard.";

            return View();
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var dashboard =
            JsonSerializer.Deserialize<JsonElement>(
                json);

        ViewBag.Dashboard = dashboard;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}