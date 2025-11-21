using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller para funcionalidades exclusivas do Vendedor
/// </summary>
[Authorize(Roles = "Vendedor")]
public class VendorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VendorController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ========== DASHBOARD ==========

    /// <summary>
    /// Dashboard principal do vendedor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Buscar estatísticas do vendedor
        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id)
            .Include(s => s.Products)
            .ToListAsync();

        var totalStores = stores.Count;
        var totalProducts = stores.Sum(s => s.Products.Count);

        // Buscar pedidos dos produtos do vendedor
        var productIds = stores.SelectMany(s => s.Products.Select(p => p.Id)).ToList();
        var orders = await _context.OrderItems
            .Where(oi => productIds.Contains(oi.ProductId))
            .Include(oi => oi.Order)
            .ToListAsync();

        var totalOrders = orders.Select(oi => oi.OrderId).Distinct().Count();
        var totalRevenue = orders.Sum(oi => oi.Subtotal);

        ViewBag.TotalStores = totalStores;
        ViewBag.TotalProducts = totalProducts;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.Stores = stores;

        return View();
    }

    // ========== GERENCIAMENTO DE LOJAS ==========

    /// <summary>
    /// Lista todas as lojas do vendedor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyStores()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id)
            .Include(s => s.Products)
            .ToListAsync();

        return View(stores);
    }

    /// <summary>
    /// Formulário para criar nova loja
    /// </summary>
    [HttpGet]
    public IActionResult CreateStore()
    {
        return View();
    }

    /// <summary>
    /// Processa criação de nova loja
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStore(Store store)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            store.UserId = user.Id;
            store.CreatedAt = DateTime.UtcNow;
            store.IsActive = true;

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Loja criada com sucesso!";
            return RedirectToAction(nameof(MyStores));
        }

        return View(store);
    }

    /// <summary>
    /// Formulário para editar loja
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditStore(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

        if (store == null)
        {
            return NotFound();
        }

        return View(store);
    }

    /// <summary>
    /// Processa edição de loja
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStore(int id, Store store)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (id != store.Id)
        {
            return NotFound();
        }

        var existingStore = await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

        if (existingStore == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            existingStore.Name = store.Name;
            existingStore.Description = store.Description;
            existingStore.CNPJ = store.CNPJ;
            existingStore.IsActive = store.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Loja atualizada com sucesso!";
            return RedirectToAction(nameof(MyStores));
        }

        return View(store);
    }

    /// <summary>
    /// Deleta loja (se não tiver produtos)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStore(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var store = await _context.Stores
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

        if (store == null)
        {
            return NotFound();
        }

        // Verificar se a loja tem produtos
        if (store.Products.Any())
        {
            TempData["Error"] = "Não é possível excluir uma loja com produtos. Exclua os produtos primeiro.";
            return RedirectToAction(nameof(MyStores));
        }

        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Loja excluída com sucesso!";
        return RedirectToAction(nameof(MyStores));
    }

    // ========== GERENCIAMENTO DE PRODUTOS ==========

    /// <summary>
    /// Lista todos os produtos do vendedor
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyProducts()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var products = await _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Where(p => p.Store.UserId == user.Id)
            .ToListAsync();

        return View(products);
    }

    /// <summary>
    /// Formulário para criar novo produto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Buscar lojas do vendedor
        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();

        if (!stores.Any())
        {
            TempData["Error"] = "Você precisa criar uma loja antes de adicionar produtos.";
            return RedirectToAction(nameof(CreateStore));
        }

        // Buscar categorias
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Stores = stores;
        ViewBag.Categories = categories;

        return View();
    }

    /// <summary>
    /// Processa criação de novo produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Verificar se a loja pertence ao vendedor
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == product.StoreId && s.UserId == user.Id);

        if (store == null)
        {
            ModelState.AddModelError("StoreId", "Loja inválida.");
        }

        if (ModelState.IsValid)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.IsActive = true;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Produto criado com sucesso!";
            return RedirectToAction(nameof(MyProducts));
        }

        // Recarregar dados para o formulário
        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Stores = stores;
        ViewBag.Categories = categories;

        return View(product);
    }

    /// <summary>
    /// Formulário para editar produto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var product = await _context.Products
            .Include(p => p.Store)
            .FirstOrDefaultAsync(p => p.Id == id && p.Store.UserId == user.Id);

        if (product == null)
        {
            return NotFound();
        }

        // Buscar lojas e categorias
        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Stores = stores;
        ViewBag.Categories = categories;

        return View(product);
    }

    /// <summary>
    /// Processa edição de produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, Product product)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (id != product.Id)
        {
            return NotFound();
        }

        var existingProduct = await _context.Products
            .Include(p => p.Store)
            .FirstOrDefaultAsync(p => p.Id == id && p.Store.UserId == user.Id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // Verificar se a loja pertence ao vendedor
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == product.StoreId && s.UserId == user.Id);

        if (store == null)
        {
            ModelState.AddModelError("StoreId", "Loja inválida.");
        }

        if (ModelState.IsValid)
        {
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.StoreId = product.StoreId;
            existingProduct.IsActive = product.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Produto atualizado com sucesso!";
            return RedirectToAction(nameof(MyProducts));
        }

        // Recarregar dados
        var stores = await _context.Stores
            .Where(s => s.UserId == user.Id && s.IsActive)
            .ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        ViewBag.Stores = stores;
        ViewBag.Categories = categories;

        return View(product);
    }

    /// <summary>
    /// Deleta produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var product = await _context.Products
            .Include(p => p.Store)
            .FirstOrDefaultAsync(p => p.Id == id && p.Store.UserId == user.Id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Produto excluído com sucesso!";
        return RedirectToAction(nameof(MyProducts));
    }
}
