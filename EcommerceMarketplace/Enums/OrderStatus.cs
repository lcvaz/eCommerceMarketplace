namespace EcommerceMarketplace.Enums;

public enum OrderStatus
{
    Pending = 1,           // Pendente (aguardando pagamento)
    PaymentConfirmed = 2,  // Pagamento confirmado
    Processing = 3,        // Em processamento (separando produtos)
    Shipped = 4,           // Enviado
    Delivered = 5,         // Entregue
    Canceled = 6,          // Cancelado
    Returned = 7           // Devolvido
}