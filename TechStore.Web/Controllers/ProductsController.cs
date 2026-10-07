using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            "http://localhost:5221/api/products");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar os produtos.";

            return View(new List<ProductViewModel>());
        }

        var json = await response.Content.ReadAsStringAsync();

        var products =
            JsonSerializer.Deserialize<List<ProductViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<ProductViewModel>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products
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

        ViewBag.Search = search;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var autorizado = await CarregarCategorias(client);

        if (!autorizado)
        {
            return IrParaLogin();
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        ProductViewModel product)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var json = JsonSerializer.Serialize(product);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/products",
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
                    ? "Não foi possível cadastrar o produto."
                    : mensagemApi;

            var autorizado = await CarregarCategorias(client);

            if (!autorizado)
            {
                return IrParaLogin();
            }

            return View(product);
        }

        TempData["Sucesso"] =
            "Produto cadastrado com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            $"http://localhost:5221/api/products/{id}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var json = await response.Content.ReadAsStringAsync();

        var product =
            JsonSerializer.Deserialize<ProductViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (product == null)
        {
            return NotFound();
        }

        var autorizado = await CarregarCategorias(client);

        if (!autorizado)
        {
            return IrParaLogin();
        }

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        ProductViewModel product)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var json = JsonSerializer.Serialize(product);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync(
            $"http://localhost:5221/api/products/{product.Id}",
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
                    ? "Não foi possível atualizar o produto."
                    : mensagemApi;

            var autorizado = await CarregarCategorias(client);

            if (!autorizado)
            {
                return IrParaLogin();
            }

            return View(product);
        }

        TempData["Sucesso"] =
            "Produto atualizado com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            $"http://localhost:5221/api/products/{id}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var json = await response.Content.ReadAsStringAsync();

        var product =
            JsonSerializer.Deserialize<ProductViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.DeleteAsync(
            $"http://localhost:5221/api/products/{id}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagemApi =
                await response.Content.ReadAsStringAsync();

            TempData["Erro"] =
                string.IsNullOrWhiteSpace(mensagemApi)
                    ? "Não foi possível excluir o produto."
                    : mensagemApi;

            return RedirectToAction(nameof(Index));
        }

        TempData["Sucesso"] =
            "Produto excluído com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CarregarCategorias(
        HttpClient client)
    {
        var response = await client.GetAsync(
            "http://localhost:5221/api/categories");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }

        var categories =
            new List<CategoryViewModel>();

        if (response.IsSuccessStatusCode)
        {
            var json =
                await response.Content.ReadAsStringAsync();

            categories =
                JsonSerializer.Deserialize<List<CategoryViewModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? new List<CategoryViewModel>();
        }

        ViewBag.Categories = categories;

        return true;
    }

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

    private IActionResult IrParaLogin()
    {
        HttpContext.Session.Remove("JWToken");

        return RedirectToAction(
            "Login",
            "Account");
    }
}