using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Models;

namespace EcommerceMarketplace.Data;

/// <summary>
/// ApplicationDbContext é o contexto do banco de dados.
/// Ele herda de IdentityDbContext para incluir as tabelas do ASP.NET Identity
/// (Users, Roles, Claims, etc).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // entre os sinais <> é um parâmetro de tipo - "Substitua TUser por ApplicationUser em TUDO"
{
    // Herdando IdentityDbContext<ApplicationUser> já estou criando a tabela de usuário atribuindo ApplicationUser a ela
    // Dentro de IdentityDbContext já é criado public DbSet<TUser> Users { get; set; }  -> TUser é substituído por ApplicationUser
    //public DbSet<IdentityRole> Roles { get; set; }
    //public DbSet<IdentityUserRole> UserRoles { get; set; }
    //public DbSet<IdentityUserClaim> UserClaims { get; set; }
    //public DbSet<IdentityUserLogin> UserLogins { get; set; }
    //public DbSet<IdentityUserToken> UserTokens { get; set; }
    //public DbSet<IdentityRoleClaim> RoleClaims { get; set; } 

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
    // O que significa : (dois pontos)? Chama o construtor da classe pai (base class). 
    // Neste caso estamos chamando o construtor da classe pai IdentityDbContext passando o parâmetro options


    // ========== DBSETS ==========
    // Cada DbSet representa uma tabela no banco de dados
    
    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<ReviewProduct> ReviewsProduct { get; set; }
    public DbSet<ReviewProductImage> ReviewProductImages { get; set; }
    public DbSet<ReviewStore> ReviewsStore { get; set; }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }

    public DbSet<PaymentConfirmationToken> PaymentConfirmationTokens { get; set; }

    // ========== CONFIGURAÇÕES DE RELACIONAMENTOS ==========
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANTE: Chama a configuração base do Identity
        base.OnModelCreating(modelBuilder);

        // ===== CONFIGURAÇÕES DE PRECISÃO DECIMAL =====
        // Garante que todos os decimais tenham precisão correta
        
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.SubtotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.ShippingAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        // ===== RELACIONAMENTO: ApplicationUser → Cart (1:1) =====
        // Um usuário tem UM carrinho
        
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Cart)
            .WithOne(c => c.Customer)
            .HasForeignKey<Cart>(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar user, deleta carrinho

        // ===== RELACIONAMENTO: ApplicationUser → Stores (1:N) =====
        // Um vendedor pode ter várias lojas
        
        modelBuilder.Entity<Store>()
            .HasOne(s => s.Vendor)
            .WithMany(u => u.Stores)
            .HasForeignKey(s => s.VendorId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar vendedor que tem lojas

        // ===== RELACIONAMENTO: ApplicationUser → Orders (1:N) =====
        // Um cliente pode ter vários pedidos
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar cliente que tem pedidos

        // ===== RELACIONAMENTO: ApplicationUser ↔ Address (N:M via CustomerAddress) =====
        // Um cliente pode ter vários endereços e um endereço pode pertencer a vários clientes

        modelBuilder.Entity<CustomerAddress>()
            .HasOne(ca => ca.Customer)
            .WithMany(u => u.CustomerAddresses)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar user, deleta associações

        modelBuilder.Entity<CustomerAddress>()
            .HasOne(ca => ca.Address)
            .WithMany(a => a.CustomerAddresses)
            .HasForeignKey(ca => ca.AddressId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar endereço, deleta associações

        // ===== RELACIONAMENTO: ApplicationUser → ReviewsProduct (1:N) =====
        // Um cliente pode fazer várias reviews de produtos
        
        modelBuilder.Entity<ReviewProduct>()
            .HasOne(r => r.Customer)
            .WithMany(u => u.ReviewsProduct)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar cliente que tem reviews

        // ===== RELACIONAMENTO: ApplicationUser → ReviewsStore (1:N) =====
        // Um cliente pode fazer várias reviews de lojas
        
        modelBuilder.Entity<ReviewStore>()
            .HasOne(r => r.Customer)
            .WithMany(u => u.ReviewsStore)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar cliente que tem reviews

        // ===== RELACIONAMENTO: Store → Products (1:N) =====
        // Uma loja pode ter vários produtos
        
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar loja, deleta produtos

        // ===== RELACIONAMENTO: Store → Address (N:1) =====
        // Uma loja tem um endereço, um endereço pode ter várias lojas

        modelBuilder.Entity<Store>()
            .HasOne(s => s.Address)
            .WithMany(a => a.Stores)
            .HasForeignKey(s => s.AddressId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar endereço que tem lojas

        // ===== RELACIONAMENTO: Store → ReviewsStore (1:N) =====
        // Uma loja pode ter várias reviews

        modelBuilder.Entity<ReviewStore>()
            .HasOne(r => r.Store)
            .WithMany(s => s.ReviewsStore)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar loja, deleta reviews

        // ===== RELACIONAMENTO: Category → Products (1:N) =====
        // Uma categoria pode ter vários produtos
        
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);  // Se deletar categoria, produtos ficam sem categoria

        // ===== RELACIONAMENTO: Category → SubCategories (Auto-relacionamento) =====
        // Uma categoria pode ter várias subcategorias
        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar categoria pai que tem filhos

        // ===== RELACIONAMENTO: Product → ReviewsProduct (1:N) =====
        // Um produto pode ter várias reviews
        
        modelBuilder.Entity<ReviewProduct>()
            .HasOne(r => r.Product)
            .WithMany(p => p.ReviewsProduct)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar produto, deleta reviews

        // ===== RELACIONAMENTO: ReviewProduct → Images (1:N) =====
        // Uma review pode ter várias imagens
        
        modelBuilder.Entity<ReviewProductImage>()
            .HasOne(img => img.ReviewProduct)
            .WithMany(r => r.Images)
            .HasForeignKey(img => img.ReviewProductId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar review, deleta imagens

        // ===== RELACIONAMENTO: Product → CartItems (1:N) =====
        // Um produto pode estar em vários carrinhos (através de CartItem)
        
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar produto, remove do carrinho

        // ===== RELACIONAMENTO: Cart → CartItems (1:N) =====
        // Um carrinho pode ter vários itens
        
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar carrinho, deleta itens

        // ===== RELACIONAMENTO: Product → OrderItems (1:N) =====
        // Um produto pode estar em vários pedidos (através de OrderItem)
        
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar produto que está em pedidos

        // ===== RELACIONAMENTO: Order → OrderItems (1:N) =====
        // Um pedido pode ter vários itens
        
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar pedido, deleta itens

        // ===== RELACIONAMENTO: Order → Address (N:1) =====
        // Um pedido tem UM endereço de entrega

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShippingAddress)
            .WithMany(a => a.Orders)
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);  // Não pode deletar endereço usado em pedidos

        // ===== RELACIONAMENTO: Order → PaymentConfirmationTokens (1:N) =====
        // Um pedido pode ter vários tokens de confirmação (caso o cliente peça reenvio)
        // mas normalmente terá apenas um

        modelBuilder.Entity<PaymentConfirmationToken>()
            .HasOne(t => t.Order)
            .WithMany()  // Order não precisa ter navegação para tokens
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);  // Se deletar pedido, deleta tokens

        // ===== ÍNDICES PARA PERFORMANCE =====
        
        // Índice em Email (buscas frequentes)
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email) // ← Cria índice em Email
            .IsUnique(); // ← Garante que não repete

        // Índice em CPF (buscas por documento)
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.CPF);

        // Índice em CNPJ da loja
        modelBuilder.Entity<Store>()
            .HasIndex(s => s.CNPJ);

        // Índice em SKU do produto (buscas frequentes)
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU);

        // Índice em OrderNumber (buscas por número do pedido)
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        // Índice em CreatedAt dos pedidos (relatórios por data)
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt);

        // Índice em Status dos pedidos (filtros frequentes)
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);

        // Índice em CEP (buscas por região)
        modelBuilder.Entity<Address>()
            .HasIndex(a => a.ZipCode);

        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.CustomerId, o.CreatedAt }); // Buscar pedidos de um cliente por data fica rápido

        // Índice em Token (buscas para validação de pagamento)
        modelBuilder.Entity<PaymentConfirmationToken>()
            .HasIndex(t => t.Token)
            .IsUnique();  // Cada token é único

        // Índice em OrderId (buscar tokens de um pedido)
        modelBuilder.Entity<PaymentConfirmationToken>()
            .HasIndex(t => t.OrderId);


        // ===== SEED DATA (OPCIONAL) =====
        // Você pode adicionar dados iniciais aqui no futuro
        // Por exemplo: categorias padrão, usuário admin, etc
    }
}