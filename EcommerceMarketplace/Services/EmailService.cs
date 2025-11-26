using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcommerceMarketplace.Services;

/// <summary>
/// Implementação do serviço de email usando SMTP.
///
/// O QUE É SMTP?
/// SMTP (Simple Mail Transfer Protocol) é o protocolo padrão para envio de emails na internet.
/// É como o "correio" da internet - você entrega a carta (email) para o servidor SMTP,
/// e ele se encarrega de entregar ao destinatário.
///
/// COMO FUNCIONA ESTE SERVIÇO:
/// 1. Lê as configurações do SMTP do arquivo appsettings.json
/// 2. Quando você chama um método para enviar email, ele:
///    a) Cria uma conexão com o servidor SMTP
///    b) Autentica usando usuário/senha
///    c) Envia o email
///    d) Fecha a conexão
///
/// CONFIGURAÇÃO NECESSÁRIA:
/// No appsettings.json, você precisa ter:
/// {
///   "EmailSettings": {
///     "SmtpServer": "smtp.gmail.com",
///     "SmtpPort": 587,
///     "SenderEmail": "seu-email@gmail.com",
///     "SenderName": "Marketplace",
///     "Username": "seu-email@gmail.com",
///     "Password": "sua-senha-de-app"
///   }
/// }
///
/// IMPORTANTE PARA DESENVOLVIMENTO:
/// - Gmail: Use "App Password", não sua senha normal (por segurança)
/// - Mailtrap: Serviço gratuito para testar emails em desenvolvimento
/// - Ethereal: Outro serviço de teste que gera emails temporários
/// </summary>
public class EmailService : IEmailService
{
    // ===== DEPENDÊNCIAS =====
    // Estas são injetadas automaticamente pelo ASP.NET Core

    private readonly IConfiguration _configuration;  // Para ler appsettings.json
    private readonly ILogger<EmailService> _logger;  // Para registrar logs (debug, erros, etc)

    /// <summary>
    /// Construtor que recebe as dependências via Dependency Injection.
    /// </summary>
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ===== MÉTODO PÚBLICO: ENVIAR EMAIL DE CONFIRMAÇÃO DE PEDIDO =====

