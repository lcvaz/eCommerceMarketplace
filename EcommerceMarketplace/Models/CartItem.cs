using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// CartItem representa um produto dentro do carrinho.
/// Tabela intermediária entre Cart e Product.
/// </summary>
public class CartItem
{
    public int Id { get; set; }

    /// <summary>
    /// Quantidade deste produto no carrinho
    /// </summary>
    [Required]
    [Range(1, 999, ErrorMessage = "Quantidade deve ser entre 1 e 999")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Preço unitário no momento que foi adicionado ao carrinho
    /// Guardamos o preço para não mudar se o produto mudar de preço depois
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // ========== FOREIGN KEYS ==========

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // ========== PROPRIEDADES CALCULADAS ==========

    /// <summary>
    /// Subtotal deste item (Quantidade × Preço)
    /// </summary>
    [NotMapped]
    public decimal Subtotal => Quantity * UnitPrice;
}