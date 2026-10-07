using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;
using TechStore.API.Models;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly TechStoreDbContext _context;

    public StockMovementsController(
        TechStoreDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // LISTAR MOVIMENTAÇÕES
    // GET: /api/stockmovements
    // =====================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockMovement>>>
        GetStockMovements()
    {
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        return Ok(movements);
    }

    // =====================================================
    // BUSCAR MOVIMENTAÇÃO POR ID
    // GET: /api/stockmovements/1
    // =====================================================

    [HttpGet("{id}")]
    public async Task<ActionResult<StockMovement>>
        GetStockMovement(int id)
    {
        var movement = await _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movement == null)
        {
            return NotFound(
                "Movimentação não encontrada.");
        }

        return Ok(movement);
    }

    // =====================================================
    // REGISTRAR ENTRADA OU SAÍDA
    // POST: /api/stockmovements
    // =====================================================

    [HttpPost]
    public async Task<ActionResult<StockMovement>>
        CreateStockMovement(StockMovement movement)
    {
        // QUANTIDADE DEVE SER MAIOR QUE ZERO
        if (movement.Quantity <= 0)
        {
            return BadRequest(
                "A quantidade deve ser maior que zero.");
        }

        // NORMALIZAR TIPO
        var type = movement.Type?.Trim();

        if (!string.Equals(
                type,
                "Entrada",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                type,
                "Saida",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "O tipo da movimentação deve ser Entrada ou Saida.");
        }

        // BUSCAR PRODUTO
        var product = await _context.Products
            .FirstOrDefaultAsync(
                p => p.Id == movement.ProductId);

        if (product == null)
        {
            return NotFound(
                "Produto não encontrado.");
        }

        // =================================================
        // ENTRADA
        // =================================================

        if (string.Equals(
            type,
            "Entrada",
            StringComparison.OrdinalIgnoreCase))
        {
            product.Quantity += movement.Quantity;

            movement.Type = "Entrada";
        }

        // =================================================
        // SAÍDA
        // =================================================

        else
        {
            if (product.Quantity < movement.Quantity)
            {
                return BadRequest(
                    $"Estoque insuficiente. " +
                    $"Quantidade disponível: {product.Quantity}.");
            }

            product.Quantity -= movement.Quantity;

            movement.Type = "Saida";
        }

        // DATA DEFINIDA PELA API
        movement.Date = DateTime.UtcNow;

        // EVITA VÍNCULO INDEVIDO RECEBIDO NO JSON
        movement.Product = null;

        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetStockMovement),
            new { id = movement.Id },
            movement);
    }
}