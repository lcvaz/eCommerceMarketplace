using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para a página inicial (Home)
/// </summary>
public class HomeViewModel
{
    /// <summary>
    /// Lista de categorias principais para exibir
    /// </summary>
    public List<Category> FeaturedCategories { get; set; } = new List<Category>();

    /// <summary>
    /// Lista de produtos em destaque
    /// </summary>
    public List<Product> FeaturedProducts { get; set; } = new List<Product>();
}