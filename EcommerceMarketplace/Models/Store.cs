namespace EcommerceMarketplace.Models;

/// <summary>
/// Store.cs representa uma loja de um vendedor(CNPJ/CPF) no sistema.
/// Cada loja pode ter vários produtos 
/// </summary>  

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Data de criação da loja
    /// essa sintaxe é para inicializar o campo com a data e hora atual 
    /// sem o set na propriedade o valor será sempre a data e hora atual e não será alterado
    /// </summary>
    public DateTime CriadoEm { get; } = DateTime.UtcNow; 
    
    
    /// <summary>
    /// avaliação da loja de 0 a 5
    /// </summary>
    public int avaliacao 
    { 
        get; 
        set {
            // calcula a média da lista de avaliações 
            //if (ListaDeAvaliacoes.Count > 0)
            //{
            //    return (int)Math.Round(ListaDeAvaliacoes.Average(r => r.Avaliacao));
            //}
            //return 0;
        }; 
    }
    

    // ========== RELACIONAMENTOS ==========
    
    // A loja pertence a um vendedor
    public virtual ApplicationUser Vendedor { get; set; } = new IList<Store>();

    // A loja tem pedidos 
    public IList<Order> ListaDePedidos { get; set; } = new IList<Order>();

    // A loja tem avaliações
    public IList<Review> ListaDeAvaliacoes { get; set; } = new IList<Review>();

    // A loja tem produtos
    public IList<Product> ListaDeProdutos { get; set; } = new IList<Product>();
}