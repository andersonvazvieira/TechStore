namespace TechStore.API.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    // PERFIL DE ACESSO
    // Valores utilizados: "Admin" ou "User"
    public string Role { get; set; } = "User";
}