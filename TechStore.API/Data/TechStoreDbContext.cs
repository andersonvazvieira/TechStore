using Microsoft.EntityFrameworkCore;
using TechStore.API.Models;

namespace TechStore.API.Data;

public class TechStoreDbContext : DbContext
{
    public TechStoreDbContext(
        DbContextOptions<TechStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products =>
        Set<Product>();

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<StockMovement> StockMovements =>
        Set<StockMovement>();

    public DbSet<User> Users =>
        Set<User>();

    // =====================================================
    // CONFIGURAÇÕES DO BANCO
    // =====================================================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CÓDIGO DO PRODUTO DEVE SER ÚNICO
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // E-MAIL DO USUÁRIO DEVE SER ÚNICO
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}