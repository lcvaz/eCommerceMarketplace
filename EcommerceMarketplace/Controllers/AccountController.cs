using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using EcommerceMarketplace.Models;
using EcommerceMarketplace.ViewModels;
using EcommerceMarketplace.Enums;

namespace EcommerceMarketplace.Controllers;
public class AccountController : Controller // Importa Controller de Mvc
{
    // 1. Campos privados para guardar as dependências
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;


    public AccountController
    (
         
        UserManager<ApplicationUser> userManager, // gerencia operações de usuário (lógica)
        SignInManager<ApplicationUser> signInManager
    )
    {
        _userManager = userManager; 
        _signInManager = signInManager;

    }


    /// <summary>
    /// No meu caso aqui GET só RETORNA uma View (HTML vazio), não faz operações lentas como banco de dados.
    /// </summary>


    /// IActionResult é uma interface que representa qualquer tipo de resposta HTTP que um Controller pode retornar.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // 1. Verifica se o modelo é válido
        if (!ModelState.IsValid) // valida automaticamente baseado nas Data Annotations do RegisterViewModel.
        {
            return View(model);  // Retorna com erros de validação
        }

        // 2. Cria um novo usuário
        var user = new ApplicationUser
        {
            UserName = model.Email, 
            Email = model.Email,
            FullName = model.FullName,
            CPF = model.CPF,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Tenta criar o usuário no banco
        var result = await _userManager.CreateAsync(user, model.Password);
        // Posrteriormente criar Repository Pattern - Separar Acesso ao Banco

        // 4. Se deu certo
        if (result.Succeeded)
        {
            // 4.1. Adiciona tipo de papel 
            var roleName = model.AccountType.ToString();  // "Cliente" ou "Vendedor"
            await _userManager.AddToRoleAsync(user, roleName);

            // 4.2. Faz login automático
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 4.3. Redireciona para Home
            return RedirectToAction("Index", "Home");
        }

        // 5. Se deu erro, adiciona os erros no ModelState
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // 6. Retorna para o formulário com os erros
        return View(model);
    }


    [HttpGet]
    public IActionResult Register() 
    {
        return View();
    }

    // Vantagens de usar interface é que você pode usá-las como tipo podendo representar várias classes de coisas 
    // diferentes porém que implementam a mesma interface



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            // Buscar o usuário para verificar a role
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user != null)
            {
                // Verificar se é Admin
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");  // Dashboard Admin
                }
                
                // Verificar se é Vendedor
                if (await _userManager.IsInRoleAsync(user, "Vendedor"))
                {
                    return RedirectToAction("Dashboard", "Vendor");  // Dashboard Vendedor
                }
                
                // Se não é Admin nem Vendedor, é Cliente
                return RedirectToAction("Index", "Home");  // Home/Dashboard Cliente
            }
            
            // Fallback: se não encontrou usuário (improvável)
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, 
                "Conta bloqueada por excesso de tentativas. Tente novamente em 5 minutos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email ou senha incorretos.");
        return View(model);
    }



    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]  // previne Cross-Site Request Forgery: como um token de segurança que prova que o POST veio do seu próprio site, não de um site malicioso.  
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync(); // Deleta o cookie de autenticação
        return RedirectToAction("Index", "Home");
    }



    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

 
}