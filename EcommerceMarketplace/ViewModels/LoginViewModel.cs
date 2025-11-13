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


    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "A senha não pode ser nula.")]
    public string Password { get; set;} = string.Empty;


    [Display(Name = "Lembrar-me")]
    public bool RememberMe { get; set; } = false;
}