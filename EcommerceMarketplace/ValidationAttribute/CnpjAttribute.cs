using Caelum.Stella.CSharp.Validation;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.ValidationAttributes;

public class CnpjAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;
        
        var validator = new CNPJValidator();
        
        try
        {
            validator.AssertValid(value.ToString()!);
            return ValidationResult.Success;
        }
        catch
        {
            return new ValidationResult("CNPJ inválido");
        }
    }
}
