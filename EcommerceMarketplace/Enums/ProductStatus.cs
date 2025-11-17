namespace EcommerceMarketplace.Enums;

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