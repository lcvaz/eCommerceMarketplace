namespace EcommerceMarketplace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Product representa um produto de uma loja 
/// Cada loja pode ter vários produtos
/// </summary> 

public class Product 
{
    /// <summary>
    /// ID do produto
    /// </summary>
    [Key]
    public int Id { get; set; }


    /// <summary>
    /// Nome do material
    /// Não pode ser nulo nem maior que 100 caracteres
    /// </summary>
    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Descrição
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }



    /// <summary>
    /// Data de modificação do produto
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow; 
    

    /// <summary>
    /// Status do produto no sistema
    /// </summary>
    public StoreStatus Status { get; set; } = StoreStatus.Active;

    
    
    /// <summary>
    /// Preço do produto no sistema
    /// </summary>
    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    public decimal Price { get; set; };


    // ========== FOREIGN KEY ==========

    /// <summary>
    /// ID da loja 
    /// </summary>
    [Required]
    public string StoreId { get; set; } = string.Empty;


    // ========== Conexões de produto ==========

    /// <summary>
    /// O produto tem ReviewProduct
    /// </summary>
    public ReviewProduct ReviewProduct { get; set; } = null!;




    // ========== PROPRIEDADES CALCULADAS ==========
    // Não vão para o banco de dados, são calculadas em tempo real
    
    /// <summary>
    /// Avaliação média do produto (0 a 5)
    /// Calcula automaticamente baseado nas ReviewProduct
    /// [NotMapped] significa que não cria coluna no banco
    /// </summary>
    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (ReviewProduct == null || ReviewProduct.Count == 0)
                return 0; // FAZER ALTERAÇÃO NA LÓGICA
            
            return Math.Round(ReviewProduct.Average(r => r.Rating), 1);
        }
    }



    // <summary>
    /// Total de avaliações recebidas
    /// </summary>
    [NotMapped]
    public int TotalReviewProduct => ReviewProduct?.Count ?? 0;

}

/// <summary>
/// Enum para status de produto
/// </summary>
public enum StoreStatus
{
    Active = 1,      // Loja ativa e funcionando
    Inactive = 2,    // Desativada temporariamente pelo vendedor
    Suspended = 3    // Suspensa por violação de políticas (ação do admin)
}