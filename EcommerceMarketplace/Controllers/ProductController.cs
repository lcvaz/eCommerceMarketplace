using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller público para visualização de produtos pelos clientes
/// </summary>
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ========== CATÁLOGO PÚBLICO ==========

    /// <summary>
    /// Catálogo público de todos os produtos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? category, string? search, decimal? minPrice, decimal? maxPrice)
    {
        // Query base - apenas produtos disponíveis
        var query = _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Where(p => p.Status == EcommerceMarketplace.Enums.ProductStatus.Available && p.Stock > 0);

        // Filtro por categoria
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Name == category);
        }

        // Filtro por busca
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) ||
                                     (p.Description != null && p.Description.Contains(search)));
        }

        // Filtro por preço mínimo
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        // Filtro por preço máximo
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Buscar todas as categorias para o filtro
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentSearch = search;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        return View(products);
    }

    // ========== DETALHES DO PRODUTO ==========

    /// <summary>
    /// Página de detalhes de um produto específico
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Store)
                .ThenInclude(s => s.Vendor)
            .Include(p => p.Category)
            .Include(p => p.ReviewsProduct)
                .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // Buscar produtos relacionados (mesma categoria)
        var relatedProducts = await _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == product.CategoryId &&
                        p.Id != product.Id &&
                        p.Status == EcommerceMarketplace.Enums.ProductStatus.Available &&
                        p.Stock > 0)
            .Take(4)
            .ToListAsync();

        // Calcular média de avaliações da loja
        var storeReviews = await _context.ReviewsStore
            .Where(r => r.StoreId == product.StoreId)
            .ToListAsync();

        ViewBag.RelatedProducts = relatedProducts;
        ViewBag.StoreAverageRating = storeReviews.Any() ? storeReviews.Average(r => r.Rating) : 0;
        ViewBag.StoreTotalReviews = storeReviews.Count;

        return View(product);
    }

    // ========== BUSCA ==========

    /// <summary>
    /// Busca de produtos (usado pelo campo de busca no header)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrEmpty(q))
        {
            return RedirectToAction(nameof(Index));
        }

        var products = await _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Where(p => (p.Name.Contains(q) ||
                        (p.Description != null && p.Description.Contains(q))) &&
                        p.Status == EcommerceMarketplace.Enums.ProductStatus.Available &&
                        p.Stock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.SearchQuery = q;
        ViewBag.ResultCount = products.Count;

        return View("SearchResults", products);
    }

    // ========== PRODUTOS POR CATEGORIA ==========

    /// <summary>
    /// Lista produtos de uma categoria específica
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Category(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        var products = await _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Where(p => p.CategoryId == id &&
                        p.Status == EcommerceMarketplace.Enums.ProductStatus.Available &&
                        p.Stock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewBag.CategoryName = category.Name;
        ViewBag.CategoryDescription = category.Description;

        return View("CategoryProducts", products);
    }
}
