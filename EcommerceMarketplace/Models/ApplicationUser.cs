using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EcommerceMarketplace.Models;

/// <summary>
/// ApplicationUser representa TODOS os usuários do sistema.
/// Herda de IdentityUser, que já traz Email, Senha (PasswordHash), 
/// PhoneNumber, UserName, e tudo relacionado a autenticação.
/// 
/// Aqui você só adiciona os campos EXTRAS específicos do seu negócio.
/// </summary>  

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Data de criação do usuário
    /// essa sintaxe é para inicializar o campo com a data e hora atual 
    /// sem o set na propriedade o valor será sempre a data e hora atual e não será alterado
    /// </summary>
    public DateTime CriadoEm { get; } = DateTime.UtcNow; 
    
    
    public string FullName { get; set; } = string.Empty;


    /// <summary>
    /// CPF do usuário (específico do Brasil)
    /// O ? torna nullable - nem todo usuário precisa ter CPF
    /// </summary>
    public string? CPF { get; set; }

    // ========== RELACIONAMENTOS ==========
    // Esses relacionamentos variam dependendo da ROLE do usuário
    // Por exemplo, um usuário com a role "Admin" pode ter um relacionamento com a tabela "Admin"

    // O vendedor terá lojas 
    public ICollection<Store> Stores { get; set; } = new List<Store>();

    // O cliente terá pedidos 
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // O cliente terá endereços de entrega
    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    // O cliente terá avaliações feitas
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
        
    // O cliente terá carrinho , porém não obrigatoriamente 
    public Cart? Cart { get; set; }
}