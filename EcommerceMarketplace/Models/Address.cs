using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.Models;

/// <summary>
/// Address representa um endereço de entrega do cliente.
/// Um cliente pode ter vários endereços (casa, trabalho, etc).
/// </summary>
public class Address
{
    public int Id { get; set; }

    /// <summary>
    /// Apelido do endereço para identificação
    /// Exemplo: "Casa", "Trabalho", "Casa da Mãe"
    /// </summary>
    [Required(ErrorMessage = "O apelido é obrigatório")]
    [StringLength(50)]
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo de quem vai receber
    /// </summary>
    [Required(ErrorMessage = "O nome do destinatário é obrigatório")]
    [StringLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// CEP - Código de Endereçamento Postal
    /// Formato: 00000-000
    /// </summary>
    [Required]
    [StringLength(9)]
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Rua/Avenida
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Número da residência
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Complemento (Apto, Bloco, etc)
    /// </summary>
    [StringLength(100)]
    public string? Complement { get; set; }

    /// <summary>
    /// Bairro
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    /// <summary>
    /// Cidade
    /// </summary>
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Estado (UF)
    /// Exemplo: SP, RJ, MG
    /// </summary>
    [Required]
    [StringLength(2)]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado deve ter 2 letras maiúsculas")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// País (geralmente "Brasil" para marketplace brasileiro)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Country { get; set; } = "Brasil";

    /// <summary>
    /// Se este é o endereço padrão do cliente
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ========== FOREIGN KEY ==========

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Customer { get; set; } = null!;

    /// <summary>
    /// Pedidos que usaram este endereço
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}