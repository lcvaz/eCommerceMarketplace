using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.ViewModels;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Checkout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Buscar carrinho do usuário com itens e produtos
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == userId);

                // Verificar se carrinho existe e tem itens
                if (cart == null || cart.IsEmpty)
                {
                    TempData["ErrorMessage"] = "Seu carrinho está vazio.";
                    return RedirectToAction("Index", "Cart");
                }

                // Buscar dados do usuário
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                // Criar ViewModel e preencher com dados do usuário
                var viewModel = new CheckoutViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    CPF = user.CPF ?? "",
                    Phone = user.PhoneNumber ?? "",

                    // Inicializar valores padrão
                    PaymentMethod = "CreditCard",
                    State = "SP",

                    // Calcular valores
                    Items = cart.CartItems.Select(ci => new CheckoutItemViewModel
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        ProductImageUrl = ci.Product.ImageUrl ?? "/images/placeholder.jpg",
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice
                    }).ToList(),

                    Subtotal = cart.TotalAmount,
                    ShippingCost = 0, // Frete grátis por enquanto
                    Discount = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de checkout");
                TempData["ErrorMessage"] = "Erro ao carregar página de checkout. Tente novamente.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // POST: Checkout/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            try
            {
                // Validar dados do cartão se método for cartão de crédito
                if (model.PaymentMethod == "CreditCard")
                {
                    if (string.IsNullOrEmpty(model.CardNumber) ||
                        string.IsNullOrEmpty(model.CardHolderName) ||
                        string.IsNullOrEmpty(model.CardExpiry) ||
                        string.IsNullOrEmpty(model.CardCVV))
                    {
                        ModelState.AddModelError("", "Todos os dados do cartão são obrigatórios.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Recarregar itens do carrinho para exibir na view
                    var userId = _userManager.GetUserId(User);
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Product)
                        .FirstOrDefaultAsync(c => c.CustomerId == userId);

                    if (cart != null)
                    {
                        model.Items = cart.CartItems.Select(ci => new CheckoutItemViewModel
                        {
                            ProductId = ci.ProductId,
                            ProductName = ci.Product.Name,
                            ProductImageUrl = ci.Product.ImageUrl ?? "/images/placeholder.jpg",
                            Quantity = ci.Quantity,
                            UnitPrice = ci.UnitPrice
                        }).ToList();

                        model.Subtotal = cart.TotalAmount;
                    }

                    return View("Index", model);
                }

                var currentUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Buscar carrinho do usuário novamente
                var userCart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == currentUserId);

                if (userCart == null || userCart.IsEmpty)
                {
                    TempData["ErrorMessage"] = "Seu carrinho está vazio.";
                    return RedirectToAction("Index", "Cart");
                }

                // Verificar estoque de todos os produtos
                foreach (var item in userCart.CartItems)
                {
                    if (item.Product.Stock < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Produto '{item.Product.Name}' sem estoque suficiente.";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                // Criar ou encontrar endereço
                var address = await _context.Addresses.FirstOrDefaultAsync(a =>
                    a.ZipCode == model.ZipCode &&
                    a.Street == model.Street &&
                    a.Number == model.Number &&
                    a.City == model.City &&
                    a.State == model.State
                );

                if (address == null)
                {
                    address = new Address
                    {
                        ZipCode = model.ZipCode,
                        Street = model.Street,
                        Number = model.Number,
                        Complement = model.Complement,
                        Neighborhood = model.Neighborhood,
                        City = model.City,
                        State = model.State,
                        Country = "Brasil",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();
                }

                // Criar associação do endereço com o cliente se solicitado
                if (model.SaveAddress)
                {
                    var existingCustomerAddress = await _context.CustomerAddresses
                        .FirstOrDefaultAsync(ca =>
                            ca.CustomerId == currentUserId &&
                            ca.AddressId == address.Id);

                    if (existingCustomerAddress == null)
                    {
                        var customerAddress = new CustomerAddress
                        {
                            CustomerId = currentUserId,
                            AddressId = address.Id,
                            IsDefault = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CustomerAddresses.Add(customerAddress);
                    }
                }

                // Gerar número único do pedido
                var orderNumber = await GenerateOrderNumberAsync();

                // Mapear método de pagamento para formato legível
                var paymentMethodDisplay = model.PaymentMethod switch
                {
                    "CreditCard" => "Cartão de Crédito",
                    "PIX" => "PIX",
                    "BankSlip" => "Boleto Bancário",
                    _ => model.PaymentMethod
                };

                // Criar pedido
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = currentUserId,
                    ShippingAddressId = address.Id,
                    SubtotalAmount = model.Subtotal,
                    ShippingAmount = model.ShippingCost,
                    DiscountAmount = model.Discount,
                    TotalAmount = model.Total,
                    PaymentMethod = paymentMethodDisplay,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Criar itens do pedido e atualizar estoque
                foreach (var cartItem in userCart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        DiscountAmount = 0
                    };
                    _context.OrderItems.Add(orderItem);

                    // Reduzir estoque do produto
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        product.Stock -= cartItem.Quantity;

                        // Atualizar status se estoque zerou
                        if (product.Stock <= 0)
                        {
                            product.Status = ProductStatus.OutOfStock;
                        }
                    }
                }

                // Limpar carrinho
                _context.CartItems.RemoveRange(userCart.CartItems);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pedido realizado com sucesso!";
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar checkout");
                TempData["ErrorMessage"] = "Erro ao processar seu pedido. Tente novamente.";
                return RedirectToAction("Index");
            }
        }

        // GET: Checkout/Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.ShippingAddress)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de confirmação");
                TempData["ErrorMessage"] = "Erro ao carregar confirmação do pedido.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Método auxiliar para gerar número único do pedido
        private async Task<string> GenerateOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith($"PED-{year}-"))
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var parts = lastOrder.OrderNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"PED-{year}-{nextNumber:D6}";
        }
    }
}
