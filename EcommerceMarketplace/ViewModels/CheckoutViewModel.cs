using System.ComponentModel.DataAnnotations;

namespace EcommerceMarketplace.ViewModels
{
    public class CheckoutViewModel
    {
        // Dados Pessoais
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome completo")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF inválido. Use o formato 000.000.000-00")]
        [Display(Name = "CPF")]
        public string CPF { get; set; } = string.Empty;

        // Endereço de Entrega
        [Required(ErrorMessage = "CEP é obrigatório")]
        [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP inválido. Use o formato 00000-000")]
        [Display(Name = "CEP")]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rua é obrigatória")]
        [Display(Name = "Rua")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório")]
        [Display(Name = "Número")]
        public string Number { get; set; } = string.Empty;

        [Display(Name = "Complemento")]
        public string? Complement { get; set; }

        [Required(ErrorMessage = "Bairro é obrigatório")]
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cidade é obrigatória")]
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estado é obrigatório")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Estado inválido. Use a sigla de 2 letras (ex: SP)")]
        [Display(Name = "Estado")]
        public string State { get; set; } = string.Empty;

        // Forma de Pagamento
        [Required(ErrorMessage = "Forma de pagamento é obrigatória")]
        [Display(Name = "Forma de pagamento")]
        public string PaymentMethod { get; set; } = "CreditCard";

        // Dados do Cartão de Crédito (somente se PaymentMethod == "CreditCard")
        [Display(Name = "Número do cartão")]
        public string? CardNumber { get; set; }

        [Display(Name = "Nome impresso no cartão")]
        public string? CardHolderName { get; set; }

        [Display(Name = "Validade")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Validade inválida. Use o formato MM/AA")]
        public string? CardExpiry { get; set; }

        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV inválido")]
        public string? CardCVV { get; set; }

        // Resumo do Pedido
        public List<CheckoutItemViewModel> Items { get; set; } = new List<CheckoutItemViewModel>();

        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total => Subtotal + ShippingCost - Discount;

        // Opção para salvar endereço
        [Display(Name = "Salvar este endereço para futuras compras")]
        public bool SaveAddress { get; set; } = true;
    }

    public class CheckoutItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = "/images/placeholder.jpg";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
