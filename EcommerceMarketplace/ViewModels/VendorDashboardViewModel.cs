namespace EcommerceMarketplace.ViewModels;

/// <summary>
/// ViewModel para o dashboard do vendedor.
/// Este é o "modelo de dados" que representa tudo que aparece na tela do dashboard.
/// 
/// Pensa assim: se você fosse desenhar a tela do dashboard em um papel,
/// cada informação que você desenharia precisa estar representada aqui.
/// </summary>
public class VendorDashboardViewModel
{
    // ===== MÉTRICAS GERAIS (Cards do Topo) =====
    // Estes três valores aparecem nos três cards grandes no topo da tela
    
    /// <summary>
    /// Número total de produtos em todas as lojas do vendedor.
    /// Exemplo: Se o vendedor tem 3 lojas, uma com 5 produtos, outra com 3 e outra com 2,
    /// este valor será 10.
    /// </summary>
    public int TotalProducts { get; set; }
    
    /// <summary>
    /// Quantidade total de vendas realizadas (número de pedidos que contêm produtos do vendedor).
    /// IMPORTANTE: Contamos apenas pedidos COMPLETOS/PAGOS, não pedidos pendentes ou cancelados.
    /// Exemplo: Se 100 pedidos diferentes compraram pelo menos um produto deste vendedor, será 100.
    /// </summary>
    public int TotalSales { get; set; }
    
    /// <summary>
    /// Receita total de todas as lojas (soma do valor de todos os itens vendidos).
    /// É calculado somando: (quantidade × preço unitário) de cada OrderItem.
    /// Exemplo: Se vendeu 10 produtos a R$50 e 5 produtos a R$100, será R$1.000.
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    // ===== INFORMAÇÕES DAS LOJAS =====
    
    /// <summary>
    /// Lista de todas as lojas do vendedor, cada uma com suas métricas individuais.
    /// Esta lista será usada para renderizar os cards de loja na seção "Minhas Lojas".
    /// 
    /// Inicializamos com uma lista vazia para evitar NullReferenceException.
    /// Assim, mesmo que o vendedor não tenha lojas, Stores.Any() retorna false ao invés de dar erro.
    /// </summary>
    public List<StoreCardViewModel> Stores { get; set; } = new List<StoreCardViewModel>();
    
    // ===== PRODUTO MAIS VENDIDO =====
    
    /// <summary>
    /// Informações do produto mais vendido nos últimos 3 meses (entre todas as lojas).
    /// 
    /// É nullable (?) porque pode ser que o vendedor não tenha nenhuma venda nos últimos 3 meses.
    /// Quando é null, a View pode exibir uma mensagem como "Nenhuma venda nos últimos 3 meses".
    /// </summary>
    public TopProductViewModel? TopProduct { get; set; }
    
    // ===== PROPRIEDADES CALCULADAS (Computed Properties) =====
    // Estas propriedades não são "setadas" - elas são calculadas automaticamente
    // baseadas em outras propriedades. São úteis para deixar a View mais limpa.
    
    /// <summary>
    /// Verifica se o vendedor tem pelo menos uma loja.
    /// Isso é útil na View para decidir se mostra a seção de lojas ou uma mensagem
    /// dizendo "Você ainda não tem lojas cadastradas".
    /// 
    /// Repara o "=>" (expression body). É uma forma curta de escrever:
    /// public bool HasStores { get { return Stores != null && Stores.Any(); } }
    /// </summary>
    public bool HasStores => Stores != null && Stores.Any();
    
    /// <summary>
    /// Verifica se o vendedor teve vendas.
    /// Útil para mostrar mensagens diferentes quando o vendedor ainda não vendeu nada.
    /// </summary>
    public bool HasSales => TotalSales > 0;
}