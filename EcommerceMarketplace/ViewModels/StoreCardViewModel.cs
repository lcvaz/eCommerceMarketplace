namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para cada card de loja no dashboard.
/// 
/// Pensa neste ViewModel como a "ficha" de uma loja. Ele contém todas as informações
/// que precisamos mostrar em cada card na seção "Minhas Lojas".
/// 
/// No protótipo, você viu três cards de loja. Cada card é uma instância deste ViewModel.
/// Por exemplo, "Loja Centro" é um StoreCardViewModel, "Loja Jardins" é outro, e assim por diante.
/// </summary>
public class StoreCardViewModel
{
    // ===== INFORMAÇÕES BÁSICAS DA LOJA =====
    
    /// <summary>
    /// ID da loja no banco de dados.
    /// Vamos usar este ID para criar links, por exemplo: /Vendor/ManageStore/5
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome da loja que aparece no card.
    /// Exemplo: "Loja Centro", "Loja Jardins", "Loja Online"
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição curta ou localização da loja.
    /// No protótipo, aparece abaixo do nome, como "Centro, São Paulo" ou "E-commerce".
    /// 
    /// É nullable (?) porque nem toda loja precisa ter uma descrição.
    /// Se for null, a View simplesmente não exibe esse campo.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// URL do logo da loja (se houver).
    /// No futuro, quando você implementar upload de logo, este campo será usado.
    /// Por enquanto, pode ficar null e você exibe um ícone padrão.
    /// </summary>
    public string? LogoUrl { get; set; }
    
    // ===== MÉTRICAS DA LOJA =====
    
    /// <summary>
    /// Número de vendas DESTA loja específica.
    /// Exemplo: Se 45 pedidos diferentes compraram produtos desta loja, será 45.
    /// 
    /// Importante: é o número de PEDIDOS que contêm produtos desta loja,
    /// não o número total de itens vendidos.
    /// </summary>
    public int Sales { get; set; }
    
    /// <summary>
    /// Receita gerada POR ESTA loja.
    /// É a soma de: (quantidade × preço unitário) de todos os OrderItems
    /// cujo produto pertence a esta loja.
    /// </summary>
    public decimal Revenue { get; set; }
    
    // ===== STATUS =====
    
    /// <summary>
    /// Status atual da loja (Active, Inactive, Suspended, etc).
    /// Guardamos como string porque é mais fácil de exibir na View.
    /// 
    /// Você pode usar este campo para, por exemplo:
    /// - Mostrar um badge verde para "Active"
    /// - Mostrar um badge vermelho para "Suspended"
    /// - Desabilitar certas ações se a loja estiver Inactive
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    // ===== PROPRIEDADES CALCULADAS =====
    
    /// <summary>
    /// Verifica se a loja está ativa.
    /// Útil para conditional rendering na View, por exemplo:
    /// @if (store.IsActive) { <!-- mostrar botão de adicionar produto --> }
    /// </summary>
    public bool IsActive => Status == "Active";
    
    /// <summary>
    /// Retorna a classe CSS apropriada para o badge de status.
    /// Isso deixa a View mais limpa, porque a lógica de cores fica aqui no ViewModel.
    /// </summary>
    public string StatusBadgeClass => Status switch
    {
        "Active" => "bg-success",
        "Inactive" => "bg-secondary",
        "Suspended" => "bg-danger",
        _ => "bg-warning"
    };
}