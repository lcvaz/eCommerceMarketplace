using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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

public enum StoreStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}