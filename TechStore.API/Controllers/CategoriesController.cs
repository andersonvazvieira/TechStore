using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;
using TechStore.API.Models;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly TechStoreDbContext _context;

    public CategoriesController(
        TechStoreDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LISTAR CATEGORIAS
    // GET: /api/categories
    // =====================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>>
        GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    // =====================================================
    // BUSCAR CATEGORIA POR ID
    // GET: /api/categories/1
    // =====================================================

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>>
        GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound(
                "Categoria não encontrada.");
        }

        return Ok(category);
    }

    // =====================================================
    // CADASTRAR CATEGORIA
    // POST: /api/categories
    // =====================================================

    [HttpPost]
    public async Task<ActionResult<Category>>
        CreateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest(
                "O nome da categoria é obrigatório.");
        }

        var name = category.Name.Trim();

        var nameExists = await _context.Categories
            .AnyAsync(c =>
                c.Name.ToLower() == name.ToLower());

        if (nameExists)
        {
            return BadRequest(
                "Já existe uma categoria com este nome.");
        }

        category.Name = name;

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            category);
    }

    // =====================================================
    // EDITAR CATEGORIA
    // PUT: /api/categories/1
    // =====================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        Category category)
    {
        if (id != category.Id)
        {
            return BadRequest(
                "O ID informado é inválido.");
        }

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest(
                "O nome da categoria é obrigatório.");
        }

        var existingCategory =
            await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCategory == null)
        {
            return NotFound(
                "Categoria não encontrada.");
        }

        var name = category.Name.Trim();

        var nameExists = await _context.Categories
            .AnyAsync(c =>
                c.Id != id &&
                c.Name.ToLower() == name.ToLower());

        if (nameExists)
        {
            return BadRequest(
                "Já existe outra categoria com este nome.");
        }

        existingCategory.Name = name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================================
    // EXCLUIR CATEGORIA
    // DELETE: /api/categories/1
    // =====================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult>
        DeleteCategory(int id)
    {
        var category =
            await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound(
                "Categoria não encontrada.");
        }

        // Não permite excluir categoria
        // utilizada por algum produto.
        var hasProducts =
            await _context.Products
                .AnyAsync(p =>
                    p.CategoryId == id);

        if (hasProducts)
        {
            return BadRequest(
                "Esta categoria possui produtos vinculados e não pode ser excluída.");
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}