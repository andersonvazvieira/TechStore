using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;

namespace TechStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly TechStoreDbContext _context;

    public DashboardController(
        TechStoreDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // DASHBOARD
    // GET: /api/dashboard
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        // TOTAL DE PRODUTOS
        var totalProducts =
            await _context.Products.CountAsync();

        // TOTAL DE CATEGORIAS
        var totalCategories =
            await _context.Categories.CountAsync();

        // PRODUTOS COM ESTOQUE BAIXO
        var lowStockProducts =
            await _context.Products
                .CountAsync(p =>
                    p.Quantity <= p.MinimumStock);

        // TOTAL DE UNIDADES EM ESTOQUE
        // Funciona corretamente mesmo com banco vazio.
        var totalStock =
            await _context.Products
                .Select(p => (int?)p.Quantity)
                .SumAsync() ?? 0;

        // ÚLTIMAS 5 MOVIMENTAÇÕES
        var recentMovements =
            await _context.StockMovements
                .AsNoTracking()
                .Include(m => m.Product)
                .OrderByDescending(m => m.Date)
                .Take(5)
                .Select(m => new
                {
                    m.Id,
                    m.ProductId,

                    ProductName =
                        m.Product != null
                            ? m.Product.Name
                            : "Produto não encontrado",

                    m.Type,
                    m.Quantity,
                    m.Date,
                    m.Observation
                })
                .ToListAsync();

        return Ok(new
        {
            totalProducts,
            totalCategories,
            lowStockProducts,
            totalStock,
            recentMovements
        });
    }
}