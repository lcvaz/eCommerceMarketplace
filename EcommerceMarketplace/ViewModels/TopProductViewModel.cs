namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para exibir o produto mais vendido.
/// </summary>
public class TopProductViewModel
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Nome do produto
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Imagem do produto
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantidade de unidades vendidas (nos últimos 3 meses)
    /// </summary>
    public int UnitsSold { get; set; }
    
    /// <summary>
    /// Receita gerada por este produto (nos últimos 3 meses)
    /// </summary>
    public decimal RevenueGenerated { get; set; }
}