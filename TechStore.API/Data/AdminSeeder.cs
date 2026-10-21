using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.API.Models;

namespace TechStore.API.Data;

public static class AdminSeeder
{
    public static async Task InitializeAsync(
        TechStoreDbContext context,
        IConfiguration configuration)
    {
        // Não modifica contas quando já existe um administrador.
        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
        {
            return;
        }

        var name = configuration["InitialAdmin:Name"];
        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Configure InitialAdmin:Name, InitialAdmin:Email " +
                "e InitialAdmin:Password nos User Secrets " +
                "para criar o primeiro administrador.");
        }

        if (password.Length < 12)
        {
            throw new InvalidOperationException(
                "A senha inicial deve possuir pelo menos 12 caracteres.");
        }

        email = email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException(
                "O e-mail inicial já pertence a outra conta. " +
                "Configure um e-mail diferente.");
        }

        var admin = new User
        {
            Name = name.Trim(),
            Email = email,
            Role = "Admin"
        };

        var passwordHasher = new PasswordHasher<User>();

        admin.PasswordHash = passwordHasher.HashPassword(
            admin,
            password);

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}