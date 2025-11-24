using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.ViewModels;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller responsável por todas as operações do carrinho de compras.
/// Apenas usuários autenticados podem acessar.
/// </summary>
[Authorize] // Apenas usuários logados podem usar o carrinho
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CartController> _logger;
    
    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CartController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }
    
    /// <summary>
    /// Exibe a página do carrinho de compras
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateUserCartAsync();
        
        // Buscar o carrinho completo com todos os relacionamentos
        var cartWithItems = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Store)
            .FirstOrDefaultAsync(c => c.Id == cart.Id);
        
        if (cartWithItems == null)
        {
            return View(new CartViewModel());
        }
        
        // Montar o ViewModel
        var viewModel = new CartViewModel
        {
            CartId = cartWithItems.Id,
            Items = cartWithItems.CartItems.Select(ci => new CartItemViewModel
            {
                CartItemId = ci.Id,
                ProductId = ci.Product.Id,
                ProductName = ci.Product.Name,
                ProductImageUrl = ci.Product.ImageUrl,
                UnitPrice = ci.UnitPrice,
                Quantity = ci.Quantity,
                AvailableStock = ci.Product.Stock,
                StoreName = ci.Product.Store.Name,
                StoreId = ci.Product.Store.Id
            }).ToList()
        };
        
        return View(viewModel);
    }
    
    /// <summary>
    /// Adiciona um produto ao carrinho
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        try
        {
            // Validação básica
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Quantidade inválida";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            
            // Buscar o produto
            var product = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
            {
                TempData["ErrorMessage"] = "Produto não encontrado";
                return RedirectToAction("Index", "Home");
            }
            
            // Verificar se o produto está disponível
            if (product.Status != Enums.ProductStatus.Available)
            {
                TempData["ErrorMessage"] = "Este produto não está disponível no momento";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            
            // VERIFICAÇÃO CRUCIAL: Estoque suficiente?
            if (product.Stock < quantity)
            {
                TempData["ErrorMessage"] = $"Estoque insuficiente. Apenas {product.Stock} unidades disponíveis";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            
            // Buscar ou criar carrinho
            var cart = await GetOrCreateUserCartAsync();
            
            // Carregar os itens do carrinho
            await _context.Entry(cart)
                .Collection(c => c.CartItems)
                .LoadAsync();
            
            // Verificar se o produto já está no carrinho
            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);
            
            if (existingItem != null)
            {
                // Produto já está no carrinho - atualizar quantidade
                var newQuantity = existingItem.Quantity + quantity;
                
                // Verificar se a nova quantidade não excede o estoque
                if (newQuantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Não é possível adicionar mais unidades. Estoque máximo: {product.Stock}";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }
                
                existingItem.Quantity = newQuantity;
                _logger.LogInformation($"Quantidade atualizada no carrinho. Produto: {product.Name}, Nova quantidade: {newQuantity}");
            }
            else
            {
                // Adicionar novo item ao carrinho
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price, // Salvar o preço atual
                    AddedAt = DateTime.UtcNow
                };
                
                cart.CartItems.Add(cartItem);
                _logger.LogInformation($"Produto adicionado ao carrinho. Produto: {product.Name}, Quantidade: {quantity}");
            }
            
            // Atualizar o timestamp do carrinho
            cart.UpdatedAt = DateTime.UtcNow;
            
            // Salvar no banco
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"{product.Name} adicionado ao carrinho!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar produto ao carrinho");
            TempData["ErrorMessage"] = "Erro ao adicionar produto ao carrinho. Tente novamente.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
    
    /// <summary>
    /// Atualiza a quantidade de um item no carrinho
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Quantidade inválida" });
            }
            
            // Buscar o item do carrinho com o produto
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Item não encontrado" });
            }
            
            // Verificar se o carrinho pertence ao usuário atual
            var userId = _userManager.GetUserId(User);
            if (cartItem.Cart.CustomerId != userId)
            {
                return Json(new { success = false, message = "Acesso negado" });
            }
            
            // Verificar estoque
            if (quantity > cartItem.Product.Stock)
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Estoque insuficiente. Máximo: {cartItem.Product.Stock} unidades" 
                });
            }
            
            // Atualizar quantidade
            cartItem.Quantity = quantity;
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Quantidade atualizada. Item: {cartItem.Id}, Nova quantidade: {quantity}");
            
            return Json(new 
            { 
                success = true, 
                message = "Quantidade atualizada",
                newSubtotal = (cartItem.Quantity * cartItem.UnitPrice).ToString("C", new System.Globalization.CultureInfo("pt-BR"))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar quantidade");
            return Json(new { success = false, message = "Erro ao atualizar quantidade" });
        }
    }
    
    /// <summary>
    /// Remove um item do carrinho
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        try
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
            
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Item não encontrado";
                return RedirectToAction("Index");
            }
            
            // Verificar se o carrinho pertence ao usuário atual
            var userId = _userManager.GetUserId(User);
            if (cartItem.Cart.CustomerId != userId)
            {
                TempData["ErrorMessage"] = "Acesso negado";
                return RedirectToAction("Index");
            }
            
            var productName = cartItem.Product.Name;
            
            // Remover o item
            _context.CartItems.Remove(cartItem);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Item removido do carrinho. Produto: {productName}");
            
            TempData["SuccessMessage"] = $"{productName} removido do carrinho";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item do carrinho");
            TempData["ErrorMessage"] = "Erro ao remover item. Tente novamente.";
            return RedirectToAction("Index");
        }
    }
    
    /// <summary>
    /// Limpa todo o carrinho
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        try
        {
            var cart = await GetOrCreateUserCartAsync();
            
            // Carregar os itens
            await _context.Entry(cart)
                .Collection(c => c.CartItems)
                .LoadAsync();
            
            // Remover todos os itens
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Carrinho limpo");
            
            TempData["SuccessMessage"] = "Carrinho limpo com sucesso";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar carrinho");
            TempData["ErrorMessage"] = "Erro ao limpar carrinho. Tente novamente.";
            return RedirectToAction("Index");
        }
    }
    
    /// <summary>
    /// Busca ou cria o carrinho do usuário atual.
    /// Cada usuário tem apenas um carrinho.
    /// </summary>
    private async Task<Cart> GetOrCreateUserCartAsync()
    {
        var userId = _userManager.GetUserId(User);
        
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Usuário não autenticado");
        }
        
        // Buscar carrinho existente
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.CustomerId == userId);
        
        // Se não existir, criar um novo
        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Novo carrinho criado para usuário {userId}");
        }
        
        return cart;
    }
}