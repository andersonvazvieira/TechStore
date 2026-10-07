using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class UsersController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UsersController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // =====================================================
    // LISTAR USUÁRIOS
    // SOMENTE ADMIN
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!UsuarioEhAdmin())
        {
            return RedirecionarSemPermissao();
        }

        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            "http://localhost:5221/api/users");

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            return RedirecionarSemPermissao();
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "Não foi possível carregar os usuários.";

            return View(
                new List<UserViewModel>());
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var users =
            JsonSerializer.Deserialize<List<UserViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? new List<UserViewModel>();

        return View(users);
    }

    // =====================================================
    // EDITAR USUÁRIO - TELA
    // SOMENTE ADMIN
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!UsuarioEhAdmin())
        {
            return RedirecionarSemPermissao();
        }

        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.GetAsync(
            $"http://localhost:5221/api/users/{id}");

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            return RedirecionarSemPermissao();
        }

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            TempData["Erro"] =
                "Não foi possível carregar o usuário.";

            return RedirectToAction(nameof(Index));
        }

        var json =
            await response.Content.ReadAsStringAsync();

        var user =
            JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // =====================================================
    // EDITAR USUÁRIO - SALVAR
    // SOMENTE ADMIN
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Edit(
        UserViewModel model)
    {
        if (!UsuarioEhAdmin())
        {
            return RedirecionarSemPermissao();
        }

        if (string.IsNullOrWhiteSpace(model.Name) ||
            string.IsNullOrWhiteSpace(model.Email))
        {
            ViewBag.Erro =
                "Nome e e-mail são obrigatórios.";

            return View(model);
        }

        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var dados = new
        {
            name = model.Name,
            email = model.Email
        };

        var json =
            JsonSerializer.Serialize(dados);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync(
            $"http://localhost:5221/api/users/{model.Id}",
            content);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            return RedirecionarSemPermissao();
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagem =
                await response.Content.ReadAsStringAsync();

            ViewBag.Erro =
                string.IsNullOrWhiteSpace(mensagem)
                    ? "Não foi possível atualizar o usuário."
                    : mensagem.Trim('"');

            return View(model);
        }

        TempData["Sucesso"] =
            "Usuário atualizado com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // EXCLUIR USUÁRIO
    // SOMENTE ADMIN
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!UsuarioEhAdmin())
        {
            return RedirecionarSemPermissao();
        }

        var client = CriarClienteAutenticado();

        if (client == null)
        {
            return IrParaLogin();
        }

        var response = await client.DeleteAsync(
            $"http://localhost:5221/api/users/{id}");

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            return IrParaLogin();
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            return RedirecionarSemPermissao();
        }

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            TempData["Erro"] =
                "Usuário não encontrado.";

            return RedirectToAction(nameof(Index));
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagem =
                await response.Content.ReadAsStringAsync();

            TempData["Erro"] =
                string.IsNullOrWhiteSpace(mensagem)
                    ? "Não foi possível excluir o usuário."
                    : mensagem.Trim('"');

            return RedirectToAction(nameof(Index));
        }

        TempData["Sucesso"] =
            "Usuário excluído com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // VERIFICAR SE É ADMINISTRADOR
    // =====================================================

    private bool UsuarioEhAdmin()
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        var role =
            HttpContext.Session.GetString("UserRole");

        return !string.IsNullOrWhiteSpace(token) &&
               role == "Admin";
    }

    // =====================================================
    // CRIAR HTTP CLIENT COM JWT
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
    // SESSÃO EXPIRADA / NÃO AUTENTICADO
    // =====================================================

    private IActionResult IrParaLogin()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(
            "Login",
            "Account");
    }

    // =====================================================
    // USUÁRIO SEM PERMISSÃO
    // =====================================================

    private IActionResult RedirecionarSemPermissao()
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return IrParaLogin();
        }

        TempData["Erro"] =
            "Você não possui permissão para acessar a área de usuários.";

        return RedirectToAction(
            "Index",
            "Home");
    }
}