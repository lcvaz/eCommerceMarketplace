using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Category representa uma categoria de produtos.
/// Exemplos: Eletrônicos, Roupas, Livros, Alimentos, etc.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da categoria é obrigatório")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Slug para URL amigável
    /// Exemplo: "eletronicos", "roupas-masculinas"
    /// </summary>
    [StringLength(100)]
    public string? Slug { get; set; }

    /// <summary>
    /// URL de ícone da categoria (opcional)
    /// </summary>
    [Url(ErrorMessage = "URL inválida")]
    [StringLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Categoria pai (para hierarquia)
    /// Exemplo: "Eletrônicos" > "Smartphones" > "iPhone"
    /// </summary>
    public int? ParentCategoryId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // ========== NAVIGATION PROPERTIES ==========

    /// <summary>
    /// Categoria pai (para subcategorias)
    /// </summary>
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// Subcategorias desta categoria
    /// </summary>
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>
    /// Produtos desta categoria
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}