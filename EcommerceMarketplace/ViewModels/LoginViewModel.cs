using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.ViewModels;

///<summary>
/// Esta classe é um DTO para login
///</summary>
public class LoginViewModel 
{
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


    [Display(Name = "Lembrar-me")]
    public bool RememberMe { get; set; } = false;
}