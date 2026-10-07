using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class StockController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public StockController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // EXIBIR ESTOQUE
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            "http://localhost:5221/api/products");

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar o estoque.";

            return View(
                new List<ProductViewModel>());
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var products =
            JsonSerializer.Deserialize<List<ProductViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<ProductViewModel>();

        return View(products);
    }

    // CRIAR HTTPCLIENT COM JWT
    private HttpClient? CriarClienteAutenticado()
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var client =
            _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        return client;
    }

    // VOLTAR PARA LOGIN
    private IActionResult IrParaLogin()
    {
        HttpContext.Session.Remove("JWToken");

        return RedirectToAction(
            "Login",
            "Account");
    }
}