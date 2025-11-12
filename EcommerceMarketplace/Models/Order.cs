using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Order representa um pedido de compra feito por um cliente.
/// </summary>
public class Order
{
    public int Id { get; set; }

    /// <summary>
    /// Número do pedido (visível para o cliente)
    /// Exemplo: "PED-2025-001234"
    /// </summary>
    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status atual do pedido
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Valor total dos produtos
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubtotalAmount { get; set; }

    /// <summary>
    /// Valor do frete
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Descontos aplicados
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Valor total final (Subtotal + Frete - Desconto)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Método de pagamento
    /// </summary>
    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Data de pagamento confirmado
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Data de envio
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Data de entrega
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Data de cancelamento
    /// </summary>
    public DateTime? CanceledAt { get; set; }

    /// <summary>
    /// Motivo do cancelamento (se houver)
    /// </summary>
    [StringLength(500)]
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Código de rastreio da entrega
    /// </summary>
    [StringLength(100)]
    public string? TrackingCode { get; set; }

    /// <summary>
    /// Observações adicionais do cliente
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    // ========== FOREIGN KEYS ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public int ShippingAddressId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;
    public Address ShippingAddress { get; set; } = null!;

    /// <summary>
    /// Itens deste pedido
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // ========== PROPRIEDADES CALCULADAS ==========

    [NotMapped]
    public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;

    [NotMapped]
    public bool IsPaid => PaidAt.HasValue;

    [NotMapped]
    public bool IsDelivered => DeliveredAt.HasValue;

    [NotMapped]
    public bool IsCanceled => CanceledAt.HasValue;
}

/// <summary>
/// Status possíveis de um pedido
/// </summary>
public enum OrderStatus
{
    Pending = 1,           // Pendente (aguardando pagamento)
    PaymentConfirmed = 2,  // Pagamento confirmado
    Processing = 3,        // Em processamento (separando produtos)
    Shipped = 4,           // Enviado
    Delivered = 5,         // Entregue
    Canceled = 6,          // Cancelado
    Returned = 7           // Devolvido
}