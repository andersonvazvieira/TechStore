using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Data;
using TechStore.API.Models;
using Xunit;

namespace TechStore.Tests;

public class StockTriggerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TechStoreDbContext _context;

    public StockTriggerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TechStoreDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TechStoreDbContext(options);

        // Aplica as migrations, incluindo as triggers.
        _context.Database.Migrate();
    }

    [Fact]
    public async Task InserirSaldoNegativo_DeveSerBloqueadoPeloBanco()
    {
        _context.Products.Add(CreateProduct(-1));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => _context.SaveChangesAsync());

        var sqliteException =
            Assert.IsType<SqliteException>(exception.InnerException);

        Assert.Equal(19, sqliteException.SqliteErrorCode);
        Assert.Contains(
            "O estoque nao pode ser negativo.",
            sqliteException.Message);

        _context.ChangeTracker.Clear();

        Assert.Equal(0, await _context.Products.CountAsync());
    }

    [Fact]
    public async Task AtualizarSaldoParaNegativo_DevePreservarSaldoAnterior()
    {
        var product = CreateProduct(10);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Executa SQL diretamente para testar a proteção do banco.
        var exception = await Assert.ThrowsAsync<SqliteException>(
            async () =>
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Products SET Quantity = {-1} WHERE Id = {product.Id}");
            });

        Assert.Equal(19, exception.SqliteErrorCode);
        Assert.Contains(
            "O estoque nao pode ser negativo.",
            exception.Message);

        _context.ChangeTracker.Clear();

        var saved = await _context.Products.SingleAsync();

        Assert.Equal(10, saved.Quantity);
    }

    [Fact]
    public async Task SaldoZeroOuPositivo_DeveSerPermitido()
    {
        var product = CreateProduct(0);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Products SET Quantity = {5} WHERE Id = {product.Id}");

        _context.ChangeTracker.Clear();

        var saved = await _context.Products.SingleAsync();

        Assert.Equal(5, saved.Quantity);
    }

    private static Product CreateProduct(int quantity)
    {
        return new Product
        {
            Name = "Produto de teste",
            Code = "TRIGGER001",
            Description = "Teste das triggers",
            Price = 10m,
            Quantity = quantity,
            MinimumStock = 0
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}