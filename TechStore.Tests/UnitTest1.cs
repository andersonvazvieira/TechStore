using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Controllers;
using TechStore.API.Data;
using TechStore.API.Models;
using Xunit;

namespace TechStore.Tests;

public class StockMovementTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TechStoreDbContext _context;
    private readonly StockMovementsController _controller;

    public StockMovementTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TechStoreDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TechStoreDbContext(options);
        _context.Database.EnsureCreated();

        _context.Products.Add(new Product
        {
            Id = 1,
            Name = "Teclado",
            Code = "TEC001",
            Price = 100m,
            Quantity = 10,
            MinimumStock = 3
        });

        _context.SaveChanges();

        _controller = new StockMovementsController(_context);
    }

    [Fact]
    public async Task Entrada_DeveAumentarEstoqueERegistrarMovimentacao()
    {
        var movement = CreateMovement("Entrada", 5);

        var result = await _controller.CreateStockMovement(movement);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        await AssertDatabaseAsync(15, 1);

        var saved = await _context.StockMovements.SingleAsync();
        Assert.Equal("Entrada", saved.Type);
        Assert.Equal(5, saved.Quantity);
        Assert.Equal(1, saved.ProductId);
    }

    [Fact]
    public async Task Saida_DeveDiminuirEstoqueERegistrarMovimentacao()
    {
        var movement = CreateMovement("Saida", 4);

        var result = await _controller.CreateStockMovement(movement);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        await AssertDatabaseAsync(6, 1);

        var saved = await _context.StockMovements.SingleAsync();
        Assert.Equal("Saida", saved.Type);
        Assert.Equal(4, saved.Quantity);
    }

    [Fact]
    public async Task SaidaIgualAoSaldo_DeveZerarEstoque()
    {
        var result = await _controller.CreateStockMovement(
            CreateMovement("Saida", 10));

        Assert.IsType<CreatedAtActionResult>(result.Result);
        await AssertDatabaseAsync(0, 1);
    }

    [Fact]
    public async Task SaidaMaiorQueSaldo_DeveRejeitarSemAlterarBanco()
    {
        var result = await _controller.CreateStockMovement(
            CreateMovement("Saida", 11));

        Assert.IsType<BadRequestObjectResult>(result.Result);
        await AssertDatabaseAsync(10, 0);
    }

    [Theory]
    [InlineData("Entrada", 0)]
    [InlineData("Entrada", -1)]
    [InlineData("Saida", 0)]
    [InlineData("Saida", -1)]
    public async Task QuantidadeInvalida_DeveRejeitarSemAlterarBanco(
        string type, int quantity)
    {
        var result = await _controller.CreateStockMovement(
            CreateMovement(type, quantity));

        Assert.IsType<BadRequestObjectResult>(result.Result);
        await AssertDatabaseAsync(10, 0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Compra")]
    [InlineData("Transferencia")]
    public async Task TipoInvalido_DeveRejeitarSemAlterarBanco(string type)
    {
        var result = await _controller.CreateStockMovement(
            CreateMovement(type, 2));

        Assert.IsType<BadRequestObjectResult>(result.Result);
        await AssertDatabaseAsync(10, 0);
    }

    [Fact]
    public async Task ProdutoInexistente_DeveRejeitarSemAlterarBanco()
    {
        var movement = CreateMovement("Entrada", 2);
        movement.ProductId = 999;

        var result = await _controller.CreateStockMovement(movement);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        await AssertDatabaseAsync(10, 0);
    }

    [Theory]
    [InlineData(" entrada ", "Entrada", 12)]
    [InlineData(" sAiDa ", "Saida", 8)]
    public async Task Tipo_DeveAceitarEspacosELetrasDiferentes(
        string receivedType, string savedType, int expectedStock)
    {
        var result = await _controller.CreateStockMovement(
            CreateMovement(receivedType, 2));

        Assert.IsType<CreatedAtActionResult>(result.Result);
        await AssertDatabaseAsync(expectedStock, 1);

        var saved = await _context.StockMovements.SingleAsync();
        Assert.Equal(savedType, saved.Type);
    }

    private static StockMovement CreateMovement(string type, int quantity)
    {
        return new StockMovement
        {
            ProductId = 1,
            Type = type,
            Quantity = quantity,
            Observation = "Teste automatizado"
        };
    }

    private async Task AssertDatabaseAsync(
        int expectedStock, int expectedMovements)
    {
        // Recarrega os dados para conferir o que foi salvo no banco.
        _context.ChangeTracker.Clear();

        var product = await _context.Products.SingleAsync();
        Assert.Equal(expectedStock, product.Quantity);

        var count = await _context.StockMovements.CountAsync();
        Assert.Equal(expectedMovements, count);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
