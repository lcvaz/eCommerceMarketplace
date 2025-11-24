using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.ViewModels;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller responsável por todas as operações relacionadas a produtos
/// visíveis para clientes (visualização, busca, filtros)
/// </summary>
 
public class ProductController : Controller
{
    // ===== DEPENDÊNCIAS =====
    
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductController> _logger;
    
    /// <summary>
    /// Construtor que recebe as dependências via Dependency Injection
    /// </summary>
    public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===== ACTION: DETALHES DO PRODUTO =====
    
    /// <summary>
    /// Exibe a página de detalhes de um produto específico.
    /// 
    /// Fluxo:
    /// 1. Busca o produto no banco com todas as relações necessárias (Store, Category, Reviews)
    /// 2. Se não encontrar, retorna 404
    /// 3. Monta o ViewModel com todos os dados organizados
    /// 4. Retorna a view com o ViewModel populado
    /// </summary>
    /// <param name="id">ID do produto a ser exibido</param>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Busca o produto no banco de dados incluindo todas as relações que precisamos
        // Include() faz um JOIN nas tabelas relacionadas para trazer os dados juntos
        var product = await _context.Products
            .Include(p => p.Store)              // Traz informações da loja
            .Include(p => p.Category)            // Traz informações da categoria
            .Include(p => p.ReviewsProduct)      // Traz todas as avaliações do produto
                .ThenInclude(r => r.Customer)    // Para cada avaliação, traz dados do cliente que avaliou
            .FirstOrDefaultAsync(p => p.Id == id);
        
        // Se o produto não existir, retorna página 404
        if (product == null)
        {
            _logger.LogWarning($"Tentativa de acessar produto inexistente. ID: {id}");
            return NotFound();
        }
        
        // Monta o ViewModel com todos os dados que a view precisa
        // Estamos transformando o modelo do banco em um formato ideal para a view
        var viewModel = new ProductDetailViewModel
        {
            // Dados básicos do produto
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            // SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            Status = product.Status.ToString(),
            
            // Dados da categoria
            CategoryName = product.Category?.Name,
            
            // Dados da loja
            StoreName = product.Store.Name,
            StoreId = product.Store.Id,
            StoreLogoUrl = product.Store.LogoUrl,
            StoreAverageRating = product.Store.AverageRating,
            StoreTotalReviews = product.Store.TotalReviews,
            
            // Avaliações do produto
            AverageRating = product.AverageRating,
            TotalReviews = product.TotalReviews,
            
            // Pega as 5 avaliações mais recentes, ordenadas da mais nova para a mais antiga
            RecentReviews = product.ReviewsProduct
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList()
        };
        
        // Log para ajudar no debugging em desenvolvimento
        _logger.LogInformation($"Produto {product.Name} (ID: {id}) visualizado.");
        
        return View(viewModel);
    }
}    