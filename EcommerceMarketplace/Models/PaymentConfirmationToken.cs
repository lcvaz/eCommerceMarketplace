using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMarketplace.Models;

/// <summary>
/// PaymentConfirmationToken armazena tokens únicos para confirmação de pagamento via email.
///
/// PARA QUE SERVE?
/// Quando um cliente faz um pedido, criamos um token único (como uma "senha temporária")
/// e enviamos por email. Quando o cliente clica no link do email, ele confirma o pagamento
/// usando este token.
///
/// FLUXO COMPLETO:
/// 1. Cliente faz checkout → Pedido criado com status "Pending"
/// 2. Sistema gera um token único (GUID aleatório)
/// 3. Token é salvo nesta tabela, associado ao pedido
/// 4. Email é enviado com link: /Payment/Confirm?token=ABC123...
/// 5. Cliente clica no link
/// 6. Sistema valida o token (existe? está expirado? já foi usado?)
/// 7. Se válido: Confirma pagamento, subtrai estoque, muda status para "PaymentConfirmed"
/// 8. Token é marcado como usado (para não poder ser reutilizado)
///
/// SEGURANÇA:
/// - Token é um GUID (Globally Unique Identifier) com 32 caracteres
/// - Praticamente impossível de adivinhar (2^128 possibilidades)
/// - Expira em 24 horas
/// - Só pode ser usado uma vez
/// - Associado a um pedido específico
/// </summary>
public class PaymentConfirmationToken
{
    // ===== PROPRIEDADES PRINCIPAIS =====

    /// <summary>
    /// ID único do registro (chave primária).
    /// Gerado automaticamente pelo banco de dados.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// O token em si - uma string única e aleatória.
    ///
    /// FORMATO: GUID sem hífens, exemplo: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
    ///
    /// COMO É GERADO:
    /// Token = Guid.NewGuid().ToString("N")
    ///
    /// O "N" significa "sem hífens". Um GUID normal seria:
    /// "a1b2c3d4-e5f6-g7h8-i9j0-k1l2m3n4o5p6"
    /// Com "N" fica: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6" (mais limpo para URLs)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o token foi criado.
    /// Usado para calcular se o token expirou.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data e hora de expiração do token.
    ///
    /// REGRA DE NEGÓCIO:
    /// Token é válido por 24 horas após criação.
    /// Se o cliente tentar usar depois disso, deve receber mensagem de "token expirado".
    ///
    /// COMO CALCULAR:
    /// ExpiresAt = DateTime.UtcNow.AddHours(24)
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indica se o token já foi usado.
    ///
    /// IMPORTANTE: Um token só pode ser usado UMA VEZ.
    /// Depois de confirmar o pagamento, IsUsed = true.
    /// Se alguém tentar usar o mesmo link novamente, deve ser rejeitado.
    ///
    /// Cenário: Cliente confirma pagamento → IsUsed = true
    ///          Cliente clica no link de novo → Mensagem "Este pagamento já foi confirmado"
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Data e hora em que o token foi usado (se foi usado).
    /// Null = ainda não foi usado.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    // ===== FOREIGN KEYS =====

    /// <summary>
    /// ID do pedido ao qual este token pertence.
    /// Cada token está associado a exatamente um pedido.
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    // ===== NAVIGATION PROPERTIES =====

    /// <summary>
    /// O pedido ao qual este token está associado.
    ///
    /// Entity Framework usa isto para criar o relacionamento no banco:
    /// PaymentConfirmationTokens.OrderId → Orders.Id (Foreign Key)
    /// </summary>
    public Order Order { get; set; } = null!;

    // ===== PROPRIEDADES CALCULADAS =====

    /// <summary>
    /// Verifica se o token está expirado.
    ///
    /// COMO FUNCIONA:
    /// Compara a data/hora atual (DateTime.UtcNow) com a data de expiração (ExpiresAt).
    /// Se a data atual for maior que ExpiresAt, está expirado.
    ///
    /// [NotMapped] significa que esta propriedade NÃO é salva no banco de dados.
    /// É calculada em tempo real sempre que você acessa.
    ///
    /// EXEMPLO:
    /// Token criado em: 26/11/2025 10:00
    /// Expira em: 27/11/2025 10:00
    /// Verificação em: 27/11/2025 15:00
    /// IsExpired = true (porque 15:00 > 10:00)
    /// </summary>
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Verifica se o token é válido (não usado E não expirado).
    ///
    /// Um token é considerado válido quando TODAS estas condições são verdadeiras:
    /// 1. Ainda não foi usado (IsUsed = false)
    /// 2. Não está expirado (IsExpired = false)
    ///
    /// Esta propriedade é muito útil para validação rápida:
    /// if (!token.IsValid) {
    ///     return "Token inválido ou expirado";
    /// }
    /// </summary>
    [NotMapped]
    public bool IsValid => !IsUsed && !IsExpired;
}
