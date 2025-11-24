using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para o formulário de criação de loja.
/// 
/// Este ViewModel representa os dados que o vendedor precisa fornecer
/// quando está criando uma nova loja no marketplace.
/// 
/// Cada propriedade aqui corresponde a um campo no formulário HTML.
/// As Data Annotations (atributos como [Required], [StringLength]) fazem
/// duas coisas importantes:
/// 1. Validação no servidor (ASP.NET Core valida antes de salvar no banco)
/// 2. Validação no cliente (JavaScript valida antes de enviar o formulário)
/// </summary>
public class CreateStoreViewModel
{
    // ===== INFORMAÇÕES BÁSICAS DA LOJA =====
    
    /// <summary>
    /// Nome da loja que será exibido no marketplace.
    /// Exemplo: "Eletrônicos Premium", "Moda Feminina Center", "Papelaria Express"
    /// 
    /// [Required] = Campo obrigatório, não pode ser vazio
    /// [StringLength] = Define tamanho mínimo e máximo
    /// [Display] = Define o label que aparece no formulário
    /// </summary>
    [Display(Name = "Nome da Loja")]
    [Required(ErrorMessage = "O nome da loja é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da loja explicando o que ela vende ou seu diferencial.
    /// Exemplo: "Especializada em eletrônicos importados com garantia estendida"
    /// 
    /// Este campo é opcional (não tem [Required]), mas se for preenchido
    /// precisa ter pelo menos 10 caracteres para ser significativo.
    /// </summary>
    [Display(Name = "Descrição")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 500 caracteres")]
    public string? Description { get; set; }
    
    // ===== INFORMAÇÕES DE CONTATO =====
    // Estas informações ajudam os clientes a entrar em contato com a loja
    
    /// <summary>
    /// Número de telefone da loja para contato.
    /// Exemplo: "(11) 98765-4321"
    /// 
    /// Não é obrigatório no momento da criação, mas é recomendado.
    /// Usamos [Phone] para validar se o formato parece um telefone válido.
    /// </summary>
    [Display(Name = "Telefone")]
    [Phone(ErrorMessage = "Formato de telefone inválido")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Email de contato da loja (pode ser diferente do email do vendedor).
    /// Exemplo: "contato@minhaloja.com"
    /// 
    /// Também opcional, mas se for fornecido precisa ser um email válido.
    /// [EmailAddress] valida o formato do email (precisa ter @ e domínio).
    /// </summary>
    [Display(Name = "Email de Contato")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
    public string? ContactEmail { get; set; }
    
    // ===== ENDEREÇO DA LOJA =====
    // Importante para lojas físicas ou para cálculo de frete
    
    /// <summary>
    /// Endereço completo da loja (rua, número, complemento).
    /// Exemplo: "Av. Paulista, 1000, Sala 301"
    /// 
    /// Obrigatório porque precisamos saber de onde os produtos serão enviados
    /// para calcular o frete corretamente.
    /// </summary>
    [Display(Name = "Endereço")]
    [Required(ErrorMessage = "O endereço é obrigatório")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "O endereço deve ter entre 5 e 200 caracteres")]
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Cidade onde a loja está localizada.
    /// Exemplo: "São Paulo"
    /// </summary>
    [Display(Name = "Cidade")]
    [Required(ErrorMessage = "A cidade é obrigatória")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A cidade deve ter entre 2 e 100 caracteres")]
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado (UF) da loja.
    /// Exemplo: "SP", "RJ", "MG"
    /// 
    /// Validamos com RegularExpression para garantir que são 2 letras maiúsculas.
    /// </summary>
    [Display(Name = "Estado (UF)")]
    [Required(ErrorMessage = "O estado é obrigatório")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado deve ser no formato XX (ex: SP, RJ)")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// CEP da loja para cálculo de frete.
    /// Exemplo: "01310-100"
    /// 
    /// O formato brasileiro de CEP é XXXXX-XXX (5 dígitos, hífen, 3 dígitos).
    /// Usamos RegularExpression para garantir este formato.
    /// </summary>
    [Display(Name = "CEP")]
    [Required(ErrorMessage = "O CEP é obrigatório")]
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "CEP deve ter 9 caracteres")]
    public string ZipCode { get; set; } = string.Empty;
    
    // ===== NOTA SOBRE O DONO DA LOJA =====
    // Repara que não temos uma propriedade "OwnerId" aqui.
    // Isso é proposital! O dono da loja é automaticamente o vendedor que está logado.
    // O Controller vai pegar o ID do usuário logado e associar à loja.
    // Não queremos que o vendedor possa escolher quem é o dono - isso seria uma falha de segurança!
}