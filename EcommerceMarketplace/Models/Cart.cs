using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Cart representa o carrinho de compras de um cliente.
/// Cada cliente tem UM carrinho.
/// </summary>
public class Cart
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ========== FOREIGN KEY ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;

    /// <summary>
    /// Itens dentro do carrinho
    /// </summary>
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // ========== PROPRIEDADES CALCULADAS ==========

    /// <summary>
    /// Total de itens no carrinho
    /// </summary>
    [NotMapped]
    public int TotalItems => CartItems?.Sum(item => item.Quantity) ?? 0;

    /// <summary>
    /// Valor total do carrinho
    /// </summary>
    [NotMapped]
    public decimal TotalAmount => CartItems?.Sum(item => item.Subtotal) ?? 0;

    /// <summary>
    /// Se o carrinho está vazio
    /// </summary>
    [NotMapped]
    public bool IsEmpty => CartItems == null || !CartItems.Any();
}