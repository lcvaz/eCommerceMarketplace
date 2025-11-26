# E-Commerce Marketplace

Sistema completo de marketplace desenvolvido em ASP.NET Core MVC com Entity Framework Core, ASP.NET Identity e PostgreSQL/Supabase.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Arquitetura do Sistema](#arquitetura-do-sistema)
- [Estrutura do Banco de Dados](#estrutura-do-banco-de-dados)
- [Models - Entidades](#models---entidades)
- [Controllers - Lógica de Negócio](#controllers---lógica-de-negócio)
- [ViewModels - Transferência de Dados](#viewmodels---transferência-de-dados)
- [Enums - Tipos Enumerados](#enums---tipos-enumerados)
- [Sistema de Autenticação](#sistema-de-autenticação)
- [Funcionalidades Principais](#funcionalidades-principais)
- [Configuração e Instalação](#configuração-e-instalação)
- [Variáveis Importantes](#variáveis-importantes)

---

## 🎯 Visão Geral

Sistema de marketplace que permite:
- **Clientes**: Navegar produtos, adicionar ao carrinho, fazer pedidos e avaliar produtos/lojas
- **Vendedores**: Criar lojas, gerenciar produtos, visualizar vendas e métricas
- **Administradores**: Gerenciar todo o sistema (funcionalidade planejada)

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM para acesso ao banco de dados
- **ASP.NET Core Identity** - Sistema de autenticação e autorização
- **PostgreSQL + Npgsql** - Banco de dados relacional

### Pacotes NuGet
```bash
Microsoft.EntityFrameworkCore.SqlServer v8.0.11
Microsoft.EntityFrameworkCore.Design v8.0.11
Microsoft.AspNetCore.Identity.EntityFrameworkCore v8.0.11
Microsoft.AspNetCore.Identity.UI v8.0.11
Npgsql.EntityFrameworkCore.PostgreSQL
Caelum.Stella.CSharp
```

### Frontend
- **Razor Views** - Engine de templates
- **Bootstrap 5** - Framework CSS
- **jQuery** - Biblioteca JavaScript

---

## 🏗️ Arquitetura do Sistema

### Padrão MVC (Model-View-Controller)

```
EcommerceMarketplace/
├── Models/              # Entidades do banco de dados
├── Controllers/         # Lógica de negócio e rotas
├── Views/              # Interface do usuário (Razor)
├── ViewModels/         # DTOs para transferência de dados
├── Data/               # DbContext e configurações do banco
├── Enums/              # Tipos enumerados
├── Migrations/         # Migrações do Entity Framework
└── wwwroot/            # Arquivos estáticos (CSS, JS, imagens)
```

---

## 🗄️ Estrutura do Banco de Dados

### Diagrama de Relacionamentos

```
ApplicationUser (Identity)
├── 1:N → Stores (Vendedor possui lojas)
├── 1:N → Orders (Cliente faz pedidos)
├── 1:N → Addresses (Cliente tem endereços)
├── 1:N → ReviewsProduct (Cliente avalia produtos)
├── 1:N → ReviewsStore (Cliente avalia lojas)
└── 1:1 → Cart (Cliente tem um carrinho)

Store
├── 1:N → Products (Loja possui produtos)
├── 1:N → ReviewsStore (Loja recebe avaliações)
└── N:1 → Address (Loja tem um endereço)

Product
├── 1:N → CartItems (Produto em carrinhos)
├── 1:N → OrderItems (Produto em pedidos)
├── 1:N → ReviewsProduct (Produto recebe avaliações)
├── N:1 → Store (Produto pertence a uma loja)
└── N:1 → Category (Produto tem uma categoria)

Order
├── 1:N → OrderItems (Pedido contém itens)
├── N:1 → ApplicationUser (Cliente do pedido)
└── N:1 → Address (Endereço de entrega)

Cart
├── 1:N → CartItems (Carrinho contém itens)
└── 1:1 → ApplicationUser (Carrinho do cliente)

Category
├── 1:N → Products (Categoria agrupa produtos)
└── 1:N → SubCategories (Hierarquia de categorias)
```

---

## 📦 Models - Entidades

### ApplicationUser
**Localização**: `EcommerceMarketplace/Models/ApplicationUser.cs`

Representa todos os usuários do sistema (Clientes, Vendedores, Admins).

**Propriedades Principais**:
- `Id` (string) - Identificador único herdado do IdentityUser
- `Email` (string) - Email do usuário (herdado)
- `UserName` (string) - Nome de usuário (herdado)
- `PasswordHash` (string) - Hash da senha (herdado)
- `FullName` (string) - Nome completo do usuário
- `CPF` (string?) - CPF brasileiro (formato: 000.000.000-00)
- `CreatedAt` (DateTime) - Data de criação da conta

**Relacionamentos**:
- `Stores` (ICollection<Store>) - Lojas do vendedor
- `Orders` (ICollection<Order>) - Pedidos do cliente
- `Addresses` (ICollection<Address>) - Endereços do cliente
- `ReviewsProduct` (ICollection<ReviewProduct>) - Avaliações de produtos
- `ReviewsStore` (ICollection<ReviewStore>) - Avaliações de lojas
- `Cart` (Cart?) - Carrinho de compras do cliente

---

### Store
**Localização**: `EcommerceMarketplace/Models/Store.cs`

Representa uma loja dentro do marketplace.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Name` (string) - Nome da loja
- `Description` (string?) - Descrição da loja
- `LogoUrl` (string?) - URL do logo
- `CNPJ` (string?) - CNPJ da loja (formato: 00.000.000/0000-00)
- `Phone` (string?) - Telefone de contato
- `ContactEmail` (string?) - Email de contato
- `Status` (StoreStatus) - Status da loja (Active, Inactive, Suspended)
- `CreatedAt` (DateTime) - Data de criação
- `UpdatedAt` (DateTime) - Última atualização
- `VendorId` (string) - FK para ApplicationUser
- `AddressId` (int) - FK para Address

**Propriedades Calculadas**:
- `AverageRating` (double) - Média das avaliações [NotMapped]
- `TotalReviews` (int) - Total de avaliações [NotMapped]

**Relacionamentos**:
- `Vendor` (ApplicationUser) - Dono da loja
- `Address` (Address) - Endereço da loja
- `Products` (ICollection<Product>) - Produtos da loja
- `ReviewsStore` (ICollection<ReviewStore>) - Avaliações recebidas

---

### Product
**Localização**: `EcommerceMarketplace/Models/Product.cs`

Representa um produto vendido por uma loja.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Name` (string) - Nome do produto
- `Description` (string?) - Descrição detalhada
- `Price` (decimal) - Preço unitário (18,2 precision)
- `Stock` (int) - Quantidade em estoque
- `SKU` (string) - Código único do produto
- `ImageUrl` (string) - URL da imagem principal
- `Status` (ProductStatus) - Status (Available, OutOfStock, Discontinued, Draft)
- `CreatedAt` (DateTime) - Data de criação
- `ModifiedAt` (DateTime) - Última modificação
- `StoreId` (int) - FK para Store
- `CategoryId` (int?) - FK para Category (opcional)

**Propriedades Calculadas**:
- `AverageRating` (double) - Média das avaliações [NotMapped]
- `TotalReviews` (int) - Total de avaliações [NotMapped]
- `IsAvailable` (bool) - Se está disponível para venda [NotMapped]

**Relacionamentos**:
- `Store` (Store) - Loja dona do produto
- `Category` (Category?) - Categoria do produto
- `ReviewsProduct` (ICollection<ReviewProduct>) - Avaliações
- `OrderItems` (ICollection<OrderItem>) - Itens em pedidos
- `CartItems` (ICollection<CartItem>) - Itens em carrinhos

---

### Category
**Localização**: `EcommerceMarketplace/Models/Category.cs`

Representa uma categoria de produtos com suporte a hierarquia.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Name` (string) - Nome da categoria
- `Description` (string?) - Descrição
- `Slug` (string?) - URL amigável (ex: "eletronicos")
- `IconUrl` (string?) - URL do ícone
- `ParentCategoryId` (int?) - FK para categoria pai (hierarquia)
- `IsActive` (bool) - Se está ativa
- `CreatedAt` (DateTime) - Data de criação

**Relacionamentos**:
- `ParentCategory` (Category?) - Categoria pai
- `SubCategories` (ICollection<Category>) - Subcategorias
- `Products` (ICollection<Product>) - Produtos da categoria

---

### Order
**Localização**: `EcommerceMarketplace/Models/Order.cs`

Representa um pedido de compra feito por um cliente.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `OrderNumber` (string) - Número visível (ex: "PED-2025-001234")
- `Status` (OrderStatus) - Status do pedido
- `SubtotalAmount` (decimal) - Valor dos produtos
- `ShippingAmount` (decimal) - Valor do frete
- `DiscountAmount` (decimal) - Descontos aplicados
- `TotalAmount` (decimal) - Valor total final
- `PaymentMethod` (string) - Método de pagamento
- `CreatedAt` (DateTime) - Data de criação
- `PaidAt` (DateTime?) - Data do pagamento
- `ShippedAt` (DateTime?) - Data do envio
- `DeliveredAt` (DateTime?) - Data da entrega
- `CanceledAt` (DateTime?) - Data do cancelamento
- `CancellationReason` (string?) - Motivo do cancelamento
- `TrackingCode` (string?) - Código de rastreio
- `Notes` (string?) - Observações do cliente
- `CustomerId` (string) - FK para ApplicationUser
- `ShippingAddressId` (int) - FK para Address

**Propriedades Calculadas**:
- `TotalItems` (int) - Total de itens [NotMapped]
- `IsPaid` (bool) - Se foi pago [NotMapped]
- `IsDelivered` (bool) - Se foi entregue [NotMapped]
- `IsCanceled` (bool) - Se foi cancelado [NotMapped]

**Relacionamentos**:
- `Customer` (ApplicationUser) - Cliente do pedido
- `ShippingAddress` (Address) - Endereço de entrega
- `OrderItems` (ICollection<OrderItem>) - Itens do pedido

---

### OrderItem
**Localização**: `EcommerceMarketplace/Models/OrderItem.cs`

Representa um produto dentro de um pedido.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Quantity` (int) - Quantidade comprada
- `UnitPrice` (decimal) - Preço unitário no momento da compra
- `DiscountAmount` (decimal) - Desconto aplicado
- `OrderId` (int) - FK para Order
- `ProductId` (int) - FK para Product

**Propriedades Calculadas**:
- `Subtotal` (decimal) - (Quantity × UnitPrice) - DiscountAmount [NotMapped]

**Relacionamentos**:
- `Order` (Order) - Pedido pai
- `Product` (Product) - Produto comprado

---

### Cart
**Localização**: `EcommerceMarketplace/Models/Cart.cs`

Representa o carrinho de compras de um cliente. Cada cliente tem UM carrinho.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `CreatedAt` (DateTime) - Data de criação
- `UpdatedAt` (DateTime) - Última atualização
- `CustomerId` (string) - FK para ApplicationUser

**Propriedades Calculadas**:
- `TotalItems` (int) - Total de itens [NotMapped]
- `TotalAmount` (decimal) - Valor total [NotMapped]
- `IsEmpty` (bool) - Se está vazio [NotMapped]

**Relacionamentos**:
- `Customer` (ApplicationUser) - Cliente dono do carrinho
- `CartItems` (ICollection<CartItem>) - Itens no carrinho

---

### CartItem
**Localização**: `EcommerceMarketplace/Models/CartItem.cs`

Representa um produto dentro do carrinho.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Quantity` (int) - Quantidade
- `UnitPrice` (decimal) - Preço unitário (salvo para histórico)
- `AddedAt` (DateTime) - Quando foi adicionado
- `CartId` (int) - FK para Cart
- `ProductId` (int) - FK para Product

**Propriedades Calculadas**:
- `Subtotal` (decimal) - Quantity × UnitPrice [NotMapped]

**Relacionamentos**:
- `Cart` (Cart) - Carrinho pai
- `Product` (Product) - Produto no carrinho

---

### Address
**Localização**: `EcommerceMarketplace/Models/Address.cs`

Representa um endereço brasileiro. Pode ser usado por clientes e lojas.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `ZipCode` (string) - CEP (formato: 00000-000)
- `Street` (string) - Rua/Avenida
- `Number` (string) - Número
- `Complement` (string?) - Complemento (Apto, Bloco, etc)
- `Neighborhood` (string) - Bairro
- `City` (string) - Cidade
- `State` (string) - Estado/UF (2 letras)
- `Country` (string) - País (padrão: "Brasil")
- `CreatedAt` (DateTime) - Data de criação
- `UpdatedAt` (DateTime) - Última atualização
- `CustomerId` (string) - FK para ApplicationUser

**Relacionamentos**:
- `Customer` (ApplicationUser) - Cliente dono do endereço
- `Stores` (ICollection<Store>) - Lojas que usam este endereço
- `Orders` (ICollection<Order>) - Pedidos que usaram este endereço

---

### ReviewProduct
**Localização**: `EcommerceMarketplace/Models/ReviewProduct.cs`

Representa uma avaliação de produto feita por um cliente.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Rating` (int) - Nota de 1 a 5
- `Title` (string?) - Título da avaliação
- `Comment` (string?) - Comentário
- `CreatedAt` (DateTime) - Data da avaliação
- `ProductId` (int) - FK para Product
- `CustomerId` (string) - FK para ApplicationUser

**Relacionamentos**:
- `Product` (Product) - Produto avaliado
- `Customer` (ApplicationUser) - Cliente que avaliou
- `Images` (ICollection<ReviewProductImage>) - Imagens da avaliação

---

### ReviewStore
**Localização**: `EcommerceMarketplace/Models/ReviewStore.cs`

Representa uma avaliação de loja feita por um cliente.

**Propriedades Principais**:
- `Id` (int) - Identificador único
- `Rating` (int) - Nota de 1 a 5
- `Comment` (string?) - Comentário
- `CreatedAt` (DateTime) - Data da avaliação
- `StoreId` (int) - FK para Store
- `CustomerId` (string) - FK para ApplicationUser

**Relacionamentos**:
- `Store` (Store) - Loja avaliada
- `Customer` (ApplicationUser) - Cliente que avaliou

---

## 🎮 Controllers - Lógica de Negócio

### HomeController
**Localização**: `EcommerceMarketplace/Controllers/HomeController.cs`

Responsável pela página inicial e navegação pública.

**Actions**:
- `Index()` - GET
  - **Linha**: 25
  - **Descrição**: Exibe a página inicial com produtos e categorias em destaque
  - **Query**: Busca 4 categorias principais e 12 produtos disponíveis
  - **Retorna**: `HomeViewModel`

- `Privacy()` - GET
  - **Linha**: 48
  - **Descrição**: Exibe página de política de privacidade

- `Error()` - GET
  - **Linha**: 55
  - **Descrição**: Exibe página de erro
  - **Retorna**: `ErrorViewModel`

**Variáveis Importantes**:
- `_context` (ApplicationDbContext) - Acesso ao banco de dados
- `_logger` (ILogger) - Sistema de logs

---

### AccountController
**Localização**: `EcommerceMarketplace/Controllers/AccountController.cs`

Gerencia autenticação, registro e login de usuários.

**Actions**:

#### Register (GET)
- **Linha**: 85
- **Descrição**: Exibe formulário de registro
- **Rota**: `/Account/Register`

#### Register (POST)
- **Linha**: 37
- **Descrição**: Processa o registro de novo usuário
- **Validações**:
  - ModelState válido
  - Email único
  - Senha forte (8 caracteres, maiúscula, minúscula, número, caractere especial)
- **Fluxo**:
  1. Valida dados (linha 40)
  2. Cria ApplicationUser (linha 46)
  3. Salva no banco via UserManager (linha 56)
  4. Adiciona role (Cliente ou Vendedor) (linha 64)
  5. Faz login automático (linha 67)
  6. Redireciona para Home (linha 70)
- **Variável**: `model` (RegisterViewModel)

#### Login (GET)
- **Linha**: 152
- **Descrição**: Exibe formulário de login
- **Rota**: `/Account/Login`

#### Login (POST)
- **Linha**: 97
- **Descrição**: Processa o login do usuário
- **Validações**: Email e senha corretos
- **Fluxo**:
  1. Valida dados (linha 99)
  2. Tenta autenticar (linha 104)
  3. Verifica role do usuário (linha 114)
  4. Redireciona baseado na role:
     - Admin → `/Admin/Index` (linha 119)
     - Vendedor → `/Vendor/Dashboard` (linha 125)
     - Cliente → `/Home/Index` (linha 131)
- **Variável**: `model` (LoginViewModel)
- **Proteções**: Lockout após 5 tentativas falhas (linha 108)

#### Logout (POST)
- **Linha**: 160
- **Descrição**: Faz logout do usuário
- **Segurança**: [ValidateAntiForgeryToken]
- **Fluxo**: Deleta cookie de autenticação (linha 161)

#### AccessDenied (GET)
- **Linha**: 176
- **Descrição**: Exibe página de acesso negado
- **Rota**: `/Account/AccessDenied`

**Variáveis Importantes**:
- `_userManager` (UserManager<ApplicationUser>) - Gerencia operações de usuário
- `_signInManager` (SignInManager<ApplicationUser>) - Gerencia login/logout

---

### ProductController
**Localização**: `EcommerceMarketplace/Controllers/ProductController.cs`

Gerencia visualização de produtos pelos clientes.

**Actions**:

#### Details (GET)
- **Linha**: 42
- **Descrição**: Exibe página de detalhes de um produto
- **Rota**: `/Product/Details/{id}`
- **Parâmetro**: `id` (int) - ID do produto
- **Query Complexa** (linha 46-51):
  ```csharp
  Include(p => p.Store)
  Include(p => p.Category)
  Include(p => p.ReviewsProduct)
      .ThenInclude(r => r.Customer)
  ```
- **Retorna**: `ProductDetailViewModel` com:
  - Dados do produto
  - Informações da loja
  - Categoria
  - 5 avaliações mais recentes (linha 89-92)
  - Média de avaliações
- **Erro**: Retorna 404 se produto não existe (linha 57)

**Variáveis Importantes**:
- `_context` (ApplicationDbContext)
- `_logger` (ILogger)
- `product` (Product) - Produto carregado do banco

---

### CartController
**Localização**: `EcommerceMarketplace/Controllers/CartController.cs`

Gerencia o carrinho de compras. **Requer autenticação** ([Authorize] linha 15).

**Actions**:

#### Index (GET)
- **Linha**: 36
- **Descrição**: Exibe página do carrinho
- **Rota**: `/Cart/Index`
- **Query**: Carrega carrinho com itens, produtos e lojas (linha 41-45)
- **Retorna**: `CartViewModel`

#### AddToCart (POST)
- **Linha**: 78
- **Descrição**: Adiciona produto ao carrinho
- **Parâmetros**:
  - `productId` (int)
  - `quantity` (int, padrão: 1)
- **Validações Críticas**:
  - Quantidade > 0 (linha 83)
  - Produto existe (linha 94)
  - Produto disponível (linha 101)
  - **Estoque suficiente** (linha 108)
  - **Não exceder estoque ao atualizar** (linha 132)
- **Fluxo**:
  1. Busca produto (linha 90)
  2. Verifica estoque (linha 108)
  3. Busca/cria carrinho (linha 115)
  4. Verifica se produto já está no carrinho (linha 123)
  5. Atualiza quantidade OU adiciona novo item (linha 126-154)
  6. Salva no banco (linha 161)
- **Mensagens**: TempData["SuccessMessage"] ou TempData["ErrorMessage"]

#### UpdateQuantity (POST)
- **Linha**: 179
- **Descrição**: Atualiza quantidade de um item (AJAX)
- **Parâmetros**:
  - `cartItemId` (int)
  - `quantity` (int)
- **Validações**:
  - Quantidade > 0 (linha 184)
  - Item existe (linha 194)
  - **Pertence ao usuário atual** (linha 201)
  - **Não exceder estoque** (linha 207)
- **Retorna**: JSON com sucesso/erro (linha 224-229)

#### RemoveItem (POST)
- **Linha**: 243
- **Descrição**: Remove item do carrinho
- **Parâmetro**: `cartItemId` (int)
- **Validações**:
  - Item existe (linha 252)
  - Pertence ao usuário (linha 259)
- **Fluxo**: Remove do contexto e salva (linha 269-272)

#### Clear (POST)
- **Linha**: 292
- **Descrição**: Limpa todo o carrinho
- **Fluxo**: Remove todos os CartItems (linha 304)

#### GetOrCreateUserCartAsync (Private)
- **Linha**: 326
- **Descrição**: Busca carrinho existente ou cria um novo
- **Lógica**: Um usuário tem apenas UM carrinho (linha 336-353)

**Variáveis Importantes**:
- `_context` (ApplicationDbContext)
- `_userManager` (UserManager)
- `userId` - ID do usuário logado (obtido via `_userManager.GetUserId(User)`)

---

### VendorController
**Localização**: `EcommerceMarketplace/Controllers/VendorController.cs`

Gerencia funcionalidades do vendedor. **Requer role "Vendedor"** ([Authorize(Roles = "Vendedor")] linha 20).

**Actions**:

#### Dashboard (GET)
- **Linha**: 55
- **Descrição**: Dashboard principal do vendedor com métricas
- **Rota**: `/Vendor/Dashboard`
- **Queries Complexas**:
  1. Busca lojas do vendedor (linha 75)
  2. Conta produtos (linha 97)
  3. **Busca itens vendidos** (linha 110-115):
     - Filtra por lojas do vendedor
     - **Apenas pedidos entregues** (OrderStatus.Delivered)
  4. Calcula total de vendas (pedidos únicos) (linha 126-129)
  5. Calcula receita total (linha 138-139)
  6. **Produto mais vendido últimos 3 meses** (linha 194-215):
     - GroupBy por produto
     - Sum de quantidades
     - OrderByDescending
- **Retorna**: `VendorDashboardViewModel` com:
  - Total de produtos
  - Total de vendas
  - Receita total
  - Cards das lojas com métricas individuais
  - Produto mais vendido

#### CreateStore (GET)
- **Linha**: 257
- **Descrição**: Exibe formulário de criação de loja
- **Rota**: `/Vendor/CreateStore`
- **Retorna**: View com `CreateStoreViewModel` vazio

#### CreateStore (POST)
- **Linha**: 280
- **Descrição**: Processa criação de nova loja
- **Validações**: ModelState.IsValid (linha 287)
- **Fluxo Importante**:
  1. Identifica vendedor (linha 299)
  2. **Verifica endereço existente** (linha 312-320):
     - Reutiliza se encontrar idêntico
     - Cria novo se não existir (linha 333-350)
  3. Cria Store (linha 356-378)
  4. Salva no banco (linha 382-383)
  5. TempData de sucesso (linha 391)
  6. Redireciona para Dashboard (linha 397)

#### ManageProducts (GET)
- **Linha**: 424
- **Descrição**: Lista produtos de uma loja
- **Rota**: `/Vendor/ManageProducts?storeId={id}`
- **Parâmetro**: `storeId` (int)
- **Validações**:
  - Loja existe (linha 437)
  - **Loja pertence ao vendedor** (linha 438)
- **Query**: Produtos com Category, ordenados por data (linha 448-452)
- **Retorna**: `ManageProductsViewModel`

#### CreateProduct (GET)
- **Linha**: 477
- **Descrição**: Exibe formulário de criação de produto
- **Rota**: `/Vendor/CreateProduct?storeId={id}`
- **Validações**: Loja existe e pertence ao vendedor (linha 489)
- **Fluxo**:
  1. Busca categorias ativas (linha 500)
  2. Passa via ViewBag (linha 506)
  3. ViewModel com StoreId preenchido (linha 510)

#### CreateProduct (POST)
- **Linha**: 535
- **Descrição**: Processa criação de produto
- **Validações**:
  - ModelState válido (linha 538)
  - Loja existe e pertence ao vendedor (linha 568)
  - **SKU único na loja** (linha 579-595)
- **Fluxo**:
  1. Verifica SKU duplicado (linha 579)
  2. Cria Product (linha 599-612)
  3. Salva no banco (linha 615-616)
  4. Redireciona para ManageProducts (linha 624)

**Variáveis Importantes**:
- `_context` (ApplicationDbContext)
- `_userManager` (UserManager)
- `vendorId` - ID do vendedor logado
- `storeIds` - Lista de IDs das lojas do vendedor
- `completedOrderItems` - Itens de pedidos entregues
- `threeMonthsAgo` - Data de 3 meses atrás para relatórios

---

## 📊 ViewModels - Transferência de Dados

### HomeViewModel
**Localização**: `EcommerceMarketplace/ViewModels/HomeViewModel.cs`

Usado na página inicial.

**Propriedades**:
- `FeaturedCategories` (List<Category>) - Categorias em destaque
- `FeaturedProducts` (List<Product>) - Produtos em destaque

---

### RegisterViewModel
**Localização**: `EcommerceMarketplace/ViewModels/RegisterViewModel.cs`

Formulário de registro de usuário.

**Propriedades**:
- `Email` (string) - [Required, EmailAddress]
- `Password` (string) - [Required, StringLength, DataType(Password)]
- `ConfirmPassword` (string) - [Compare("Password")]
- `FullName` (string) - [Required]
- `CPF` (string?) - Opcional
- `AccountType` (AccountType) - Enum: Cliente ou Vendedor

---

### LoginViewModel
**Localização**: `EcommerceMarketplace/ViewModels/LoginViewModel.cs`

Formulário de login.

**Propriedades**:
- `Email` (string) - [Required, EmailAddress]
- `Password` (string) - [Required, DataType(Password)]
- `RememberMe` (bool) - "Lembrar-me"

---

### ProductDetailViewModel
**Localização**: `EcommerceMarketplace/ViewModels/ProductDetailViewModel.cs`

Detalhes de um produto.

**Propriedades**:
- Dados do produto (Id, Name, Description, Price, Stock, ImageUrl, Status)
- Dados da categoria (CategoryName)
- Dados da loja (StoreName, StoreId, StoreLogoUrl, StoreAverageRating, StoreTotalReviews)
- Avaliações (AverageRating, TotalReviews, RecentReviews)

---

### CartViewModel
**Localização**: `EcommerceMarketplace/ViewModels/CartViewModel.cs`

Carrinho de compras.

**Propriedades**:
- `CartId` (int)
- `Items` (List<CartItemViewModel>)
- `TotalItems` (int) - Calculado
- `TotalAmount` (decimal) - Calculado

---

### CartItemViewModel
**Nested em CartViewModel**

Item do carrinho.

**Propriedades**:
- `CartItemId` (int)
- `ProductId` (int)
- `ProductName` (string)
- `ProductImageUrl` (string)
- `UnitPrice` (decimal)
- `Quantity` (int)
- `AvailableStock` (int)
- `StoreName` (string)
- `StoreId` (int)
- `Subtotal` (decimal) - Calculado

---

### VendorDashboardViewModel
**Localização**: `EcommerceMarketplace/ViewModels/VendorDashboardViewModel.cs`

Dashboard do vendedor.

**Propriedades**:
- `TotalProducts` (int)
- `TotalSales` (int)
- `TotalRevenue` (decimal)
- `Stores` (List<StoreCardViewModel>)
- `TopProduct` (TopProductViewModel?)
- `HasStores` (bool) - Calculado

---

### StoreCardViewModel
**Localização**: `EcommerceMarketplace/ViewModels/StoreCardViewModel.cs`

Card de loja no dashboard.

**Propriedades**:
- `Id` (int)
- `Name` (string)
- `Description` (string?)
- `LogoUrl` (string?)
- `Status` (string)
- `Sales` (int)
- `Revenue` (decimal)

---

### TopProductViewModel
**Localização**: `EcommerceMarketplace/ViewModels/TopProductViewModel.cs`

Produto mais vendido.

**Propriedades**:
- `ProductId` (int)
- `ProductName` (string)
- `ImageUrl` (string)
- `UnitsSold` (int)
- `RevenueGenerated` (decimal)

---

### CreateStoreViewModel
**Localização**: `EcommerceMarketplace/ViewModels/CreateStoreViewModel.cs`

Formulário de criação de loja.

**Propriedades**:
- Dados da loja (Name, Description, Phone, ContactEmail)
- Dados do endereço (ZipCode, Street, Number, Complement, Neighborhood, City, State)

---

### CreateProductViewModel
**Localização**: `EcommerceMarketplace/ViewModels/CreateProductViewModel.cs`

Formulário de criação de produto.

**Propriedades**:
- `StoreId` (int)
- `Name` (string) - [Required]
- `Description` (string?)
- `Price` (decimal) - [Required, Range(0.01, 999999)]
- `Stock` (int) - [Required, Range(0, int.MaxValue)]
- `SKU` (string) - [Required]
- `ImageUrl` (string) - [Required, Url]
- `CategoryId` (int?)
- `Status` (ProductStatus)

---

### ManageProductsViewModel
**Localização**: `EcommerceMarketplace/ViewModels/ManageProductsViewModel.cs`

Lista de produtos de uma loja.

**Propriedades**:
- `StoreId` (int)
- `StoreName` (string)
- `StoreDescription` (string?)
- `StoreLogoUrl` (string?)
- `Products` (List<Product>)

---

## 🔢 Enums - Tipos Enumerados

### AccountType
**Localização**: `EcommerceMarketplace/Enums/AccountType.cs`

Tipo de conta do usuário.

**Valores**:
- `Cliente = 1` - Cliente comprador
- `Vendedor = 2` - Vendedor com lojas

---

### OrderStatus
**Localização**: `EcommerceMarketplace/Enums/OrderStatus.cs`

Status de um pedido.

**Valores**:
- `Pending = 1` - Pendente (aguardando pagamento)
- `PaymentConfirmed = 2` - Pagamento confirmado
- `Processing = 3` - Em processamento (separando produtos)
- `Shipped = 4` - Enviado
- `Delivered = 5` - Entregue
- `Canceled = 6` - Cancelado
- `Returned = 7` - Devolvido

**Usado em**:
- `Order.Status`
- Filtros em queries (ex: VendorController linha 114)

---

### ProductStatus
**Localização**: `EcommerceMarketplace/Enums/ProductStatus.cs`

Status de um produto.

**Valores**:
- `Available = 1` - Disponível para venda
- `OutOfStock = 2` - Sem estoque
- `Discontinued = 3` - Descontinuado
- `Draft = 4` - Rascunho (ainda não publicado)

**Usado em**:
- `Product.Status`
- Validações de disponibilidade (CartController linha 101)

---

### StoreStatus
**Localização**: `EcommerceMarketplace/Enums/StoreStatus.cs`

Status de uma loja.

**Valores**:
- `Active = 1` - Ativa
- `Inactive = 2` - Inativa
- `Suspended = 3` - Suspensa

**Usado em**:
- `Store.Status`

---

## 🔐 Sistema de Autenticação

### Configuração do Identity
**Localização**: `EcommerceMarketplace/Program.cs` (linha 18-40)

**Regras de Senha**:
- Mínimo 8 caracteres (linha 25)
- Requer número (linha 21)
- Requer letra minúscula (linha 22)
- Requer letra maiúscula (linha 23)
- Requer caractere especial (linha 24)

**Lockout**:
- 5 tentativas falhas (linha 29)
- Bloqueio de 5 minutos (linha 28)

**Email**:
- Deve ser único (linha 33)
- Confirmação desabilitada em dev (linha 36)

**Cookies**:
- Login: `/Account/Login` (linha 45)
- Logout: `/Account/Logout` (linha 46)
- Access Denied: `/Account/AccessDenied` (linha 47)
- Expira em 7 dias (linha 48)
- Renovação automática (linha 49)

---

### Roles do Sistema
**Localização**: `EcommerceMarketplace/Data/SeedData.cs` (linha 14-32)

**Roles Criadas Automaticamente**:
1. **Admin** - Administrador do sistema
2. **Vendedor** - Vendedor com lojas
3. **Cliente** - Cliente comprador

**Inicialização**: Chamada em `Program.cs` linha 82-94

---

### Proteção de Rotas

**[Authorize]** - Requer autenticação:
- `CartController` (linha 15)
- `VendorController` (linha 20)

**[Authorize(Roles = "Vendedor")]** - Requer role específica:
- `VendorController` (linha 20)

**[ValidateAntiForgeryToken]** - Proteção CSRF:
- Todos os métodos POST

---

## ⚙️ Funcionalidades Principais

### 1. Gestão de Usuários

#### Registro
- **Controller**: AccountController
- **Action**: Register (POST) - linha 37
- **ViewModel**: RegisterViewModel
- **Fluxo**:
  1. Validação de dados
  2. Criação do ApplicationUser
  3. Hash de senha automático pelo Identity
  4. Atribuição de role (Cliente ou Vendedor)
  5. Login automático
- **Variável de senha**: `model.Password` (linha 56)

#### Login
- **Controller**: AccountController
- **Action**: Login (POST) - linha 97
- **ViewModel**: LoginViewModel
- **Fluxo**:
  1. Validação de credenciais
  2. Autenticação via SignInManager (linha 104)
  3. Verificação de role (linha 114-131)
  4. Redirecionamento baseado em role
- **Variável de email**: `model.Email` (linha 105)
- **Variável de senha**: `model.Password` (linha 106)

---

### 2. Carrinho de Compras

#### Adicionar ao Carrinho
- **Controller**: CartController
- **Action**: AddToCart (POST) - linha 78
- **Validações Críticas**:
  - Produto disponível (linha 101-105)
  - **Estoque suficiente** (linha 108-112)
- **Variáveis**:
  - `productId` - ID do produto
  - `quantity` - Quantidade desejada
  - `product.Stock` - Estoque disponível (linha 108)
  - `existingItem` - Item já no carrinho (linha 123)

#### Verificação de Estoque
- **Localização**: CartController linha 108 e 132
- **Lógica**:
  ```csharp
  if (product.Stock < quantity) // Linha 108
  if (newQuantity > product.Stock) // Linha 132
  ```

#### Gerenciamento de Carrinho
- **Um carrinho por usuário**: CartController linha 326-356
- **Variável**: `cart` (Cart)
- **Criação automática**: Se não existe, cria novo (linha 341-353)

---

### 3. Gestão de Lojas (Vendedor)

#### Dashboard do Vendedor
- **Controller**: VendorController
- **Action**: Dashboard - linha 55
- **Métricas Calculadas**:
  1. **Total de Produtos** (linha 97):
     ```csharp
     var totalProducts = await _context.Products
         .Where(p => storeIds.Contains(p.StoreId))
         .CountAsync();
     ```

  2. **Total de Vendas** (linha 126):
     ```csharp
     var totalSales = completedOrderItems
         .Select(oi => oi.OrderId)
         .Distinct()
         .Count();
     ```

  3. **Receita Total** (linha 138):
     ```csharp
     var totalRevenue = completedOrderItems
         .Sum(oi => oi.Quantity * oi.UnitPrice);
     ```

#### Criar Loja
- **Controller**: VendorController
- **Action**: CreateStore (POST) - linha 280
- **Fluxo**:
  1. Verifica endereço existente (linha 312)
  2. Reutiliza ou cria novo endereço (linha 324-350)
  3. Cria Store vinculada ao vendedor (linha 356)
- **Variáveis**:
  - `vendorId` - ID do vendedor (linha 299)
  - `address` - Endereço da loja (linha 322)
  - `store` - Nova loja (linha 356)

#### Criar Produto
- **Controller**: VendorController
- **Action**: CreateProduct (POST) - linha 535
- **Validações**:
  - **SKU único por loja** (linha 579-595)
  - Loja pertence ao vendedor (linha 568)
- **Variáveis**:
  - `model.SKU` - Código único (linha 580)
  - `existingProduct` - Produto com SKU duplicado (linha 579)

---

### 4. Visualização de Produtos

#### Detalhes do Produto
- **Controller**: ProductController
- **Action**: Details - linha 42
- **Query com Relacionamentos** (linha 46):
  ```csharp
  Include(p => p.Store)
  Include(p => p.Category)
  Include(p => p.ReviewsProduct)
      .ThenInclude(r => r.Customer)
  ```
- **Propriedades Calculadas**:
  - `AverageRating` - Product.cs linha 99
  - `TotalReviews` - Product.cs linha 111
  - `IsAvailable` - Product.cs linha 114

---

## 🔧 Configuração e Instalação

### 1. Requisitos
- .NET 8.0 SDK
- PostgreSQL (ou Supabase)
- Visual Studio 2022 / VS Code / Rider

### 2. Pacotes NuGet
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.11
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.11
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Caelum.Stella.CSharp
```

### 3. Connection String
**Localização**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=seu-host;Database=nome-do-banco;Username=usuario;Password=senha"
  }
}
```

**Configuração**: Program.cs linha 11-13

### 4. Migrations
```bash
# Criar migration inicial
dotnet ef migrations add InitialCreate

# Aplicar migrations ao banco
dotnet ef database update
```

### 5. Executar Aplicação
```bash
dotnet run
```

**URL**: http://localhost:5005 (configurado em launchSettings.json)

---

## 📍 Variáveis Importantes

### Program.cs

**Linha 11**: `builder.Services.AddDbContext<ApplicationDbContext>`
- Registra o contexto do banco de dados

**Linha 18**: `builder.Services.AddIdentity<ApplicationUser, IdentityRole>`
- Configura o sistema de autenticação

**Linha 21-25**: Regras de senha
- `RequireDigit`, `RequireLowercase`, `RequireUppercase`, `RequireNonAlphanumeric`, `RequiredLength`

**Linha 28-30**: Configurações de lockout
- `DefaultLockoutTimeSpan`, `MaxFailedAccessAttempts`, `AllowedForNewUsers`

**Linha 45-49**: Configurações de cookies
- `LoginPath`, `LogoutPath`, `AccessDeniedPath`, `ExpireTimeSpan`, `SlidingExpiration`

---

### ApplicationDbContext.cs

**Linha 31-46**: DbSets (tabelas do banco)
- `Stores`, `Products`, `Categories`, `ReviewsProduct`, `ReviewProductImages`, `ReviewsStore`, `Orders`, `OrderItems`, `Carts`, `CartItems`, `Addresses`

**Linha 58-88**: Precisão decimal
- Todos os campos monetários com `HasPrecision(18, 2)`

**Linha 90-256**: Relacionamentos
- Configurações de Foreign Keys e Delete Behaviors

**Linha 258-295**: Índices
- Otimizações para queries frequentes

---

### CartController.cs

**Linha 108**: Verificação de estoque
```csharp
if (product.Stock < quantity)
```

**Linha 132**: Verificação ao atualizar quantidade
```csharp
if (newQuantity > product.Stock)
```

**Linha 328**: Obter ID do usuário
```csharp
var userId = _userManager.GetUserId(User);
```

---

### VendorController.cs

**Linha 60**: ID do vendedor logado
```csharp
var vendorId = _userManager.GetUserId(User);
```

**Linha 90**: IDs das lojas do vendedor
```csharp
var storeIds = vendorStores.Select(s => s.Id).ToList();
```

**Linha 110-115**: Itens vendidos (apenas entregues)
```csharp
var completedOrderItems = await _context.OrderItems
    .Where(oi => storeIds.Contains(oi.Product.StoreId) &&
                oi.Order.Status == OrderStatus.Delivered)
```

**Linha 185**: Data de 3 meses atrás
```csharp
var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
```

---

### AccountController.cs

**Linha 56**: Criação de usuário
```csharp
var result = await _userManager.CreateAsync(user, model.Password);
```

**Linha 64**: Atribuição de role
```csharp
await _userManager.AddToRoleAsync(user, roleName);
```

**Linha 104**: Autenticação
```csharp
var result = await _signInManager.PasswordSignInAsync(
    model.Email,
    model.Password,
    model.RememberMe,
    lockoutOnFailure: true
);
```

---

## 📚 Padrões de Código

### Convenções de Nomenclatura
- **Controllers**: Sufixo `Controller` (ex: `HomeController`)
- **Models**: Substantivos no singular (ex: `Product`, `Store`)
- **ViewModels**: Sufixo `ViewModel` (ex: `CartViewModel`)
- **Métodos privados**: Prefixo com underscore em campos (ex: `_context`)
- **Métodos assíncronos**: Sufixo `Async` (ex: `GetOrCreateUserCartAsync`)

### Estrutura de Actions
```csharp
[HttpGet/Post]
[Authorize] // Se necessário
[ValidateAntiForgeryToken] // Em POSTs
public async Task<IActionResult> NomeAction(parametros)
{
    // 1. Validações
    // 2. Busca de dados
    // 3. Lógica de negócio
    // 4. Salvar no banco (se necessário)
    // 5. Mensagem de feedback (TempData)
    // 6. Redirecionamento ou retorno de View
}
```

### Tratamento de Erros
- **Try-Catch**: Em operações que podem falhar
- **ModelState.AddModelError**: Para erros de validação
- **TempData**: Para mensagens de sucesso/erro
- **Logging**: `_logger.LogError/LogWarning/LogInformation`

---

## 🎓 Conceitos Importantes

### Entity Framework Core
- **DbSet**: Representa uma tabela no banco
- **Include/ThenInclude**: Carrega relacionamentos (Eager Loading)
- **Migrations**: Controle de versão do banco de dados
- **OnModelCreating**: Configuração de relacionamentos e índices

### ASP.NET Identity
- **UserManager**: Gerencia operações de usuário (criar, atualizar, deletar)
- **SignInManager**: Gerencia login/logout
- **RoleManager**: Gerencia roles (permissões)
- **PasswordHasher**: Hash automático de senhas

### Padrão MVC
- **Model**: Representa dados (entidades do banco)
- **View**: Interface do usuário (arquivos .cshtml)
- **Controller**: Lógica de negócio e roteamento
- **ViewModel**: Dados formatados especificamente para Views

---

## 📝 Notas Finais

Este sistema foi desenvolvido seguindo as melhores práticas de:
- **Segurança**: Proteção CSRF, hash de senhas, validação de propriedade
- **Performance**: Índices no banco, queries otimizadas
- **Manutenibilidade**: Código bem documentado, separação de responsabilidades
- **Escalabilidade**: Arquitetura preparada para crescimento

Para qualquer dúvida sobre uma funcionalidade específica, consulte o arquivo referenciado na documentação acima.
