using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Product representa um produto de uma loja 
/// </summary> 
public class Product
{
    [Required(ErrorMessage = "O ID do produto é obrigatório")]
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Preço do produto
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")] 
    public decimal Price { get; set; }  

    /// <summary>
    /// Quantidade em estoque
    /// </summary>
    [Required]
    public int Stock { get; set; } = 0;

    /// <summary>
    /// SKU - Código único do produto
    /// </summary>
    [StringLength(50)]
    public string SKU { get; set; }

    /// <summary>
    /// URL da imagem principal do produto
    /// </summary>
    [Url(ErrorMessage = "URL inválida")]
    public string ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status do produto
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    // ========== FOREIGN KEYS ==========

    /// <summary>
    /// ID da loja dona do produto
    /// </summary>
    [Required]
    public int StoreId { get; set; }  

    /// <summary>
    /// ID da categoria (opcional)
    /// </summary>
    public int? CategoryId { get; set; }

    // ========== NAVIGATION PROPERTIES ==========

    /// <summary>
    /// A loja dona deste produto
    /// </summary>
    public Store Store { get; set; } = null!;

    /// <summary>
    /// Categoria do produto
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Avaliações deste produto
    /// </summary>
    public ICollection<ReviewProduct> ReviewsProduct { get; set; } = new List<ReviewProduct>();  

    /// <summary>
    /// Itens de pedido que contêm este produto
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Itens de carrinho que contêm este produto
    /// </summary>
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // ========== PROPRIEDADES CALCULADAS ==========

    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (ReviewsProduct == null || !ReviewsProduct.Any())
                return 0;
            
            return Math.Round(ReviewsProduct.Average(r => r.Rating), 1);
        }
    }

    [NotMapped]
    public int TotalReviews => ReviewsProduct?.Count ?? 0;

    [NotMapped]
    public bool IsAvailable => Status == ProductStatus.Available && Stock > 0;
}

/// <summary>
/// Status possíveis de um produto
/// </summary>
public enum ProductStatus
{
    Available = 1,      // Disponível para venda
    OutOfStock = 2,     // Sem estoque
    Discontinued = 3,   // Descontinuado
    Draft = 4          // Rascunho (ainda não publicado)
}