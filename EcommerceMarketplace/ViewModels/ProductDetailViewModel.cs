using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para a página de detalhes do produto.
/// Contém todas as informações necessárias para exibir o produto completo,
/// incluindo dados da loja e avaliações.
/// </summary>
 
public class ProductDetailViewModel
{
    // ===== INFORMAÇÕES BÁSICAS DO PRODUTO =====
    
    /// <summary>
    /// ID do produto - necessário para adicionar ao carrinho
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do produto
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição completa do produto
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Preço atual do produto
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Quantidade disponível em estoque
    /// </summary>
    public int Stock { get; set; }
    
    
    /// <summary>
    /// URL da imagem principal do produto
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Status atual do produto (Available, OutOfStock, etc)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    // ===== INFORMAÇÕES DA CATEGORIA =====
    
    /// <summary>
    /// Nome da categoria do produto
    /// </summary>
    public string? CategoryName { get; set; }
    
    // ===== INFORMAÇÕES DA LOJA =====
    
    /// <summary>
    /// ID da loja - útil para links
    /// </summary>
    public int StoreId { get; set; }
    
    /// <summary>
    /// Nome da loja vendedora
    /// </summary>
    public string StoreName { get; set; } = string.Empty;
    
    /// <summary>
    /// Logo da loja (se disponível)
    /// </summary>
    public string? StoreLogoUrl { get; set; }
    
    /// <summary>
    /// Avaliação média da loja (1-5 estrelas)
    /// </summary>
    public double StoreAverageRating { get; set; }
    
    /// <summary>
    /// Total de avaliações que a loja recebeu
    /// </summary>
    public int StoreTotalReviews { get; set; }
    
    // ===== AVALIAÇÕES DO PRODUTO =====
    
    /// <summary>
    /// Avaliação média deste produto (1-5 estrelas)
    /// </summary>
    public double AverageRating { get; set; }
    
    /// <summary>
    /// Total de avaliações que este produto recebeu
    /// </summary>
    public int TotalReviews { get; set; }
    
    /// <summary>
    /// Lista das avaliações mais recentes do produto
    /// Limitamos a 5 para não sobrecarregar a página
    /// </summary>
    public List<ReviewProduct> RecentReviews { get; set; } = new List<ReviewProduct>();
    
    // ===== PROPRIEDADES CALCULADAS =====
    
    /// <summary>
    /// Indica se o produto está disponível para compra
    /// Um produto está disponível se tiver estoque e status Available
    /// </summary>
    public bool IsAvailable => Stock > 0 && Status == "Available";
    
    /// <summary>
    /// Mensagem de status do estoque para exibir ao usuário
    /// </summary>
    public string StockMessage
    {
        get
        {
            if (Stock == 0)
                return "Produto esgotado";
            else if (Stock <= 5)
                return $"Apenas {Stock} unidades restantes!";
            else if (Stock <= 10)
                return $"{Stock} unidades disponíveis";
            else
                return "Em estoque";
        }
    }
    
    /// <summary>
    /// Classe CSS para o badge de status do estoque
    /// </summary>
    public string StockBadgeClass
    {
        get
        {
            if (Stock == 0)
                return "bg-danger";
            else if (Stock <= 5)
                return "bg-warning";
            else
                return "bg-success";
        }
    }
} 
