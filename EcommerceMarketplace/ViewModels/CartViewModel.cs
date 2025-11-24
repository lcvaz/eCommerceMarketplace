using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para a página do carrinho de compras.
/// Contém o carrinho completo e cálculos de totais.
/// </summary>
public class CartViewModel
{
    /// <summary>
    /// ID do carrinho
    /// </summary>
    public int CartId { get; set; }
    
    /// <summary>
    /// Lista de itens no carrinho
    /// </summary>
    public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    
    /// <summary>
    /// Total de itens (soma das quantidades)
    /// </summary>
    public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
    
    /// <summary>
    /// Subtotal (soma de todos os produtos)
    /// </summary>
    public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;
    
    /// <summary>
    /// Valor do frete (será calculado no checkout)
    /// Por enquanto, apenas um valor fixo como exemplo
    /// </summary>
    public decimal ShippingCost => 0; // Frete grátis por enquanto
    
    /// <summary>
    /// Desconto aplicado (futura implementação de cupons)
    /// </summary>
    public decimal Discount => 0;
    
    /// <summary>
    /// Total final a pagar
    /// </summary>
    public decimal Total => Subtotal + ShippingCost - Discount;
    
    /// <summary>
    /// Se o carrinho está vazio
    /// </summary>
    public bool IsEmpty => Items == null || !Items.Any();
}

/// <summary>
/// ViewModel para cada item individual no carrinho
/// </summary>
public class CartItemViewModel
{
    /// <summary>
    /// ID do item no carrinho
    /// </summary>
    public int CartItemId { get; set; }
    
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Nome do produto
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL da imagem do produto
    /// </summary>
    public string ProductImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Preço unitário (preço no momento que foi adicionado ao carrinho)
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Quantidade no carrinho
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Estoque disponível do produto (para limitar quantidade)
    /// </summary>
    public int AvailableStock { get; set; }
    
    /// <summary>
    /// Nome da loja vendedora
    /// </summary>
    public string StoreName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID da loja (útil para agrupar por loja se necessário)
    /// </summary>
    public int StoreId { get; set; }
    
    /// <summary>
    /// Subtotal deste item (Quantidade × Preço)
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;
    
    /// <summary>
    /// Se há estoque suficiente para a quantidade no carrinho
    /// </summary>
    public bool HasSufficientStock => AvailableStock >= Quantity;
    
    /// <summary>
    /// Mensagem de aviso sobre estoque
    /// </summary>
    public string? StockWarning
    {
        get
        {
            if (AvailableStock == 0)
                return "Produto esgotado";
            else if (Quantity > AvailableStock)
                return $"Apenas {AvailableStock} unidades disponíveis";
            else if (AvailableStock <= 5)
                return "Estoque baixo";
            else
                return null;
        }
    }
}