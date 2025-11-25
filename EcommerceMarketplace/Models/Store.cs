using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceMarketplace.Enums; 

namespace EcommerceMarketplace.Models;

public class Store 
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da loja é obrigatório")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Url(ErrorMessage = "URL inválida")]
    public string? LogoUrl { get; set; }

    [StringLength(18)]  // 00.000.000/0000-00
    [RegularExpression(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$")]
    public string? CNPJ { get; set; }

    // ========== INFORMAÇÕES DE CONTATO ==========

    [Phone(ErrorMessage = "Formato de telefone inválido")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(100)]
    public string? ContactEmail { get; set; }

    // ========== ENDEREÇO DA LOJA ==========

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "O estado é obrigatório")]
    [StringLength(2)]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado deve ter 2 letras maiúsculas")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório")]
    [StringLength(9)]
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string ZipCode { get; set; } = string.Empty;

    // ========== TIMESTAMPS ==========

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public StoreStatus Status { get; set; } = StoreStatus.Active;

    // ========== FOREIGN KEY ==========

    [Required]
    public string VendorId { get; set; } = string.Empty;  

    // ========== NAVIGATION PROPERTIES ==========

    public ApplicationUser Vendor { get; set; } = null!;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
    
    public ICollection<ReviewStore> ReviewsStore { get; set; } = new List<ReviewStore>();  

    // ========== PROPRIEDADES CALCULADAS ==========

    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (ReviewsStore == null || !ReviewsStore.Any())  
                return 0;
            
            return Math.Round(ReviewsStore.Average(r => r.Rating), 1);
        }
    }

    [NotMapped]
    public int TotalReviews => ReviewsStore?.Count ?? 0;  
}

