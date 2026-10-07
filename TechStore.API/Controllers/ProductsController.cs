using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;
using TechStore.API.Models;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly TechStoreDbContext _context;

    public ProductsController(
        TechStoreDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LISTAR PRODUTOS
    // GET: /api/products
    // =====================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>>
        GetProducts()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    // =====================================================
    // BUSCAR PRODUTO POR ID
    // GET: /api/products/1
    // =====================================================

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>>
        GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(
                "Produto não encontrado.");
        }

        return Ok(product);
    }

    // =====================================================
    // CADASTRAR PRODUTO
    // POST: /api/products
    // =====================================================

    [HttpPost]
    public async Task<ActionResult<Product>>
        CreateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest(
                "O nome do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(product.Code))
        {
            return BadRequest(
                "O código do produto é obrigatório.");
        }

        if (product.Price < 0)
        {
            return BadRequest(
                "O preço não pode ser negativo.");
        }

        if (product.MinimumStock < 0)
        {
            return BadRequest(
                "O estoque mínimo não pode ser negativo.");
        }

        // Normaliza o código.
        // Exemplo: tec001 passa a ser TEC001.
        var code = product.Code
            .Trim()
            .ToUpperInvariant();

        var codeExists = await _context.Products
            .AnyAsync(p => p.Code.ToUpper() == code);

        if (codeExists)
        {
            return BadRequest(
                "Já existe um produto cadastrado com este código.");
        }

        // Verifica se a categoria realmente existe.
        if (product.CategoryId.HasValue)
        {
            var categoryExists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Id == product.CategoryId.Value);

            if (!categoryExists)
            {
                return BadRequest(
                    "A categoria informada não existe.");
            }
        }

        product.Name =
            product.Name.Trim();

        product.Code =
            code;

        product.Description =
            product.Description?.Trim()
            ?? string.Empty;

        // A API controla a data de cadastro.
        product.CreatedAt =
            DateTime.UtcNow;

        // =================================================
        // REGRA DE ESTOQUE
        // =================================================
        // Todo produto novo começa com estoque ZERO.
        //
        // Qualquer quantidade adicionada posteriormente
        // deverá passar por uma movimentação de Entrada.
        //
        // Dessa forma nenhuma alteração de estoque ocorre
        // sem ficar registrada no histórico.
        // =================================================

        product.Quantity = 0;

        // Evita receber objeto Category pelo JSON.
        product.Category = null;

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }

    // =====================================================
    // EDITAR PRODUTO
    // PUT: /api/products/1
    // =====================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(
                "O ID informado é inválido.");
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest(
                "O nome do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(product.Code))
        {
            return BadRequest(
                "O código do produto é obrigatório.");
        }

        if (product.Price < 0)
        {
            return BadRequest(
                "O preço não pode ser negativo.");
        }

        if (product.MinimumStock < 0)
        {
            return BadRequest(
                "O estoque mínimo não pode ser negativo.");
        }

        var existingProduct =
            await _context.Products
                .FirstOrDefaultAsync(
                    p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound(
                "Produto não encontrado.");
        }

        var code = product.Code
            .Trim()
            .ToUpperInvariant();

        var codeExists =
            await _context.Products
                .AnyAsync(p =>
                    p.Id != id &&
                    p.Code.ToUpper() == code);

        if (codeExists)
        {
            return BadRequest(
                "Já existe outro produto cadastrado com este código.");
        }

        if (product.CategoryId.HasValue)
        {
            var categoryExists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Id ==
                        product.CategoryId.Value);

            if (!categoryExists)
            {
                return BadRequest(
                    "A categoria informada não existe.");
            }
        }

        // =================================================
        // ATUALIZA SOMENTE DADOS DO PRODUTO
        // =================================================

        existingProduct.Name =
            product.Name.Trim();

        existingProduct.Code =
            code;

        existingProduct.Description =
            product.Description?.Trim()
            ?? string.Empty;

        existingProduct.Price =
            product.Price;

        existingProduct.MinimumStock =
            product.MinimumStock;

        existingProduct.CategoryId =
            product.CategoryId;

        // NÃO ALTERAMOS:
        //
        // existingProduct.Quantity
        // existingProduct.CreatedAt
        //
        // A quantidade só pode mudar através
        // de Entrada ou Saída de estoque.

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================================
    // EXCLUIR PRODUTO
    // DELETE: /api/products/1
    // =====================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult>
        DeleteProduct(int id)
    {
        var product =
            await _context.Products
                .FirstOrDefaultAsync(
                    p => p.Id == id);

        if (product == null)
        {
            return NotFound(
                "Produto não encontrado.");
        }

        // Produtos que possuem histórico de estoque
        // não podem ser excluídos.
        var hasMovements =
            await _context.StockMovements
                .AnyAsync(m =>
                    m.ProductId == id);

        if (hasMovements)
        {
            return BadRequest(
                "Este produto possui movimentações de estoque e não pode ser excluído.");
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================================
    // PRODUTOS COM ESTOQUE BAIXO
    // GET: /api/products/low-stock
    // =====================================================

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<Product>>>
        GetLowStockProducts()
    {
        var products =
            await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p =>
                    p.Quantity <=
                    p.MinimumStock)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

        return Ok(products);
    }
}