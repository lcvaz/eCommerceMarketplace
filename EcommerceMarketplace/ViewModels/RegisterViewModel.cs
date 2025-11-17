using System.ComponentModel.DataAnnotations;
using EcommerceMarketplace.Enums;


namespace EcommerceMarketplace.ViewModels;

///<summary>
/// Esta classe é um DTO
///</summary>
public class RegisterViewModel 
{
    [Display(Name = "Nome Completo")]
    [Required(ErrorMessage = "O nome não pode ser nulo.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 200 caracteres")]
    public string FullName { get; set;} = string.Empty;


    [Display(Name = "Email")]
    [Required(ErrorMessage = "O Email não pode ser nulo.")]
    [EmailAddress(ErrorMessage = "Revise o formato do Email")]
    public string Email { get; set;} = string.Empty;


    [Display(Name = "CPF")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "CPF deve ter 14 caracteres")]
    [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
    public string? CPF { get; set;} = string.Empty;


    [Display(Name = "Senha")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "A senha não pode ser nula.")]
    public string Password { get; set;} = string.Empty;


    [Display(Name = "Confirmação de senha")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "As senhas não coincidem")]
    [Required(ErrorMessage = "Confirme sua senha.")]
    public string ConfirmPassword { get; set;} = string.Empty;


    [Display(Name = "Tipo de Conta")]
    [Required(ErrorMessage = "Selecione o tipo de conta")]
    public AccountType AccountType { get; set; } = AccountType.Cliente;
}