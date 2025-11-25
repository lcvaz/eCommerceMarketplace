using System.ComponentModel.DataAnnotations;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para criação de novos produtos.
/// Este modelo contém todos os campos necessários para o formulário de criação de produto.
/// </summary>
public class CreateProductViewModel
{
    // ===== DADOS BÁSICOS DO PRODUTO =====

    [Required(ErrorMessage = "O nome do produto é obrigatório")]
    [StringLength(200, ErrorMessage = "O nome não pode ter mais de 200 caracteres")]
    [Display(Name = "Nome do Produto")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }

    // ===== PREÇO E ESTOQUE =====

    [Required(ErrorMessage = "O preço é obrigatório")]
    [Range(0.01, 999999.99, ErrorMessage = "O preço deve estar entre R$ 0,01 e R$ 999.999,99")]
    [Display(Name = "Preço")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
    [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo")]
    [Display(Name = "Quantidade em Estoque")]
    public int Stock { get; set; } = 0;

    // ===== IDENTIFICAÇÃO =====

    [Required(ErrorMessage = "O SKU é obrigatório")]
    [StringLength(50, ErrorMessage = "O SKU não pode ter mais de 50 caracteres")]
    [Display(Name = "SKU (Código do Produto)")]
    public string SKU { get; set; } = string.Empty;

    // ===== IMAGEM DO PRODUTO =====

    [Required(ErrorMessage = "A URL da imagem é obrigatória")]
    [Url(ErrorMessage = "URL inválida")]
    [Display(Name = "URL da Imagem do Produto")]
    public string ImageUrl { get; set; } = string.Empty;

    // ===== CATEGORIA =====

    [Display(Name = "Categoria")]
    public int? CategoryId { get; set; }

    // ===== STATUS =====

    [Display(Name = "Status do Produto")]
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    // ===== LOJA =====

    /// <summary>
    /// ID da loja à qual este produto pertence.
    /// Este campo será preenchido automaticamente pelo controller
    /// com base na loja selecionada pelo vendedor.
    /// </summary>
    [Required]
    public int StoreId { get; set; }
}
