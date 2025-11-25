using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.ViewModels;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// Controller responsável por todas as funcionalidades do vendedor.
/// Por enquanto, tem apenas o Dashboard, mas no futuro terá:
/// - Gerenciamento de lojas
/// - Gerenciamento de produtos
/// - Visualização de pedidos
/// - Relatórios e análises
/// </summary>
[Authorize(Roles = "Vendedor")] // IMPORTANTE: Apenas usuários com role "Vendedor" podem acessar
public class VendorController : Controller
{
    // ===== DEPENDÊNCIAS =====
    
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VendorController> _logger;
    
    /// <summary>
    /// Construtor que recebe as dependências via Dependency Injection.
    /// O ASP.NET Core automaticamente fornece essas instâncias quando o controller é criado.
    /// </summary>
    public VendorController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<VendorController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }
    
    // ===== ACTION: DASHBOARD DO VENDEDOR =====
    
    /// <summary>
    /// Exibe o dashboard principal do vendedor com todas as métricas e informações.
    /// 
    /// Esta action faz MUITAS queries no banco de dados, mas isso é normal para um dashboard.
    /// Em produção, você pode otimizar com cache (guardar os dados por alguns minutos),
    /// mas para desenvolvimento, está perfeito assim.
    /// 
    /// URL: /Vendor/Dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        // ===== ETAPA 1: IDENTIFICAR O VENDEDOR =====
        // Precisamos saber QUEM é o vendedor logado para buscar apenas os dados dele
        
        var vendorId = _userManager.GetUserId(User);
        
        if (string.IsNullOrEmpty(vendorId))
        {
            // Isso teoricamente nunca deveria acontecer porque temos [Authorize],
            // mas é sempre bom ter uma verificação de segurança
            _logger.LogWarning("Tentativa de acesso ao dashboard sem usuário autenticado");
            return RedirectToAction("Login", "Account");
        }
        
        _logger.LogInformation($"Vendedor {vendorId} acessando dashboard");
        
        // ===== ETAPA 2: BUSCAR TODAS AS LOJAS DO VENDEDOR =====
        // Um vendedor pode ter múltiplas lojas. Precisamos buscar todas elas.

        var vendorStores = await _context.Stores
            .Where(s => s.VendorId == vendorId)
            .ToListAsync();
        
        // Se o vendedor não tem nenhuma loja ainda, retornamos um dashboard vazio
        // A View vai detectar isso (HasStores = false) e mostrar uma mensagem apropriada
        if (!vendorStores.Any())
        {
            _logger.LogInformation($"Vendedor {vendorId} ainda não possui lojas cadastradas");
            return View(new VendorDashboardViewModel());
        }
        
        // ===== ETAPA 3: PREPARAR LISTA DE IDs DAS LOJAS =====
        // Vamos precisar destes IDs em várias queries, então vamos extraí-los uma vez
        
        var storeIds = vendorStores.Select(s => s.Id).ToList();
        
        _logger.LogInformation($"Vendedor {vendorId} possui {storeIds.Count} loja(s)");
        
        // ===== ETAPA 4: CALCULAR TOTAL DE PRODUTOS =====
        // Conta quantos produtos existem em TODAS as lojas deste vendedor
        
        var totalProducts = await _context.Products
            .Where(p => storeIds.Contains(p.StoreId))
            .CountAsync();
        
        _logger.LogInformation($"Total de produtos: {totalProducts}");
        
        // ===== ETAPA 5: BUSCAR TODOS OS ITENS VENDIDOS =====
        // Esta é a query mais importante. Precisamos buscar todos os OrderItems
        // cujos produtos pertencem às lojas deste vendedor.
        //
        // IMPORTANTE: Filtramos apenas pedidos ENTREGUES (completos).
        // Não contamos pedidos pendentes, cancelados ou reembolsados.

        var completedOrderItems = await _context.OrderItems
            .Include(oi => oi.Product)           // Precisamos do produto para saber de qual loja é
            .Include(oi => oi.Order)             // Precisamos do pedido para verificar status e data
            .Where(oi => storeIds.Contains(oi.Product.StoreId) &&   // Produto é de uma loja do vendedor
                        oi.Order.Status == OrderStatus.Delivered)    // Pedido está entregue
            .ToListAsync();
        
        _logger.LogInformation($"Total de itens vendidos (completados): {completedOrderItems.Count}");
        
        // ===== ETAPA 6: CALCULAR TOTAL DE VENDAS (NÚMERO DE PEDIDOS) =====
        // "Total de vendas" significa quantos PEDIDOS diferentes compraram produtos do vendedor.
        // Um pedido pode conter múltiplos produtos, mas conta como uma venda.
        // 
        // Usamos Distinct() porque se um pedido comprou 3 produtos diferentes do vendedor,
        // queremos contar como 1 venda, não 3.
        
        var totalSales = completedOrderItems
            .Select(oi => oi.OrderId)
            .Distinct()
            .Count();
        
        _logger.LogInformation($"Total de vendas (pedidos): {totalSales}");
        
        // ===== ETAPA 7: CALCULAR RECEITA TOTAL =====
        // Receita é a soma de: (quantidade × preço unitário) de cada item vendido.
        // Usamos o preço UNITÁRIO salvo no OrderItem, não o preço atual do produto,
        // porque o preço pode ter mudado desde a venda.
        
        var totalRevenue = completedOrderItems
            .Sum(oi => oi.Quantity * oi.UnitPrice);
        
        _logger.LogInformation($"Receita total: {totalRevenue:C}");
        
        // ===== ETAPA 8: CALCULAR MÉTRICAS POR LOJA =====
        // Agora precisamos calcular vendas e receita para CADA loja individualmente.
        // Isso vai popular os cards na seção "Minhas Lojas".
        
        var storeCards = new List<StoreCardViewModel>();
        
        foreach (var store in vendorStores)
        {
            // Filtrar apenas os OrderItems que pertencem a esta loja específica
            var storeOrderItems = completedOrderItems
                .Where(oi => oi.Product.StoreId == store.Id)
                .ToList();
            
            // Calcular vendas desta loja (número de pedidos únicos)
            var storeSales = storeOrderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();
            
            // Calcular receita desta loja
            var storeRevenue = storeOrderItems
                .Sum(oi => oi.Quantity * oi.UnitPrice);
            
            // Criar o ViewModel para esta loja
            storeCards.Add(new StoreCardViewModel
            {
                Id = store.Id,
                Name = store.Name,
                Description = store.Description,
                LogoUrl = store.LogoUrl,
                Status = store.Status.ToString(),
                Sales = storeSales,
                Revenue = storeRevenue
            });
            
            _logger.LogInformation($"Loja '{store.Name}': {storeSales} vendas, {storeRevenue:C} receita");
        }
        
        // ===== ETAPA 9: ENCONTRAR PRODUTO MAIS VENDIDO (ÚLTIMOS 3 MESES) =====
        // Vamos buscar o produto que vendeu MAIS UNIDADES nos últimos 3 meses.
        // Não é o que faturou mais, é o que vendeu mais quantidade.
        
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        
        // Esta query é complexa. Vamos por partes:
        // 1. Filtra OrderItems dos últimos 3 meses
        // 2. Agrupa por produto (GroupBy)
        // 3. Para cada grupo, soma as quantidades vendidas
        // 4. Ordena do maior para o menor (mais vendido primeiro)
        // 5. Pega apenas o primeiro (FirstOrDefaultAsync)

        var topProduct = await _context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => storeIds.Contains(oi.Product.StoreId) &&          // Produto do vendedor
                        oi.Order.Status == OrderStatus.Delivered &&        // Pedido entregue
                        oi.Order.CreatedAt >= threeMonthsAgo)              // Últimos 3 meses
            .GroupBy(oi => new
            {
                oi.ProductId,
                oi.Product.Name,
                oi.Product.ImageUrl
            })
            .Select(g => new TopProductViewModel
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                UnitsSold = g.Sum(oi => oi.Quantity),                     // Soma total de unidades
                RevenueGenerated = g.Sum(oi => oi.Quantity * oi.UnitPrice) // Receita total
            })
            .OrderByDescending(p => p.UnitsSold)                            // Ordena por mais vendido
            .FirstOrDefaultAsync();                                         // Pega só o primeiro
        
        if (topProduct != null)
        {
            _logger.LogInformation($"Produto mais vendido: '{topProduct.ProductName}' ({topProduct.UnitsSold} unidades)");
        }
        else
        {
            _logger.LogInformation("Nenhuma venda nos últimos 3 meses");
        }
        
        // ===== ETAPA 10: MONTAR O VIEWMODEL FINAL =====
        // Agora que temos todos os dados, vamos montar o ViewModel que será enviado para a View
        
        var viewModel = new VendorDashboardViewModel
        {
            TotalProducts = totalProducts,
            TotalSales = totalSales,
            TotalRevenue = totalRevenue,
            Stores = storeCards,
            TopProduct = topProduct
        };
        
        _logger.LogInformation("Dashboard do vendedor carregado com sucesso");
        
        // ===== ETAPA 11: RETORNAR A VIEW =====
        // Retorna a View ~/Views/Vendor/Dashboard.cshtml com o ViewModel populado
        return View(viewModel);
    }

    // ===== ACTION: CRIAR NOVA LOJA (GET) =====
    
    /// <summary>
    /// Exibe o formulário para criar uma nova loja.
    /// Esta é a action que é chamada quando o vendedor clica em "Nova Loja".
    /// 
    /// Não precisa fazer nenhuma query ao banco aqui, apenas retorna
    /// a View com um ViewModel vazio para o vendedor preencher.
    /// 
    /// URL: /Vendor/CreateStore
    /// </summary>
    [HttpGet]
    public IActionResult CreateStore()
    {
        // Retorna a View com um ViewModel vazio
        // O ViewModel vazio significa que todos os campos do formulário estarão vazios
        return View(new CreateStoreViewModel());
    }
    
    // ===== ACTION: CRIAR NOVA LOJA (POST) =====
    
    /// <summary>
    /// Processa o formulário de criação de loja.
    /// Esta action é chamada quando o vendedor preenche o formulário e clica em "Criar Loja".
    /// 
    /// Fluxo:
    /// 1. Verifica se os dados são válidos (ModelState.IsValid)
    /// 2. Pega o ID do vendedor logado
    /// 3. Cria uma nova Store no banco
    /// 4. Redireciona para o Dashboard
    /// 
    /// URL: /Vendor/CreateStore (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken] // Proteção contra CSRF attacks
    public async Task<IActionResult> CreateStore(CreateStoreViewModel model)
    {
        // ===== VALIDAÇÃO =====
        // ModelState.IsValid verifica se todos os [Required], [StringLength], etc
        // foram respeitados. Se alguma validação falhar, retorna o formulário
        // com as mensagens de erro para o usuário corrigir.
        
        if (!ModelState.IsValid)
        {
            // Retorna a View com o modelo preenchido e os erros de validação
            // Assim o usuário vê o que digitou e quais campos têm problemas
            return View(model);
        }
        
        try
        {
            // ===== IDENTIFICAR O VENDEDOR =====
            // A loja será criada para o vendedor que está logado
            
            var vendorId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(vendorId))
            {
                _logger.LogError("Tentativa de criar loja sem usuário autenticado");
                ModelState.AddModelError(string.Empty, "Erro de autenticação. Faça login novamente.");
                return View(model);
            }
            
            // ===== CRIAR O ENDEREÇO =====
            // Primeiro, criamos o endereço no banco de dados.
            // Verificamos se já existe um endereço idêntico para reutilizá-lo.

            var existingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.ZipCode == model.ZipCode &&
                    a.Street == model.Street &&
                    a.Number == model.Number &&
                    (a.Complement == model.Complement || (a.Complement == null && model.Complement == null)) &&
                    a.Neighborhood == model.Neighborhood &&
                    a.City == model.City &&
                    a.State == model.State);

            Address address;

            if (existingAddress != null)
            {
                // Reutiliza o endereço existente
                address = existingAddress;
                _logger.LogInformation($"Reutilizando endereço existente (ID: {address.Id})");
            }
            else
            {
                // Cria um novo endereço
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
                await _context.SaveChangesAsync();  // Salva para obter o ID

                _logger.LogInformation($"Novo endereço criado (ID: {address.Id})");
            }

            // ===== CRIAR A ENTIDADE STORE =====
            // Agora criamos a loja associando-a ao endereço criado/reutilizado

            var store = new Store
            {
                // Dados básicos
                Name = model.Name,
                Description = model.Description,

                // Contato
                Phone = model.Phone,
                ContactEmail = model.ContactEmail,

                // Associar ao endereço
                AddressId = address.Id,

                // Associar ao vendedor logado
                VendorId = vendorId,

                // Valores padrão/iniciais
                Status = StoreStatus.Active,          // Loja começa ativa
                CreatedAt = DateTime.UtcNow,          // Data/hora da criação
                UpdatedAt = DateTime.UtcNow           // Mesma data/hora inicialmente

                // NOTA: AverageRating e TotalReviews são propriedades CALCULADAS automaticamente
                // com base nas reviews. Não podem ser atribuídas manualmente.
            };

            // ===== SALVAR NO BANCO =====

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Nova loja criada: '{store.Name}' (ID: {store.Id}) pelo vendedor {vendorId}");
            
            // ===== MENSAGEM DE SUCESSO =====
            // TempData mantém uma mensagem por uma requisição
            // Vamos usar isso para mostrar uma mensagem de sucesso no Dashboard
            
            TempData["SuccessMessage"] = $"Loja '{store.Name}' criada com sucesso!";
            
            // ===== REDIRECIONAR =====
            // Após criar a loja, levamos o vendedor de volta ao Dashboard
            // onde ele poderá ver a nova loja listada
            
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            // ===== TRATAMENTO DE ERROS =====
            // Se algo der errado (problema no banco, etc), logamos o erro
            // e mostramos uma mensagem amigável ao usuário
            
            _logger.LogError(ex, "Erro ao criar loja");
            ModelState.AddModelError(string.Empty, "Erro ao criar loja. Tente novamente.");
            return View(model);
        }
    }
}