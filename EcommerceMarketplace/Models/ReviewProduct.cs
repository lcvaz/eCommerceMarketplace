using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// ReviewProduct representa uma avaliação de um produto feita por um cliente.
/// </summary>
public class ReviewProduct
{
    public int Id { get; set; }

    /// <summary>
    /// Nota de 1 a 5 estrelas
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "A avaliação deve ser de 1 a 5 estrelas")]
    public int Rating { get; set; }

    /// <summary>
    /// Comentário do cliente sobre o produto
    /// </summary>
    [Required(ErrorMessage = "O comentário é obrigatório")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "O comentário deve ter entre 10 e 2000 caracteres")]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Título da review (opcional)
    /// Exemplo: "Produto excelente!", "Não recomendo"
    /// </summary>
    [StringLength(200)]
    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Se a compra foi verificada (cliente realmente comprou o produto)
    /// </summary>
    public bool IsVerifiedPurchase { get; set; } = false;

    // ========== CAMPOS ESPECÍFICOS DE PRODUTO ==========

    /// <summary>
    /// Se o cliente recomenda o produto
    /// </summary>
    public bool RecommendProduct { get; set; } = true;

    /// <summary>
    /// Tamanho/Variação do produto comprado
    /// Exemplo: "M", "42", "Azul"
    /// </summary>
    [StringLength(50)]
    public string? ProductVariation { get; set; }

    /// <summary>
    /// Se o tamanho/produto correspondeu ao esperado
    /// </summary>
    public bool? FitsAsExpected { get; set; }

    /// <summary>
    /// Qualidade percebida (1-5)
    /// </summary>
    [Range(1, 5)]
    public int? QualityRating { get; set; }

    /// <summary>
    /// Custo-benefício (1-5)
    /// </summary>
    [Range(1, 5)]
    public int? ValueForMoneyRating { get; set; }

    // ========== FOREIGN KEYS ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}