    /// <summary>
    /// Envia email de confirmação de pedido com link para confirmar pagamento.
    ///
    /// FLUXO DETALHADO:
    /// 1. Loga que está enviando o email (para debug)
    /// 2. Monta o corpo do email em HTML (bonito e responsivo)
    /// 3. Chama o método SendEmailAsync para fazer o envio
    /// 4. Loga sucesso ou erro
    /// </summary>
    public async Task SendOrderConfirmationEmailAsync(
        string recipientEmail,
        string recipientName,
        string orderNumber,
        decimal totalAmount,
        string confirmationToken)
    {
        try
        {
            _logger.LogInformation($"Enviando email de confirmação de pedido para {recipientEmail}. Pedido: {orderNumber}");

            // ===== MONTAR O ASSUNTO DO EMAIL =====
            var subject = $"Confirme seu pedido #{orderNumber}";

            // ===== MONTAR O CORPO DO EMAIL EM HTML =====
            // Este HTML cria um email bonito e profissional
            // Usamos estilos inline porque muitos clientes de email não aceitam CSS externo

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmação de Pedido</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>

    <!-- CABEÇALHO -->
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>🛍️ Pedido Realizado!</h1>
        <p style='margin: 10px 0 0 0; font-size: 16px;'>Confirme seu pagamento para finalizar</p>
    </div>

    <!-- CONTEÚDO PRINCIPAL -->
    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>

        <!-- SAUDAÇÃO -->
        <p style='font-size: 16px; margin-bottom: 20px;'>
            Olá <strong>{recipientName}</strong>,
        </p>

        <p style='font-size: 15px; margin-bottom: 20px;'>
            Recebemos seu pedido e estamos quase lá! Para finalizar sua compra,
            precisamos que você <strong>confirme o pagamento</strong> clicando no botão abaixo.
        </p>

        <!-- INFORMAÇÕES DO PEDIDO -->
        <div style='background: white; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 4px solid #667eea;'>
            <h2 style='margin: 0 0 15px 0; font-size: 18px; color: #667eea;'>📦 Detalhes do Pedido</h2>

            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; color: #666;'>Número do Pedido:</td>
                    <td style='padding: 8px 0; text-align: right;'><strong>{orderNumber}</strong></td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #666;'>Valor Total:</td>
                    <td style='padding: 8px 0; text-align: right;'><strong style='color: #28a745; font-size: 18px;'>{totalAmount:C}</strong></td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #666;'>Status:</td>
                    <td style='padding: 8px 0; text-align: right;'>
                        <span style='background: #ffc107; color: #000; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: bold;'>
                            AGUARDANDO CONFIRMAÇÃO
                        </span>
                    </td>
                </tr>
            </table>
        </div>

        <!-- BOTÃO DE CONFIRMAÇÃO -->
        <div style='text-align: center; margin: 30px 0;'>
            <a href='http://localhost:5005/Payment/Confirm?token={confirmationToken}'
               style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);'>
                ✅ Confirmar Pagamento
            </a>
        </div>

        <!-- INSTRUÇÕES -->
        <div style='background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 25px 0;'>
            <p style='margin: 0; font-size: 14px; color: #856404;'>
                <strong>⏰ Importante:</strong> Este link é válido por 24 horas.
                Após confirmação, subtrairemos os produtos do estoque e processaremos seu pedido imediatamente.
            </p>
        </div>

        <!-- INFORMAÇÕES ADICIONAIS -->
        <p style='font-size: 14px; color: #666; margin-top: 25px;'>
            Ao confirmar o pagamento, você receberá um novo email com os detalhes de rastreamento
            e previsão de entrega.
        </p>

        <p style='font-size: 14px; color: #666;'>
            Se você não realizou este pedido, pode ignorar este email com segurança.
        </p>

        <!-- RODAPÉ -->
        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center;'>
            <p style='font-size: 14px; color: #666; margin: 5px 0;'>
                Obrigado por comprar conosco! 💜
            </p>
            <p style='font-size: 12px; color: #999; margin: 5px 0;'>
                eCommerce Marketplace - Sua loja online completa
            </p>
        </div>
    </div>

</body>
</html>";

            // ===== ENVIAR O EMAIL =====
            // Chama o método genérico de envio que faz a conexão SMTP
            await SendEmailAsync(recipientEmail, subject, htmlBody);

            _logger.LogInformation($"Email de confirmação enviado com sucesso para {recipientEmail}");
        }
        catch (Exception ex)
        {
            // Se algo der errado, loga o erro mas não quebra a aplicação
            // O pedido já foi criado, então o cliente pode entrar em contato se não receber o email
            _logger.LogError(ex, $"Erro ao enviar email de confirmação para {recipientEmail}");

            // Re-lança a exceção para que o controller saiba que houve um problema
            // (mas o pedido já foi salvo no banco)
            throw;
        }
    }

    // ===== MÉTODO PÚBLICO: ENVIAR EMAIL GENÉRICO =====

    /// <summary>
    /// Envia um email genérico usando SMTP.
    ///
    /// ESTE É O MÉTODO PRINCIPAL que faz o trabalho pesado de conectar ao SMTP e enviar.
    /// Todos os outros métodos de envio de email eventualmente chamam este aqui.
    ///
    /// COMO FUNCIONA:
    /// 1. Lê as configurações do SMTP do appsettings.json
    /// 2. Valida se todas as configurações necessárias existem
    /// 3. Cria um objeto MailMessage (o email em si)
    /// 4. Cria um objeto SmtpClient (o "carteiro" que vai enviar)
    /// 5. Configura autenticação e segurança (SSL/TLS)
    /// 6. Envia o email
    /// 7. Faz cleanup (libera recursos)
    ///
    /// POSSÍVEIS ERROS:
    /// - Configurações ausentes: Verifica se todas as configs estão no appsettings.json
    /// - Falha de autenticação: Username/password incorretos
    /// - Timeout: Servidor SMTP não responde
    /// - Email rejeitado: Email de destino inválido
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            // ===== ETAPA 1: LER CONFIGURAÇÕES =====
            // Pega as configurações da seção "EmailSettings" do appsettings.json

            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = _configuration["EmailSettings:SmtpPort"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            // ===== ETAPA 2: VALIDAR CONFIGURAÇÕES =====
            // Se alguma configuração estiver faltando, não conseguimos enviar email

            if (string.IsNullOrEmpty(smtpServer) ||
                string.IsNullOrEmpty(smtpPort) ||
                string.IsNullOrEmpty(senderEmail) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                var errorMsg = "Configurações de email incompletas no appsettings.json. " +
                              "Verifique se EmailSettings:SmtpServer, SmtpPort, SenderEmail, Username e Password estão configurados.";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            _logger.LogInformation($"Enviando email para {to} com assunto '{subject}'");

            // ===== ETAPA 3: CRIAR O EMAIL (MailMessage) =====
            // MailMessage representa o email em si (remetente, destinatário, assunto, corpo)

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),  // De quem é o email
                Subject = subject,                                 // Assunto
                Body = htmlBody,                                   // Corpo (HTML)
                IsBodyHtml = true,                                 // Indica que o corpo é HTML (não texto puro)
                Priority = MailPriority.Normal                     // Prioridade normal
            };

            // Adiciona o destinatário
            mailMessage.To.Add(new MailAddress(to));

            // ===== ETAPA 4: CRIAR O CLIENTE SMTP (SmtpClient) =====
            // SmtpClient é quem realmente faz o envio, conectando ao servidor SMTP

            using var smtpClient = new SmtpClient(smtpServer, int.Parse(smtpPort))
            {
                // Credenciais de autenticação
                Credentials = new NetworkCredential(username, password),

                // EnableSsl = true significa usar criptografia (IMPORTANTE para segurança!)
                // Todos os emails modernos usam SSL/TLS para proteger os dados
                EnableSsl = true,

                // Timeout de 30 segundos (se não conseguir enviar em 30s, desiste)
                Timeout = 30000
            };

            // ===== ETAPA 5: ENVIAR O EMAIL =====
            // SendMailAsync é assíncrono (não trava a aplicação enquanto envia)

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation($"Email enviado com sucesso para {to}");
        }
        catch (SmtpException ex)
        {
            // Erros específicos de SMTP (servidor não responde, autenticação falhou, etc)
            _logger.LogError(ex, $"Erro SMTP ao enviar email para {to}: {ex.Message}");
            throw new Exception($"Falha ao enviar email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            // Outros erros genéricos
            _logger.LogError(ex, $"Erro inesperado ao enviar email para {to}: {ex.Message}");
            throw;
        }
    }
}
