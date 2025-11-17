using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EcommerceMarketplace.Data;
using EcommerceMarketplace.Models;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO DO BANCO DE DADOS ==========

// Adiciona o DbContext com PostgreSQL/Supabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== CONFIGURAÇÃO DO IDENTITY ==========

// Adiciona Identity com configurações personalizadas
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configurações de Senha
    options.Password.RequireDigit = true;           // Exige números
    options.Password.RequireLowercase = true;       // Exige letras minúsculas
    options.Password.RequireUppercase = true;       // Exige letras maiúsculas
    options.Password.RequireNonAlphanumeric = true; // Exige caracteres especiais
    options.Password.RequiredLength = 8;            // Tamanho mínimo
    
    // Configurações de Lockout (bloqueio após tentativas falhas)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configurações de Usuário
    options.User.RequireUniqueEmail = true;         // Email único
    
    // Configurações de Sign In
    options.SignIn.RequireConfirmedEmail = false;   // Mudar para true em produção
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuração de Cookies de Autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";           // Rota de login
    options.LogoutPath = "/Account/Logout";         // Rota de logout
    options.AccessDeniedPath = "/Account/AccessDenied"; // Rota de acesso negado
    options.ExpireTimeSpan = TimeSpan.FromDays(7);  // Cookie expira em 7 dias
    options.SlidingExpiration = true;               // Renova o cookie automaticamente
});

// ========== OUTROS SERVIÇOS ==========

// Adiciona suporte a Controllers com Views (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ========== CONFIGURAÇÃO DO PIPELINE HTTP ==========

// Página de erro amigável em produção
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ========== IMPORTANTE: Ordem correta! ==========
app.UseAuthentication();  // ← DEVE vir ANTES de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();