using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para a página de gerenciamento de produtos de uma loja.
/// Contém informações da loja e lista de produtos.
/// </summary>
public class ManageProductsViewModel
{
    // ===== INFORMAÇÕES DA LOJA =====

    /// <summary>
    /// ID da loja sendo gerenciada
    /// </summary>
    public int StoreId { get; set; }

    /// <summary>
    /// Nome da loja
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da loja
    /// </summary>
    public string? StoreDescription { get; set; }

    /// <summary>
    /// URL do logo da loja
    /// </summary>
    public string? StoreLogoUrl { get; set; }

    // ===== LISTA DE PRODUTOS =====

    /// <summary>
    /// Lista de produtos da loja
    /// </summary>
    public List<Product> Products { get; set; } = new List<Product>();

    // ===== PROPRIEDADES CALCULADAS =====

    /// <summary>
    /// Verifica se a loja tem produtos
    /// </summary>
    public bool HasProducts => Products != null && Products.Any();

    /// <summary>
    /// Total de produtos na loja
    /// </summary>
    public int TotalProducts => Products?.Count ?? 0;
}
