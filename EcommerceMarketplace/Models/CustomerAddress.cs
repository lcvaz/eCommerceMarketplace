using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// CustomerAddress é uma tabela de associação (junction table) para o relacionamento N:M
/// entre Customer e Address.
///
/// Um cliente pode ter vários endereços e um endereço pode pertencer a vários clientes.
/// </summary>
public class CustomerAddress
{
    public int Id { get; set; }

    // ========== FOREIGN KEYS ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public int AddressId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;
    public Address Address { get; set; } = null!;

    // ========== METADADOS ==========

    /// <summary>
    /// Se este é o endereço padrão deste cliente
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Data em que este endereço foi associado ao cliente
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
