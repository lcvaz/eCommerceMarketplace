### Comandos de instalação de pacotes:
```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.11
dotnet add package Caelum.Stella.CSharp
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

```
dotnet ef migrations add InitialCreate 

**O que vai acontecer:**
- EF Core analisa todas as suas classes
- Analisa o ApplicationDbContext
- Gera código C# que representa as tabelas
- Cria uma pasta `Migrations/` com os arquivos


```
```

### ASP.NET Core Identity:
```
Link documentação: https://learn.microsoft.com/pt-br/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio

É uma API que suporta a funcionalidade de logon da interface do usuário (UI).
Gerencia usuários, senhas, dados de perfil, funções, declarações, tokens, confirmação por email, podem usar um provedor de logon externo...

Normalmente, o Identity é configurado usando um banco de dados do SQL Server para armazenar nomes de usuários, senhas e dados de perfil. Como alternativa, você pode usar outro armazenamento persistente, por exemplo, o Armazenamento de Tabelas do Azure.

O ASP.NET Core Identity adiciona a funcionalidade de logon da interface do usuário aos aplicativos Web do ASP.NET Core. Para proteger APIs Web e SPAs, use uma das seguintes opções:

Microsoft Entra ID
Azure Active Directory B2C (Azure AD B2C)
Servidor Identity Duende
O Duende Identity Server é uma estrutura do OpenID Connect e OAuth 2.0 para ASP.NET Core. O Duende Identity Server habilita os seguintes recursos de segurança:

AaaS (autenticação como serviço)
SSO (logon único) em vários tipos de aplicativo
Controle de acesso para APIs
Portal de Federação


Razor é uma sintaxe para criar marcação de templates, enquanto Blazor é um framework web que utiliza os componentes Razor para construir aplicações de página única (SPAs) com C#


Sobre uma IdentityUser Classe:

https://learn.microsoft.com/pt-br/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuser?view=aspnetcore-1.1

Sobre uma IdentityDbContext Classe: 

https://learn.microsoft.com/pt-br/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identitydbcontext?view=aspnetcore-9.0
```


### Fluxo Completo de Criação usando IdentityDbContext
```
1. Program.cs pede: new ApplicationDbContext(options)
                                              ↓
2. Construtor recebe options → passa para base(options)
                                              ↓
3. IdentityDbContext recebe → configura conexão com banco
                                              ↓
4. ApplicationDbContext fica pronto para usar
```

### Usando índices 

SELECT * FROM Orders WHERE OrderNumber = 'PED-2025-001234';

-- Mas agora o banco usa o índice:
-- 1. Olha no índice: OrderNumber 'PED-2025-001234' → Linha 1234
-- 2. Vai DIRETO na linha 1234
-- Tempo: 0.01 segundos! ⚡

### 🔍 Entendendo a Connection String
```
Server=.;
   ↑
   Servidor local (. = localhost)

Database=EcommerceMarketplaceDB;
   ↑
   Nome do banco de dados que será criado

Trusted_Connection=True;
   ↑
   Usar autenticação do Windows (sem usuário/senha)

MultipleActiveResultSets=True;
   ↑
   Permite múltiplas queries simultâneas

TrustServerCertificate=True
   ↑
   Confia no certificado do SQL Server (necessário para localhost)
   
```


### O Que É Um Controller?
```
Um Controller é uma classe que:

Recebe requisições HTTP (GET, POST, PUT, DELETE)
Processa a lógica de negócio
Retorna uma resposta (View, JSON, Redirect)
```


### Aplicação rodando em http://localhost:5005