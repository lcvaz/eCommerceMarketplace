using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.ViewComponents;

/// <summary>
/// ViewComponent que exibe o badge do carrinho com a contagem de itens.
/// É chamado automaticamente pelo layout para mostrar quantos itens há no carrinho.
/// </summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartBadgeViewComponent(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Método principal do ViewComponent.
    /// Busca a contagem de itens no carrinho do usuário atual.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Se o usuário não estiver autenticado, retornar 0
        // Em ViewComponents, usamos HttpContext.User que retorna ClaimsPrincipal
        if (!(HttpContext.User?.Identity?.IsAuthenticated ?? false))
        {
            return View(0);
        }

        try
        {
            // Obter o ID do usuário atual
            // HttpContext.User retorna ClaimsPrincipal, que é o tipo esperado pelo UserManager
            var userId = _userManager.GetUserId(HttpContext.User);

            if (string.IsNullOrEmpty(userId))
            {
                return View(0);
            }

            // Buscar o carrinho do usuário
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            // Se não tem carrinho ou o carrinho está vazio, retornar 0
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return View(0);
            }

            // Calcular o total de itens (soma das quantidades)
            var totalItems = cart.CartItems.Sum(ci => ci.Quantity);

            return View(totalItems);
        }
        catch (Exception)
        {
            // Em caso de erro, retornar 0 silenciosamente
            // (não queremos quebrar o layout por causa de um erro no badge do carrinho)
            return View(0);
        }
    }
}