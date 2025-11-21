using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller para funcionalidades exclusivas do Admin
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ========== DASHBOARD ==========

    /// <summary>
    /// Dashboard principal do admin com estatísticas gerais
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Estatísticas gerais do sistema
        var totalUsers = await _userManager.Users.CountAsync();
        var totalStores = await _context.Stores.CountAsync();
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == EcommerceMarketplace.Enums.OrderStatus.Entregue)
            .SumAsync(o => o.Total);

        // Usuários por role
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var vendors = await _userManager.GetUsersInRoleAsync("Vendedor");
        var clients = await _userManager.GetUsersInRoleAsync("Cliente");

        // Pedidos recentes
        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalStores = totalStores;
        ViewBag.TotalProducts = totalProducts;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalAdmins = admins.Count;
        ViewBag.TotalVendors = vendors.Count;
        ViewBag.TotalClients = clients.Count;
        ViewBag.RecentOrders = recentOrders;

        return View();
    }

    // ========== GERENCIAMENTO DE USUÁRIOS ==========

    /// <summary>
    /// Lista todos os usuários do sistema
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var usersWithRoles = new List<(ApplicationUser User, string Role)>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Sem Role";
            usersWithRoles.Add((user, role));
        }

        return View(usersWithRoles);
    }

    /// <summary>
    /// Detalhes de um usuário
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var stores = await _context.Stores.Where(s => s.UserId == id).ToListAsync();
        var orders = await _context.Orders.Where(o => o.UserId == id).ToListAsync();

        ViewBag.Roles = roles;
        ViewBag.Stores = stores;
        ViewBag.Orders = orders;

        return View(user);
    }

    /// <summary>
    /// Desativa/ativa usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Usar LockoutEnd para desativar/ativar usuário
        if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
        {
            // Desativar usuário (bloquear por 100 anos)
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            TempData["Success"] = $"Usuário {user.Email} foi desativado.";
        }
        else
        {
            // Ativar usuário
            user.LockoutEnd = null;
            TempData["Success"] = $"Usuário {user.Email} foi ativado.";
        }

        await _userManager.UpdateAsync(user);

        return RedirectToAction(nameof(Users));
    }

    /// <summary>
    /// Altera a role de um usuário
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // Remover roles antigas
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Adicionar nova role
        await _userManager.AddToRoleAsync(user, newRole);

        TempData["Success"] = $"Role do usuário {user.Email} alterada para {newRole}.";

        return RedirectToAction(nameof(UserDetails), new { id = userId });
    }

    // ========== GERENCIAMENTO DE LOJAS ==========

    /// <summary>
    /// Lista todas as lojas do sistema
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Stores()
    {
        var stores = await _context.Stores
            .Include(s => s.User)
            .Include(s => s.Products)
            .ToListAsync();

        return View(stores);
    }

    /// <summary>
    /// Detalhes de uma loja
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> StoreDetails(int id)
    {
        var store = await _context.Stores
            .Include(s => s.User)
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null)
        {
            return NotFound();
        }

        return View(store);
    }

    /// <summary>
    /// Ativa/desativa loja
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStoreStatus(int id)
    {
        var store = await _context.Stores.FindAsync(id);

        if (store == null)
        {
            return NotFound();
        }

        store.IsActive = !store.IsActive;
        await _context.SaveChangesAsync();

        var status = store.IsActive ? "ativada" : "desativada";
        TempData["Success"] = $"Loja {store.Name} foi {status}.";

        return RedirectToAction(nameof(Stores));
    }

    /// <summary>
    /// Deleta loja (forçado pelo admin)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStore(int id)
    {
        var store = await _context.Stores
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null)
        {
            return NotFound();
        }

        // Admin pode deletar mesmo com produtos
        _context.Stores.Remove(store);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Loja {store.Name} foi excluída permanentemente.";
        return RedirectToAction(nameof(Stores));
    }

    // ========== GERENCIAMENTO DE PRODUTOS ==========

    /// <summary>
    /// Lista todos os produtos do sistema
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var products = await _context.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .ToListAsync();

        return View(products);
    }

    /// <summary>
    /// Ativa/desativa produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductStatus(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();

        var status = product.IsActive ? "ativado" : "desativado";
        TempData["Success"] = $"Produto {product.Name} foi {status}.";

        return RedirectToAction(nameof(Products));
    }

    /// <summary>
    /// Deleta produto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Produto {product.Name} foi excluído.";
        return RedirectToAction(nameof(Products));
    }

    // ========== GERENCIAMENTO DE CATEGORIAS ==========

    /// <summary>
    /// Lista todas as categorias
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Categories()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();

        return View(categories);
    }

    /// <summary>
    /// Criar nova categoria
    /// </summary>
    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }

    /// <summary>
    /// Processa criação de categoria
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoria criada com sucesso!";
            return RedirectToAction(nameof(Categories));
        }

        return View(category);
    }

    /// <summary>
    /// Editar categoria
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    /// <summary>
    /// Processa edição de categoria
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoria atualizada com sucesso!";
            return RedirectToAction(nameof(Categories));
        }

        return View(category);
    }

    /// <summary>
    /// Deleta categoria (se não tiver produtos)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            TempData["Error"] = "Não é possível excluir uma categoria com produtos.";
            return RedirectToAction(nameof(Categories));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Categoria excluída com sucesso!";
        return RedirectToAction(nameof(Categories));
    }
}
