using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// OrderItem representa um produto dentro de um pedido.
/// Tabela intermediária entre Order e Product.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    /// <summary>
    /// Quantidade comprada
    /// </summary>
    [Required]
    [Range(1, 999)]
    public int Quantity { get; set; }

    /// <summary>
    /// Preço unitário no momento da compra
    /// Importante: Guardamos o preço histórico porque o preço do produto pode mudar
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Desconto aplicado NESTE item específico (se houver)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    // ========== FOREIGN KEYS ==========

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // ========== PROPRIEDADES CALCULADAS ==========

    /// <summary>
    /// Subtotal deste item (Quantidade × Preço - Desconto)
    /// </summary>
    [NotMapped]
    public decimal Subtotal => (Quantity * UnitPrice) - DiscountAmount;
}