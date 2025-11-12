using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// ReviewProductImage representa uma imagem enviada em uma review de produto.
/// Uma review pode ter várias imagens.
/// </summary>
public class ReviewProductImage
{
    public int Id { get; set; }

    /// <summary>
    /// URL da imagem armazenada
    /// Exemplo: "/images/reviews/produto-123-foto1.jpg"
    /// </summary>
    [Required]
    [Url(ErrorMessage = "URL inválida")]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Ordem de exibição (primeira foto, segunda foto, etc)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Descrição alternativa da imagem (acessibilidade)
    /// </summary>
    [StringLength(200)]
    public string? AltText { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // ========== FOREIGN KEY ==========

    [Required]
    public int ReviewProductId { get; set; }

    // ========== NAVIGATION PROPERTY ==========

    public ReviewProduct ReviewProduct { get; set; } = null!;
}