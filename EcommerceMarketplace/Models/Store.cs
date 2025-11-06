namespace EcommerceMarketplace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Store representa uma loja de um vendedor no marketplace.
/// Cada vendedor pode ter uma ou mais lojas.
/// </summary> 

public class Store 
{
    /// <summary>
    /// ID da loja
    /// </summary>
    [Key]
    public int IdStore { get; set; }


    /// <summary>
    /// Nome da loja
    /// Não pode ser nulo nem maior que 100 caracteres
    /// </summary>
    [Required(ErrorMessage = "O nome da loja é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Descrição
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }


    /// <summary>
    /// URL da logo da loja (caminho do arquivo)
    /// Exemplo: "/images/stores/logo-123.png"
    /// </summary>
    public string? LogoUrl { get; set; }


    /// <summary>
    /// CNPJ da loja (para vendedores pessoa jurídica)
    /// Opcional porque pode ser pessoa física
    /// </summary>
    [StringLength(18)] // 00.000.000/0000-00
    public string? CNPJ { get; set; }


    /// <summary>
    /// Data de criação da loja
    /// Essa sintaxe é para inicializar o campo com a data e hora atual 
    /// Sem o set na propriedade o valor será sempre a data e hora atual e não será alterado
    /// </summary>
    public DateTime CriadoEm { get; } = DateTime.UtcNow; 
    

    /// <summary>
    /// Status da loja no sistema
    /// </summary>
    public StoreStatus Status { get; set; } = StoreStatus.Active;

    
    /// <summary>
    /// avaliação da loja de 0 a 5
    /// </summary>
    public int avaliacao 
    { 
        get; 
        set {
            // calcula a média da lista de avaliações 
            //if (ListaDeAvaliacoes.Count > 0)
            //{
            //    return (int)Math.Round(ListaDeAvaliacoes.Average(r => r.Avaliacao));
            //}
            //return 0;
        }; 
    }
    

    // ========== FOREIGN KEY ==========

    /// <summary>
    /// ID do vendedor da loja 
    /// </summary>
    [Required]
    public string VendedorId { get; set; } = string.Empty;


    // ========== Conexões de loja ==========

    /// <summary>
    /// O vendedor dono desta loja
    /// Com esta propriedade, você pode fazer: loja.Vendor.Email
    /// </summary>
    public ApplicationUser Vendor { get; set; } = null!;



    /// <summary>
    /// Lista de produtos desta loja
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
    
    

    /// <summary>
    /// Avaliações recebidas por esta loja
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();



    // ========== PROPRIEDADES CALCULADAS ==========
    // Não vão para o banco de dados, são calculadas em tempo real
    
    /// <summary>
    /// Avaliação média da loja (0 a 5)
    /// Calcula automaticamente baseado nas reviews
    /// [NotMapped] significa que não cria coluna no banco
    /// </summary>
    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (Reviews == null || Reviews.Count == 0)
                return 0; // FAZER ALTERAÇÃO NA LÓGICA
            
            return Math.Round(Reviews.Average(r => r.Rating), 1);
        }
    }



    // <summary>
    /// Total de avaliações recebidas
    /// </summary>
    [NotMapped]
    public int TotalReviews => Reviews?.Count ?? 0;

}

/// <summary>
/// Enum para status da loja
/// </summary>
public enum StoreStatus
{
    Active = 1,      // Loja ativa e funcionando
    Inactive = 2,    // Desativada temporariamente pelo vendedor
    Suspended = 3    // Suspensa por violação de políticas (ação do admin)
}