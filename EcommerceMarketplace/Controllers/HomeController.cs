using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMarketplace.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController
    (
        ApplicationDbContext context,
        ILogger<HomeController> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel
        {
            // Busca 4 categorias principais (sem categoria pai)
            FeaturedCategories = await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .Take(4)
                .ToListAsync(),

            // Busca 8 produtos em destaque (disponíveis e com estoque)
            FeaturedProducts = await _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Where(p => p.Status == Enums.ProductStatus.Available && p.Stock > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
