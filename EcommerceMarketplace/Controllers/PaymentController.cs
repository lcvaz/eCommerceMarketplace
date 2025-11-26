using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.Controllers;

/// <summary>
/// PaymentController gerencia a confirmação de pagamentos via email.
///
/// FLUXO COMPLETO DE CONFIRMAÇÃO:
/// 1. Cliente faz checkout → Pedido criado com status "Pending"
/// 2. Token de confirmação é gerado e salvo no banco
/// 3. Email é enviado com link: /Payment/Confirm?token=ABC123...
/// 4. Cliente clica no link (chega aqui neste controller)
/// 5. Validamos o token (existe? expirou? já foi usado?)
/// 6. Se válido:
///    a) Muda status do pedido para "PaymentConfirmed"
///    b) Subtrai produtos do estoque
///    c) Marca produtos como "OutOfStock" se estoque zerou
///    d) Marca token como usado
///    e) Registra data de pagamento
/// 7. Exibe página de sucesso ou erro
///
/// IMPORTANTE:
/// Este controller NÃO requer autenticação ([AllowAnonymous] implícito).
/// Qualquer pessoa com o token válido pode confirmar o pagamento.
/// Isso é necessário porque o cliente pode abrir o email em outro dispositivo.
/// </summary>
public class PaymentController : Controller
{
    // ===== DEPENDÊNCIAS =====

    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentController> _logger;

