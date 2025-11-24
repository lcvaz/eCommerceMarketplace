using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.Data;

/// <summary>
/// Classe responsável por popular dados iniciais no banco
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Inicializa o banco de dados com roles e dados de exemplo
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Passo 1: Criar as roles
        await CreateRolesAsync(roleManager);

        // Passo 2: Criar usuário vendedor de exemplo
        var vendor = await CreateVendorUserAsync(userManager);

        // Passo 3: Criar loja de exemplo
        var store = await CreateStoreAsync(context, vendor);

        // Passo 4: Criar categorias
        var categories = await CreateCategoriesAsync(context);

        // Passo 5: Criar produtos de exemplo
        await CreateProductsAsync(context, store, categories);
    }

    /// <summary>
    /// Cria as roles padrão do sistema se não existirem
    /// </summary>
    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "Vendedor", "Cliente" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    /// <summary>
    /// Cria um usuário vendedor de exemplo se não existir
    /// </summary>
    private static async Task<ApplicationUser> CreateVendorUserAsync(UserManager<ApplicationUser> userManager)
    {
        // Verifica se já existe um vendedor de exemplo
        var existingVendor = await userManager.FindByEmailAsync("vendedor@example.com");
        
        if (existingVendor != null)
        {
            return existingVendor;
        }

        // Cria novo vendedor
        var vendor = new ApplicationUser
        {
            UserName = "vendedor@example.com",
            Email = "vendedor@example.com",
            FullName = "Maria Silva",
            CPF = "123.456.789-00",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(vendor, "Vendedor@123");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(vendor, "Vendedor");
            return vendor;
        }

        throw new Exception("Falha ao criar usuário vendedor de exemplo.");
    }

    /// <summary>
    /// Cria uma loja de exemplo se não existir
    /// </summary>
    private static async Task<Store> CreateStoreAsync(ApplicationDbContext context, ApplicationUser vendor)
    {
        // Verifica se já existe uma loja para este vendedor
        var existingStore = await context.Stores
            .FirstOrDefaultAsync(s => s.VendorId == vendor.Id);
        
        if (existingStore != null)
        {
            return existingStore;
        }

        // Cria nova loja
        var store = new Store
        {
            Name = "Papelaria Artesanal",
            Description = "Produtos artesanais únicos e exclusivos para você",
            CNPJ = "12.345.678/0001-90",
            Status = StoreStatus.Active,
            VendorId = vendor.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Stores.Add(store);
        await context.SaveChangesAsync();

        return store;
    }

    /// <summary>
    /// Cria categorias de exemplo se não existirem
    /// </summary>
    private static async Task<Dictionary<string, Category>> CreateCategoriesAsync(ApplicationDbContext context)
    {
        var categoryNames = new[] { "Cadernos", "Papéis de presente", "Cartões", "Impressões" };
        var categories = new Dictionary<string, Category>();

        foreach (var name in categoryNames)
        {
            // Verifica se a categoria já existe
            var existingCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == name);

            if (existingCategory != null)
            {
                categories[name] = existingCategory;
            }
            else
            {
                // Cria nova categoria
                var category = new Category
                {
                    Name = name,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                categories[name] = category;
            }
        }

        return categories;
    }

    /// <summary>
    /// Cria produtos de exemplo se não existirem
    /// </summary>
    private static async Task CreateProductsAsync(
        ApplicationDbContext context, 
        Store store, 
        Dictionary<string, Category> categories)
    {
        // Verifica se já existem produtos nesta loja
        var existingProducts = await context.Products
            .Where(p => p.StoreId == store.Id)
            .CountAsync();

        if (existingProducts > 0)
        {
            // Já existem produtos, não precisa criar
            return;
        }

        // Lista de produtos de exemplo
        var products = new List<Product>
        {
            new Product
            {
                Name = "Caderno Minimalista A5",
                Description = "Caderno com capa em tons pastéis. 120 páginas pautadas. Perfeito para anotações diárias, bullet journal ou planejamento. Papel de alta qualidade que não borra com caneta.",
                Price = 35.90M,
                Stock = 15,
                SKU = "CAD-MIN-A5-001",
                ImageUrl = "https://images.unsplash.com/photo-1531346878377-a5be20888e57?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Cadernos"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Papel de Presente Botânico",
                Description = "Papel reciclado com estampas de folhas. Sustentável e elegante para todos os tipos de presentes. Medida: 70x100cm. Pacote com 3 folhas.",
                Price = 12.50M,
                Stock = 30,
                SKU = "PAP-BOT-001",
                ImageUrl = "https://images.unsplash.com/photo-1513885535751-8b9238bd345a?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Papéis de presente"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Kit Cartões Florais",
                Description = "Conjunto com 6 cartões e envelopes. Designs florais exclusivos pintados à mão. Perfeito para ocasiões especiais como aniversários, agradecimentos e celebrações.",
                Price = 28.00M,
                Stock = 20,
                SKU = "CAR-FLO-KIT-001",
                ImageUrl = "https://images.unsplash.com/photo-1524721696987-b9527df9e512?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Cartões"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Impressão Abstract Art",
                Description = "Arte abstrata em papel algodão 300g. Peça única para decoração sofisticada. Dimensões: 30x40cm. Cores vibrantes e design contemporâneo.",
                Price = 45.00M,
                Stock = 8,
                SKU = "IMP-ABS-001",
                ImageUrl = "https://images.unsplash.com/photo-1541961017774-22349e4a1262?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Impressões"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Caderno de Desenho A4",
                Description = "Caderno profissional para desenhos e esboços. 80 folhas de papel especial 180g/m². Capa dura resistente. Ideal para artistas e designers.",
                Price = 52.90M,
                Stock = 12,
                SKU = "CAD-DES-A4-001",
                ImageUrl = "https://images.unsplash.com/photo-1517842645767-c639042777db?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Cadernos"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Papel Kraft Rústico",
                Description = "Papel kraft natural para embrulhos rústicos e charmosos. Rolo com 10 metros x 50cm. Textura única e resistente.",
                Price = 18.00M,
                Stock = 25,
                SKU = "PAP-KRA-001",
                ImageUrl = "https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Papéis de presente"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Cartões de Agradecimento",
                Description = "Set de 10 cartões minimalistas para agradecimentos. Design clean e elegante. Envelopes inclusos. Papel premium 250g/m².",
                Price = 22.50M,
                Stock = 18,
                SKU = "CAR-AGR-001",
                ImageUrl = "https://images.unsplash.com/photo-1519052537078-e6302a4968d4?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Cartões"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Poster Geométrico",
                Description = "Impressão de design geométrico moderno. Papel fosco de alta qualidade. Tamanho: 50x70cm. Emoldurado opcionalmente.",
                Price = 38.00M,
                Stock = 10,
                SKU = "IMP-GEO-001",
                ImageUrl = "https://images.unsplash.com/photo-1549887534-1541e9326642?w=500",
                Status = ProductStatus.Available,
                StoreId = store.Id,
                CategoryId = categories["Impressões"].Id,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            }
        };

        // Adiciona todos os produtos ao contexto
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        Console.WriteLine($"✓ {products.Count} produtos criados com sucesso!");
    }
}