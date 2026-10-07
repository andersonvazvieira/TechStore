using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // =====================================================
    // EXIBIR TELA DE LOGIN
    // =====================================================

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // =====================================================
    // REALIZAR LOGIN
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password))
        {
            ViewBag.Erro =
                "Informe o e-mail e a senha.";

            return View(model);
        }

        var client =
            _httpClientFactory.CreateClient();

        var dadosLogin = new
        {
            email = model.Email,
            password = model.Password
        };

        var json =
            JsonSerializer.Serialize(dadosLogin);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/auth/login",
            content);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Erro =
                "E-mail ou senha inválidos.";

            return View(model);
        }

        var respostaJson =
            await response.Content.ReadAsStringAsync();

        var loginResponse =
            JsonSerializer.Deserialize<JsonElement>(
                respostaJson);

        var token =
            loginResponse
                .GetProperty("token")
                .GetString();

        if (string.IsNullOrWhiteSpace(token))
        {
            ViewBag.Erro =
                "Não foi possível realizar o login.";

            return View(model);
        }

        // GUARDAR JWT NA SESSION
        HttpContext.Session.SetString(
            "JWToken",
            token);

        // GUARDAR ROLE NA SESSION
        if (loginResponse.TryGetProperty(
                "user",
                out var userElement) &&
            userElement.TryGetProperty(
                "role",
                out var roleElement))
        {
            var role =
                roleElement.GetString();

            if (!string.IsNullOrWhiteSpace(role))
            {
                HttpContext.Session.SetString(
                    "UserRole",
                    role);
            }
        }

        return RedirectToAction(
            "Index",
            "Home");
    }

    // =====================================================
    // EXIBIR CADASTRO DE USUÁRIO
    // SOMENTE ADMIN
    // =====================================================

    [HttpGet]
    public IActionResult Register()
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        var role =
            HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        if (role != "Admin")
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        return View();
    }

    // =====================================================
    // CADASTRAR USUÁRIO
    // SOMENTE ADMIN
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> Register(
        RegisterUserViewModel model)
    {
        var token =
            HttpContext.Session.GetString("JWToken");

        var role =
            HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        if (role != "Admin")
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        if (string.IsNullOrWhiteSpace(model.Name) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            ViewBag.Erro =
                "Preencha todos os campos.";

            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ViewBag.Erro =
                "As senhas não são iguais.";

            return View(model);
        }

        if (model.Password.Length < 6)
        {
            ViewBag.Erro =
                "A senha deve possuir pelo menos 6 caracteres.";

            return View(model);
        }

        var client =
            _httpClientFactory.CreateClient();

        // ENVIAR JWT DO ADMIN PARA A API
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var novoUsuario = new
        {
            name = model.Name,
            email = model.Email,
            password = model.Password
        };

        var json =
            JsonSerializer.Serialize(novoUsuario);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "http://localhost:5221/api/auth/register",
            content);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Account");
        }

        if (response.StatusCode ==
            HttpStatusCode.Forbidden)
        {
            ViewBag.Erro =
                "Você não possui permissão para cadastrar usuários.";

            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            var mensagemApi =
                await response.Content.ReadAsStringAsync();

            ViewBag.Erro =
                string.IsNullOrWhiteSpace(mensagemApi)
                    ? "Não foi possível cadastrar o usuário."
                    : mensagemApi.Trim('"');

            return View(model);
        }

        TempData["Sucesso"] =
            "Usuário cadastrado com sucesso.";

        return RedirectToAction(
            "Index",
            "Users");
    }

    // =====================================================
    // LOGOUT
    // =====================================================

    [HttpPost]
    public IActionResult Logout()
    {
        // REMOVE TOKEN E PERFIL
        HttpContext.Session.Clear();

        return RedirectToAction(
            "Login",
            "Account");
    }
}