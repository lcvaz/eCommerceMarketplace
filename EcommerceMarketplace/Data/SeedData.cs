using Microsoft.AspNetCore.Identity;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.Data;

/// <summary>
/// Classe responsável por popular dados iniciais no banco
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Cria as roles padrão do sistema se não existirem
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Lista de roles padrão
        string[] roleNames = { "Admin", "Vendedor", "Cliente" };

        foreach (var roleName in roleNames)
        {
            // Verifica se a role já existe
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            
            if (!roleExist)
            {
                // Cria a role se não existir
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}