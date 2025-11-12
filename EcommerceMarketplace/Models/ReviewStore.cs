using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// ReviewStore representa uma avaliação de uma loja feita por um cliente.
/// </summary>
public class ReviewStore
{
    public int Id { get; set; }

    /// <summary>
    /// Nota geral de 1 a 5 estrelas
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "A avaliação deve ser de 1 a 5 estrelas")]
    public int Rating { get; set; }

    /// <summary>
    /// Comentário do cliente sobre a loja
    /// </summary>
    [Required(ErrorMessage = "O comentário é obrigatório")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "O comentário deve ter entre 10 e 2000 caracteres")]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Título da review (opcional)
    /// </summary>
    [StringLength(200)]
    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Se a compra foi verificada
    /// </summary>
    public bool IsVerifiedPurchase { get; set; } = false;

    // ========== CAMPOS ESPECÍFICOS DE LOJA ==========

    /// <summary>
    /// Avaliação do tempo de entrega (1-5)
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int ShippingRating { get; set; }

    /// <summary>
    /// Avaliação do atendimento (1-5)
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int ServiceRating { get; set; }

    /// <summary>
    /// Avaliação da embalagem (1-5)
    /// </summary>
    [Range(1, 5)]
    public int? PackagingRating { get; set; }

    /// <summary>
    /// Quantos dias levou para entregar
    /// </summary>
    [Range(0, 365)]
    public int? DeliveryDays { get; set; }

    /// <summary>
    /// Se compraria novamente nesta loja
    /// </summary>
    public bool WouldBuyAgain { get; set; } = true;

    // ========== FOREIGN KEYS ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public int StoreId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;
    public Store Store { get; set; } = null!;
}