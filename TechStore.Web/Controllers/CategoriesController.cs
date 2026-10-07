using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoriesController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // LISTAR CATEGORIAS
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            "http://localhost:5221/api/categories");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar as categorias.";

            return View(new List<CategoryViewModel>());
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var categories =
            JsonSerializer.Deserialize<List<CategoryViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<CategoryViewModel>();

        return View(categories);
    }

    // EXIBIR CADASTRO
    [HttpGet]
    public IActionResult Create()
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        return View();
    }

    // CADASTRAR CATEGORIA
    [HttpPost]
    public async Task<IActionResult> Create(
        CategoryViewModel category)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var json =
            JsonSerializer.Serialize(category);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/categories",
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
                    ? "Não foi possível cadastrar a categoria."
                    : mensagemApi;

            return View(category);
        }

        TempData["Sucesso"] =
            "Categoria cadastrada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // EXIBIR EDIÇÃO
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            $"http://localhost:5221/api/categories/{id}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var category =
            JsonSerializer.Deserialize<CategoryViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // ATUALIZAR CATEGORIA
    [HttpPost]
    public async Task<IActionResult> Edit(
        CategoryViewModel category)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var json =
            JsonSerializer.Serialize(category);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync(
            $"http://localhost:5221/api/categories/{category.Id}",
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
                    ? "Não foi possível atualizar a categoria."
                    : mensagemApi;

            return View(category);
        }

        TempData["Sucesso"] =
            "Categoria atualizada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // EXIBIR EXCLUSÃO
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            $"http://localhost:5221/api/categories/{id}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var category =
            JsonSerializer.Deserialize<CategoryViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // EXCLUIR CATEGORIA
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
            $"http://localhost:5221/api/categories/{id}");

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
                    ? "Não foi possível excluir a categoria."
                    : mensagemApi;

            return RedirectToAction(nameof(Index));
        }

        TempData["Sucesso"] =
            "Categoria excluída com sucesso.";

        return RedirectToAction(nameof(Index));
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