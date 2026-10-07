using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly TechStoreDbContext _context;

    public UsersController(
        TechStoreDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LISTAR USUÁRIOS
    // GET: /api/users
    // SOMENTE ADMIN
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // =====================================================
    // BUSCAR USUÁRIO
    // GET: /api/users/1
    // SOMENTE ADMIN
    // =====================================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(
                "Usuário não encontrado.");
        }

        return Ok(user);
    }

    // =====================================================
    // EDITAR USUÁRIO
    // PUT: /api/users/1
    // SOMENTE ADMIN
    // =====================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        UpdateUserDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) ||
            string.IsNullOrWhiteSpace(model.Email))
        {
            return BadRequest(
                "Nome e e-mail são obrigatórios.");
        }

        var user = await _context.Users
            .FindAsync(id);

        if (user == null)
        {
            return NotFound(
                "Usuário não encontrado.");
        }

        var email = model.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Email == email &&
                u.Id != id);

        if (emailExists)
        {
            return BadRequest(
                "Já existe outro usuário cadastrado com este e-mail.");
        }

        user.Name = model.Name.Trim();
        user.Email = email;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Usuário atualizado com sucesso."
        });
    }

    // =====================================================
    // EXCLUIR USUÁRIO
    // DELETE: /api/users/1
    // SOMENTE ADMIN
    // =====================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // ID DO USUÁRIO QUE ESTÁ AUTENTICADO
        var loggedUserIdText =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
            loggedUserIdText,
            out var loggedUserId))
        {
            return Unauthorized(
                "Não foi possível identificar o usuário autenticado.");
        }

        // ADMIN NÃO PODE EXCLUIR A PRÓPRIA CONTA
        if (loggedUserId == id)
        {
            return BadRequest(
                "Você não pode excluir a sua própria conta.");
        }

        var user = await _context.Users
            .FindAsync(id);

        if (user == null)
        {
            return NotFound(
                "Usuário não encontrado.");
        }

        // PROTEÇÃO CONTRA EXCLUSÃO DO ÚLTIMO ADMINISTRADOR
        if (user.Role == "Admin")
        {
            var adminCount =
                await _context.Users
                    .CountAsync(u =>
                        u.Role == "Admin");

            if (adminCount <= 1)
            {
                return BadRequest(
                    "O último administrador do sistema não pode ser excluído.");
            }
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Usuário excluído com sucesso."
        });
    }
}

// =========================================================
// DTO PARA EDIÇÃO
// =========================================================

public class UpdateUserDto
{
    public string Name { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;
}