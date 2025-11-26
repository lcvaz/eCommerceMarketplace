namespace EcommerceMarketplace.Services;

/// <summary>
/// Interface que define os métodos para envio de emails.
///
/// PARA QUE SERVE UMA INTERFACE?
/// Uma interface é como um "contrato" que define QUAIS métodos uma classe deve ter,
/// mas NÃO define COMO esses métodos funcionam.
///
/// Vantagens:
/// 1. TESTABILIDADE: Podemos criar uma versão "fake" para testes
/// 2. FLEXIBILIDADE: Podemos trocar a implementação (ex: de SMTP para SendGrid) sem mudar o código que usa
/// 3. DEPENDENCY INJECTION: O ASP.NET Core pode injetar automaticamente a implementação correta
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um email de confirmação de pedido com link para confirmar pagamento.
    ///
    /// COMO FUNCIONA:
    /// 1. Recebe os dados do pedido e do destinatário
    /// 2. Monta um email HTML bonito com as informações
    /// 3. Inclui um link para o usuário confirmar o pagamento
    /// 4. Envia o email usando SMTP (ou outro provedor configurado)
    ///
    /// QUANDO USAR:
    /// - Logo após criar um pedido com status Pending
    /// - O link de confirmação deve redirecionar para /Payment/Confirm?token={token}
    /// </summary>
    /// <param name="recipientEmail">Email de quem vai receber (cliente)</param>
    /// <param name="recipientName">Nome do cliente</param>
    /// <param name="orderNumber">Número do pedido (ex: PED-2025-000001)</param>
    /// <param name="totalAmount">Valor total do pedido</param>
    /// <param name="confirmationToken">Token único para confirmar o pagamento</param>
    /// <returns>Task que completa quando o email for enviado (ou falhar)</returns>
    Task SendOrderConfirmationEmailAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        decimal totalAmount,
        string confirmationToken
    );

    /// <summary>
    /// Envia um email genérico (pode ser usado para outras funcionalidades futuras).
    ///
    /// Este é um método mais flexível para enviar qualquer tipo de email.
    /// Útil para: recuperação de senha, notificações, newsletters, etc.
    /// </summary>
    /// <param name="to">Email do destinatário</param>
    /// <param name="subject">Assunto do email</param>
    /// <param name="htmlBody">Corpo do email em HTML</param>
    /// <returns>Task que completa quando o email for enviado</returns>
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
