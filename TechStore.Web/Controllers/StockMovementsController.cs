using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class StockMovementsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public StockMovementsController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // =====================================================
    // HISTÓRICO DE MOVIMENTAÇÕES
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            "http://localhost:5221/api/stockmovements");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar as movimentações.";

            return View(
                new List<StockMovementViewModel>());
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var movements =
            JsonSerializer.Deserialize<List<StockMovementViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<StockMovementViewModel>();

        return View(movements);
    }

    // =====================================================
    // ENTRADA - EXIBIR TELA
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Entry()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var autorizado =
            await CarregarProdutos(client);

        if (!autorizado)
        {
            return IrParaLogin();
        }

        return View();
    }

    // =====================================================
    // ENTRADA - REGISTRAR
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Entry(
        StockMovementViewModel movement)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        movement.Type = "Entrada";

        var json =
            JsonSerializer.Serialize(movement);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/stockmovements",
            content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagemApi =
                await response.Content.ReadAsStringAsync();

            ViewBag.Erro =
                string.IsNullOrWhiteSpace(mensagemApi)
                    ? "Não foi possível registrar a entrada."
                    : mensagemApi;

            var autorizado =
                await CarregarProdutos(client);

            if (!autorizado)
            {
                return IrParaLogin();
            }

            return View(movement);
        }

        TempData["Sucesso"] =
            "Entrada registrada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // SAÍDA - EXIBIR TELA
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Exit()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var autorizado =
            await CarregarProdutos(client);

        if (!autorizado)
        {
            return IrParaLogin();
        }

        return View();
    }

    // =====================================================
    // SAÍDA - REGISTRAR
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Exit(
        StockMovementViewModel movement)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        movement.Type = "Saida";

        var json =
            JsonSerializer.Serialize(movement);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/stockmovements",
            content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagemApi =
                await response.Content.ReadAsStringAsync();

            ViewBag.Erro =
                string.IsNullOrWhiteSpace(mensagemApi)
                    ? "Não foi possível registrar a saída."
                    : mensagemApi;

            var autorizado =
                await CarregarProdutos(client);

            if (!autorizado)
            {
                return IrParaLogin();
            }

            return View(movement);
        }

        TempData["Sucesso"] =
            "Saída registrada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // CARREGAR PRODUTOS
    // =====================================================

    private async Task<bool> CarregarProdutos(
        HttpClient client)
    {
        var response = await client.GetAsync(
            "http://localhost:5221/api/products");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }

        var products =
            new List<ProductViewModel>();

        if (response.IsSuccessStatusCode)
        {
            var json =
                await response.Content.ReadAsStringAsync();

            products =
                JsonSerializer.Deserialize<List<ProductViewModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? new List<ProductViewModel>();
        }

        ViewBag.Products = products;

        return true;
    }

    // =====================================================
    // CRIAR CLIENTE AUTENTICADO COM JWT
    // =====================================================

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

    // =====================================================
    // REDIRECIONAR PARA LOGIN
    // =====================================================

    private IActionResult IrParaLogin()
    {
        HttpContext.Session.Remove("JWToken");

        return RedirectToAction(
            "Login",
            "Account");
    }
}