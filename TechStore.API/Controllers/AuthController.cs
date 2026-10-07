using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TechStore.API.Data;
using TechStore.API.DTOs;
using TechStore.API.Models;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TechStoreDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        TechStoreDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    // =====================================================
    // CADASTRAR NOVO USUÁRIO
    // SOMENTE ADMIN
    // POST: /api/auth/register
    // =====================================================

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(
        RegisterUserDto registerDto)
    {
        if (string.IsNullOrWhiteSpace(registerDto.Name) ||
            string.IsNullOrWhiteSpace(registerDto.Email) ||
            string.IsNullOrWhiteSpace(registerDto.Password))
        {
            return BadRequest(
                "Nome, e-mail e senha são obrigatórios.");
        }

        if (registerDto.Password.Length < 6)
        {
            return BadRequest(
                "A senha deve possuir pelo menos 6 caracteres.");
        }

        var email = registerDto.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return BadRequest(
                "Já existe um usuário cadastrado com este e-mail.");
        }

        var user = new User
        {
            Name = registerDto.Name.Trim(),
            Email = email,
            Role = "User"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                registerDto.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Usuário cadastrado com sucesso."
        });
    }

    // =====================================================
    // LOGIN
    // CONTINUA PÚBLICO
    // POST: /api/auth/login
    // =====================================================

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email) ||
            string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest(
                "E-mail e senha são obrigatórios.");
        }

        var email = loginDto.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Unauthorized(
                "E-mail ou senha inválidos.");
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginDto.Password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(
                "E-mail ou senha inválidos.");
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Login realizado com sucesso.",

            token,

            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            }
        });
    }

    // =====================================================
    // GERAR TOKEN JWT
    // =====================================================

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "A chave JWT não foi configurada.");

        var jwtIssuer =
            _configuration["Jwt:Issuer"];

        var jwtAudience =
            _configuration["Jwt:Audience"];

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Name),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}