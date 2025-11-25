using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Address representa um endereço que pode ser usado por clientes e lojas.
///
/// Relacionamentos:
/// - Um endereço pode ter vários clientes (N:M via CustomerAddress)
/// - Um endereço pode ter várias lojas (1:N)
/// - Um endereço pode ser usado em vários pedidos (1:N)
/// </summary>
public class Address
{
    public int Id { get; set; }

    /// <summary>
    /// CEP - Código de Endereçamento Postal
    /// Formato: 00000-000
    /// </summary>
    [Required]
    [StringLength(9)]
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Rua/Avenida
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Número da residência/estabelecimento
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Complemento (Apto, Bloco, Sala, etc)
    /// </summary>
    [StringLength(100)]
    public string? Complement { get; set; }

    /// <summary>
    /// Bairro
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    /// <summary>
    /// Cidade
    /// </summary>
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Estado (UF)
    /// Exemplo: SP, RJ, MG
    /// </summary>
    [Required]
    [StringLength(2)]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado deve ter 2 letras maiúsculas")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// País (geralmente "Brasil" para marketplace brasileiro)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Country { get; set; } = "Brasil";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ========== NAVIGATION PROPERTIES ==========

    /// <summary>
    /// Associações com clientes (relação N:M)
    /// </summary>
    public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    /// <summary>
    /// Lojas que usam este endereço
    /// </summary>
    public ICollection<Store> Stores { get; set; } = new List<Store>();

    /// <summary>
    /// Pedidos que usaram este endereço
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}