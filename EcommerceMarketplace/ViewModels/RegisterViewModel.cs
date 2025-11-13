using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.ViewModels;

///<summary>
/// Esta classe é um DTO
///</summary>
public class RegisterViewModel 
{
    [Required]
    public string FullName { get; set;} = string.Empty;
    public string Email { get; set;} = string.Empty;
    public string CPF { get; set;} = string.Empty;
    public string Password { get; set;} = string.Empty;
    public string ConfirmPassword { get; set;} = string.Empty;
}