    /// <summary>
    /// Construtor que recebe as dependências via Dependency Injection.
    /// </summary>
    public PaymentController(
        ApplicationDbContext context,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===== ACTION: CONFIRMAR PAGAMENTO =====

    /// <summary>
    /// Confirma o pagamento de um pedido usando o token recebido por email.
    ///
    /// URL: /Payment/Confirm?token=a1b2c3d4e5f6...
    ///
    /// VALIDAÇÕES REALIZADAS:
    /// 1. Token existe no banco de dados?
    /// 2. Token não está expirado? (24 horas)
    /// 3. Token não foi usado antes?
    /// 4. Pedido associado existe?
    /// 5. Pedido ainda está "Pending"?
    /// 6. Produtos ainda têm estoque suficiente?
    ///
    /// Se todas as validações passarem:
    /// - Confirma pagamento
    /// - Subtrai estoque
    /// - Marca token como usado
    /// - Redireciona para página de sucesso
    ///
    /// Se alguma validação falhar:
    /// - Mostra página de erro com mensagem específica
    /// </summary>
    /// <param name="token">Token de confirmação recebido por email</param>
    /// <returns>View de sucesso ou erro</returns>
    [HttpGet]
    public async Task<IActionResult> Confirm(string token)
    {
        try
        {
            // ===== VALIDAÇÃO 1: TOKEN FOI FORNECIDO? =====
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Tentativa de confirmação sem token");
                ViewBag.ErrorTitle = "Token não fornecido";
                ViewBag.ErrorMessage = "O link de confirmação está incompleto. Verifique se copiou o link completo do email.";
                return View("Error");
            }

            _logger.LogInformation($"Iniciando confirmação de pagamento com token: {token.Substring(0, 8)}...");

            // ===== VALIDAÇÃO 2: TOKEN EXISTE NO BANCO? =====
            // Buscamos o token no banco incluindo o pedido associado e seus itens
            // Include() = JOIN no SQL, traz os dados relacionados de uma vez

            var paymentToken = await _context.PaymentConfirmationTokens
                .Include(t => t.Order)                    // Traz o pedido
                    .ThenInclude(o => o.OrderItems)       // Traz os itens do pedido
                        .ThenInclude(oi => oi.Product)    // Traz os produtos dos itens
                .Include(t => t.Order.Customer)           // Traz o cliente
                .FirstOrDefaultAsync(t => t.Token == token);

            if (paymentToken == null)
            {
                _logger.LogWarning($"Token não encontrado: {token}");
                ViewBag.ErrorTitle = "Token inválido";
                ViewBag.ErrorMessage = "Este link de confirmação não é válido. Verifique se copiou o link corretamente.";
                return View("Error");
            }

            // ===== VALIDAÇÃO 3: TOKEN JÁ FOI USADO? =====
            if (paymentToken.IsUsed)
            {
                _logger.LogWarning($"Tentativa de usar token já utilizado: {token} (Pedido: {paymentToken.Order.OrderNumber})");
                ViewBag.ErrorTitle = "Pagamento já confirmado";
                ViewBag.ErrorMessage = $"O pagamento do pedido {paymentToken.Order.OrderNumber} já foi confirmado anteriormente em {paymentToken.UsedAt:dd/MM/yyyy HH:mm}.";
                ViewBag.OrderNumber = paymentToken.Order.OrderNumber;
                return View("AlreadyConfirmed");
            }

            // ===== VALIDAÇÃO 4: TOKEN EXPIROU? =====
            if (paymentToken.IsExpired)
            {
                _logger.LogWarning($"Token expirado: {token} (Expirou em: {paymentToken.ExpiresAt})");
                ViewBag.ErrorTitle = "Link expirado";
                ViewBag.ErrorMessage = $"Este link de confirmação expirou em {paymentToken.ExpiresAt:dd/MM/yyyy HH:mm}. Entre em contato conosco para obter um novo link.";
                ViewBag.OrderNumber = paymentToken.Order.OrderNumber;
                return View("Expired");
            }

            var order = paymentToken.Order;

            // ===== VALIDAÇÃO 5: PEDIDO JÁ FOI PAGO? =====
            // Verificação extra: mesmo que o token não esteja marcado como usado,
            // checamos se o pedido já foi confirmado (segurança adicional)

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning($"Pedido {order.OrderNumber} já não está mais Pending. Status atual: {order.Status}");
                ViewBag.ErrorTitle = "Pedido já processado";
                ViewBag.ErrorMessage = $"O pedido {order.OrderNumber} já foi processado anteriormente. Status atual: {order.Status}";
                ViewBag.OrderNumber = order.OrderNumber;
                return View("AlreadyConfirmed");
            }

            // ===== VALIDAÇÃO 6: PRODUTOS AINDA TÊM ESTOQUE? =====
            // Verificamos se todos os produtos ainda têm estoque suficiente.
            // É possível que entre o pedido e a confirmação, o estoque tenha acabado.

            var stockProblems = new List<string>();  // Lista de produtos sem estoque

            foreach (var orderItem in order.OrderItems)
            {
                var product = orderItem.Product;

                if (product.Stock < orderItem.Quantity)
                {
                    stockProblems.Add($"{product.Name}: estoque disponível ({product.Stock}) menor que quantidade pedida ({orderItem.Quantity})");
                }
            }

            if (stockProblems.Any())
            {
                _logger.LogWarning($"Problemas de estoque ao confirmar pedido {order.OrderNumber}: {string.Join(", ", stockProblems)}");
                ViewBag.ErrorTitle = "Estoque insuficiente";
                ViewBag.ErrorMessage = "Infelizmente alguns produtos do seu pedido não têm mais estoque disponível:";
                ViewBag.StockProblems = stockProblems;
                ViewBag.OrderNumber = order.OrderNumber;
                return View("InsufficientStock");
            }

            // ===== TODAS AS VALIDAÇÕES PASSARAM! VAMOS CONFIRMAR O PAGAMENTO =====

            _logger.LogInformation($"Confirmando pagamento do pedido {order.OrderNumber}");

            // ETAPA 1: ATUALIZAR STATUS E DATA DE PAGAMENTO DO PEDIDO
            order.Status = OrderStatus.PaymentConfirmed;
            order.PaidAt = DateTime.UtcNow;

            // ETAPA 2: SUBTRAIR ESTOQUE DE TODOS OS PRODUTOS
            // Esta é a parte CRÍTICA que só acontece AGORA, após confirmação
            foreach (var orderItem in order.OrderItems)
            {
                var product = orderItem.Product;

                _logger.LogInformation($"Subtraindo estoque: {product.Name} - Antes: {product.Stock}, Quantidade vendida: {orderItem.Quantity}");

                // Subtrai a quantidade vendida do estoque
                product.Stock -= orderItem.Quantity;

                _logger.LogInformation($"Após subtração: {product.Name} - Estoque atual: {product.Stock}");

                // Se o estoque zerou ou ficou negativo, marca produto como "Sem Estoque"
                if (product.Stock <= 0)
                {
                    product.Status = ProductStatus.OutOfStock;
                    _logger.LogInformation($"Produto {product.Name} marcado como OutOfStock");
                }
            }

            // ETAPA 3: MARCAR TOKEN COMO USADO
            // Isso impede que o mesmo link seja usado novamente
            paymentToken.IsUsed = true;
            paymentToken.UsedAt = DateTime.UtcNow;

            // ETAPA 4: SALVAR TODAS AS MUDANÇAS NO BANCO DE DADOS
            // SaveChangesAsync() executa todas as operações como uma TRANSAÇÃO:
            // - Ou todas as mudanças são salvas com sucesso
            // - Ou nenhuma é salva (se houver erro)
            // Isso garante consistência dos dados

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Pagamento confirmado com sucesso! Pedido: {order.OrderNumber}, Cliente: {order.Customer.Email}");

            // ===== REDIRECIONAR PARA PÁGINA DE SUCESSO =====
            // Passamos os dados para a view via ViewBag

            ViewBag.SuccessTitle = "Pagamento Confirmado!";
            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.TotalAmount = order.TotalAmount;
            ViewBag.CustomerName = order.Customer.FullName;

            return View("Success");
        }
        catch (Exception ex)
        {
            // ===== TRATAMENTO DE ERROS =====
            // Se algo der errado (problema no banco, etc), registramos o erro
            // e mostramos uma mensagem genérica ao usuário

            _logger.LogError(ex, $"Erro ao confirmar pagamento com token: {token}");

            ViewBag.ErrorTitle = "Erro ao confirmar pagamento";
            ViewBag.ErrorMessage = "Ocorreu um erro inesperado ao processar sua confirmação. " +
                                  "Por favor, tente novamente mais tarde ou entre em contato com nosso suporte.";
            return View("Error");
        }
    }
}
