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

---

## 🆕 NOVAS FUNCIONALIDADES IMPLEMENTADAS

### 📧 Sistema de Confirmação de Pagamento via Email

#### O QUE FOI IMPLEMENTADO?

Um sistema completo de confirmação de pagamento por email que garante que apenas pedidos confirmados pelos clientes tenham seus estoques subtraídos.

---

### 🔄 FLUXO COMPLETO DE COMPRA

#### 1️⃣ Cliente Faz Checkout
- Cliente adiciona produtos ao carrinho
- Preenche dados pessoais e endereço de entrega
- Escolhe forma de pagamento (PIX, Boleto ou Cartão)
- Clica em "Finalizar Pedido"

#### 2️⃣ Sistema Cria o Pedido
**Arquivo**: `CheckoutController.cs` (linha 233-248)

```csharp
// Cria o pedido com status "Pending" (Aguardando Pagamento)
var order = new Order
{
    OrderNumber = orderNumber,        // Ex: PED-2025-000001
    Status = OrderStatus.Pending,     // Status inicial
    TotalAmount = model.Total,
    // ... outros dados
};
```

**IMPORTANTE**: Nesta etapa, o estoque **NÃO** é subtraído ainda!

#### 3️⃣ Sistema Gera Token de Confirmação
**Arquivo**: `CheckoutController.cs` (linha 291-312)

```csharp
// Gera um token único (GUID) com validade de 24 horas
var token = new PaymentConfirmationToken
{
    Token = Guid.NewGuid().ToString("N"),  // Token único
    OrderId = order.Id,
    ExpiresAt = DateTime.UtcNow.AddHours(24),  // Expira em 24h
    IsUsed = false
};
```

**O que é um GUID?**
- GUID = Globally Unique Identifier (Identificador Único Global)
- É uma string de 32 caracteres hexadecimais
- Tem 2^128 combinações possíveis (praticamente impossível de adivinhar)
- Exemplo: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`

#### 4️⃣ Sistema Envia Email de Confirmação
**Arquivo**: `EmailService.cs` (linha 63-173)

O email contém:
- ✅ Informações do pedido (número, valor total)
- ✅ Botão para confirmar pagamento
- ✅ Link com o token: `/Payment/Confirm?token=ABC123...`
- ✅ Aviso de validade (24 horas)
- ✅ Layout profissional em HTML

**Exemplo de Email Enviado:**

```
┌─────────────────────────────────────────┐
│       🛍️ Pedido Realizado!              │
│   Confirme seu pagamento para finalizar │
└─────────────────────────────────────────┘

Olá João Silva,

Recebemos seu pedido e estamos quase lá! Para finalizar
sua compra, precisamos que você confirme o pagamento.

📦 Detalhes do Pedido
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Número do Pedido:  PED-2025-000001
Valor Total:       R$ 8.000,00
Status:            AGUARDANDO CONFIRMAÇÃO

┌─────────────────────────────────────────┐
│     ✅ CONFIRMAR PAGAMENTO              │
└─────────────────────────────────────────┘

⏰ Importante: Este link é válido por 24 horas.
```

#### 5️⃣ Cliente Clica no Link do Email
**Arquivo**: `PaymentController.cs` (linha 55-191)

Quando o cliente clica no botão "Confirmar Pagamento", ele é redirecionado para:
```
http://localhost:5005/Payment/Confirm?token=a1b2c3d4e5f6...
```

#### 6️⃣ Sistema Valida o Token

O sistema faz 6 validações de segurança:

**Validação 1**: Token foi fornecido?
```csharp
if (string.IsNullOrEmpty(token))
    → Erro: "O link de confirmação está incompleto"
```

**Validação 2**: Token existe no banco de dados?
```csharp
var paymentToken = await _context.PaymentConfirmationTokens
    .FirstOrDefaultAsync(t => t.Token == token);

if (paymentToken == null)
    → Erro: "Este link de confirmação não é válido"
```

**Validação 3**: Token já foi usado antes?
```csharp
if (paymentToken.IsUsed)
    → Erro: "O pagamento já foi confirmado anteriormente"
```

**Validação 4**: Token expirou (mais de 24 horas)?
```csharp
if (paymentToken.IsExpired)  // DateTime.UtcNow > ExpiresAt
    → Erro: "Este link de confirmação expirou"
```

**Validação 5**: Pedido ainda está "Pending"?
```csharp
if (order.Status != OrderStatus.Pending)
    → Erro: "O pedido já foi processado anteriormente"
```

**Validação 6**: Produtos ainda têm estoque?
```csharp
foreach (var orderItem in order.OrderItems)
{
    if (product.Stock < orderItem.Quantity)
        → Erro: "Estoque insuficiente"
}
```

#### 7️⃣ Sistema Confirma o Pagamento
**Arquivo**: `PaymentController.cs` (linha 155-175)

Se TODAS as validações passarem:

```csharp
// 1. Atualiza status do pedido
order.Status = OrderStatus.PaymentConfirmed;
order.PaidAt = DateTime.UtcNow;

// 2. AGORA SIM: Subtrai o estoque dos produtos
foreach (var orderItem in order.OrderItems)
{
    product.Stock -= orderItem.Quantity;

    // Se estoque zerou, marca produto como "Sem Estoque"
    if (product.Stock <= 0)
    {
        product.Status = ProductStatus.OutOfStock;
    }
}

// 3. Marca token como usado (não pode ser usado novamente)
paymentToken.IsUsed = true;
paymentToken.UsedAt = DateTime.UtcNow;

// 4. Salva tudo no banco de dados
await _context.SaveChangesAsync();
```

#### 8️⃣ Cliente Vê Página de Sucesso
**Arquivo**: `Views/Payment/Success.cshtml`

```
✅ Pagamento Confirmado!

Olá João Silva, seu pagamento foi confirmado com sucesso!

📦 Número do Pedido: PED-2025-000001
💰 Valor Total: R$ 8.000,00

Próximos Passos:
1. Seu pedido já está sendo processado
2. Os produtos foram reservados
3. Você receberá o código de rastreamento em breve
```

---

### 📂 ARQUIVOS CRIADOS E MODIFICADOS

#### Novos Arquivos

1. **Services/IEmailService.cs**
   - Interface que define os métodos de envio de email
   - Documenta como enviar emails genéricos e emails de confirmação

2. **Services/EmailService.cs** (242 linhas)
   - Implementação completa do serviço de email usando SMTP
   - Envia emails HTML bonitos e profissionais
   - Tratamento completo de erros
   - Logs detalhados para debug

3. **Models/PaymentConfirmationToken.cs** (107 linhas)
   - Modelo que armazena tokens de confirmação
   - Propriedades: Token, OrderId, CreatedAt, ExpiresAt, IsUsed, UsedAt
   - Propriedades calculadas: IsExpired, IsValid
   - Totalmente documentado com explicações

4. **Controllers/PaymentController.cs** (191 linhas)
   - Controller responsável por confirmar pagamentos
   - Faz todas as 6 validações de segurança
   - Subtrai estoque somente após confirmação
   - Marca produtos como "Sem Estoque" quando necessário
   - Logs detalhados de todas as operações

5. **Views/Payment/** (5 arquivos)
   - `Success.cshtml` - Página de sucesso após confirmação
   - `Error.cshtml` - Página de erro genérico
   - `AlreadyConfirmed.cshtml` - Token já foi usado
   - `Expired.cshtml` - Token expirado
   - `InsufficientStock.cshtml` - Produtos sem estoque

#### Arquivos Modificados

1. **appsettings.json**
   - Adicionada seção `EmailSettings` com configurações SMTP
   - Configurado para usar Ethereal (email de teste) em desenvolvimento

2. **Data/ApplicationDbContext.cs**
   - Adicionado `DbSet<PaymentConfirmationToken>`
   - Configurado relacionamento Order → PaymentConfirmationTokens
   - Criados índices para performance (Token único, OrderId)

3. **Controllers/CheckoutController.cs**
   - Injetado `IEmailService` no construtor
   - **REMOVIDO**: Código que subtraía estoque no checkout
   - **ADICIONADO**: Criação de token de confirmação
   - **ADICIONADO**: Envio de email de confirmação
   - Tratamento de erros de email

4. **Views/Checkout/Confirmation.cshtml**
   - **REMOVIDO**: Linha 4 das instruções PIX ("Confirme o pagamento de R$ 0,00")
   - Agora mostra apenas 3 instruções

5. **Program.cs**
   - Registrado `EmailService` como Scoped no container de DI
   - Adicionada documentação sobre Dependency Injection Lifetime

---

### 🔐 SEGURANÇA IMPLEMENTADA

#### 1. Tokens Únicos e Imprevisíveis
- Usa GUID (2^128 combinações possíveis)
- Impossível de adivinhar por força bruta
- Formato limpo para URLs (sem hífens)

#### 2. Expiração Automática
- Tokens expiram em 24 horas
- Verificação feita via propriedade calculada `IsExpired`
- Usa `DateTime.UtcNow` (UTC = horário universal)

#### 3. Uso Único
- Cada token só pode ser usado uma vez
- `IsUsed = true` marca token como consumido
- Tentativas de reusar mostram mensagem apropriada

#### 4. Validação de Estoque em Tempo Real
- Verifica estoque novamente no momento da confirmação
- Previne venda de produtos sem estoque
- Mostra mensagem específica se estoque acabou

#### 5. Validação de Status do Pedido
- Apenas pedidos "Pending" podem ser confirmados
- Evita dupla confirmação de pagamento
- Mantém integridade dos dados

---

### 📊 BANCO DE DADOS

#### Nova Tabela: PaymentConfirmationTokens

```sql
CREATE TABLE PaymentConfirmationTokens (
    Id              INT PRIMARY KEY IDENTITY,
    Token           NVARCHAR(100) NOT NULL UNIQUE,  -- Token único
    OrderId         INT NOT NULL,                   -- FK para Orders
    CreatedAt       DATETIME2 NOT NULL,
    ExpiresAt       DATETIME2 NOT NULL,
    IsUsed          BIT NOT NULL DEFAULT 0,
    UsedAt          DATETIME2 NULL,

    -- Índices para performance
    INDEX IX_PaymentConfirmationTokens_Token (Token) UNIQUE,
    INDEX IX_PaymentConfirmationTokens_OrderId (OrderId),

    -- Chave estrangeira
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
```

#### Relacionamentos

```
Orders (1) ──────→ (N) PaymentConfirmationTokens
   │
   ├─ Um pedido pode ter vários tokens
   │  (caso o cliente peça reenvio do email)
   │
   └─ Normalmente tem apenas 1 token
```

---

### ⚙️ CONFIGURAÇÃO DE EMAIL

#### Desenvolvimento (Usando Ethereal)

**Arquivo**: `appsettings.json`

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.ethereal.email",
    "SmtpPort": "587",
    "SenderEmail": "dev@marketplace.com",
    "SenderName": "eCommerce Marketplace",
    "Username": "dev@marketplace.com",
    "Password": "dev123456"
  }
}
```

**O que é Ethereal?**
- Serviço de email falso para testes
- NÃO envia emails reais
- Captura emails e permite visualizar no navegador
- Perfeito para desenvolvimento

#### Produção (Usando Gmail, SendGrid, AWS SES, etc.)

**Para usar Gmail:**

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SenderEmail": "seu-email@gmail.com",
    "SenderName": "eCommerce Marketplace",
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app"
  }
}
```

**IMPORTANTE**:
- Gmail requer "App Password" (não use sua senha normal)
- Ative autenticação de 2 fatores
- Gere uma senha de app em: https://myaccount.google.com/apppasswords

**Para usar SendGrid:**

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": "587",
    "SenderEmail": "noreply@seudominio.com",
    "SenderName": "eCommerce Marketplace",
    "Username": "apikey",
    "Password": "SUA_API_KEY_DO_SENDGRID"
  }
}
```

---

### 🧪 TESTANDO O SISTEMA

#### Passo 1: Criar Migration

```bash
cd EcommerceMarketplace
dotnet ef migrations add AddPaymentConfirmationTokens
dotnet ef database update
```

#### Passo 2: Executar a Aplicação

```bash
dotnet run
```

#### Passo 3: Fazer um Pedido

1. Acesse: http://localhost:5005
2. Adicione produtos ao carrinho
3. Vá para o checkout
4. Preencha os dados e finalize
5. Verifique os logs no console

#### Passo 4: Verificar Email (Desenvolvimento)

**Como o Ethereal não envia emails reais**, você verá nos logs:

```
info: EcommerceMarketplace.Services.EmailService[0]
      Enviando email de confirmação de pedido para joao@email.com. Pedido: PED-2025-000001

info: EcommerceMarketplace.Services.EmailService[0]
      Email de confirmação enviado com sucesso para joao@email.com
```

#### Passo 5: Simular Clique no Link

Copie o token dos logs e acesse:
```
http://localhost:5005/Payment/Confirm?token=SEU_TOKEN_AQUI
```

#### Passo 6: Verificar Estoque

1. Vá para o dashboard do vendedor
2. Veja que o estoque foi subtraído
3. Produtos com estoque zero ficam "OutOfStock"

---

### 🐛 TRATAMENTO DE ERROS

Todos os possíveis erros são tratados com mensagens claras:

| Erro | Mensagem ao Usuário |
|------|---------------------|
| Token não fornecido | "O link de confirmação está incompleto" |
| Token inválido | "Este link de confirmação não é válido" |
| Token já usado | "O pagamento já foi confirmado em DD/MM/YYYY" |
| Token expirado | "Este link expirou em DD/MM/YYYY. Entre em contato" |
| Pedido já processado | "O pedido já foi processado. Status: [status]" |
| Estoque insuficiente | Lista de produtos sem estoque disponível |
| Erro no banco de dados | "Erro ao processar. Tente novamente mais tarde" |
| Erro ao enviar email | "Pedido criado, mas houve problema no email. Contate suporte" |

---

### 📈 MELHORIAS FUTURAS SUGERIDAS

1. **Dashboard em Tempo Real**
   - SignalR para atualizar dashboard automaticamente
   - Notificações push quando pedido for confirmado
   - Gráficos de vendas em tempo real

2. **Notificações por WhatsApp**
   - Integração com Twilio ou API oficial do WhatsApp
   - Enviar confirmação também via WhatsApp
   - Mais efetivo que email

3. **Sistema de Fila de Emails**
   - RabbitMQ ou Azure Service Bus
   - Reenviar automaticamente emails que falharam
   - Melhor performance em picos de tráfego

4. **Integração com Gateway de Pagamento**
   - Mercado Pago
   - Stripe
   - PagSeguro
   - Confirmação automática após pagamento aprovado

5. **Relatórios para Vendedores**
   - PDF com pedidos confirmados
   - Exportar para Excel
   - Gráficos de performance

---

### 💡 CONCEITOS IMPORTANTES EXPLICADOS

#### O que é Dependency Injection?

É um padrão onde as dependências de uma classe são "injetadas" automaticamente,
ao invés de serem criadas manualmente dentro da classe.

**Sem DI (Ruim):**
```csharp
public class CheckoutController
{
    private EmailService _emailService;

    public CheckoutController()
    {
        _emailService = new EmailService();  // ❌ Acoplamento forte
    }
}
```

**Com DI (Bom):**
```csharp
public class CheckoutController
{
    private IEmailService _emailService;

    public CheckoutController(IEmailService emailService)  // ✅ Injetado
    {
        _emailService = emailService;
    }
}
```

**Vantagens:**
- ✅ Facilita testes (pode injetar versões fake)
- ✅ Reduz acoplamento entre classes
- ✅ Facilita trocar implementações

#### O que é SMTP?

SMTP = Simple Mail Transfer Protocol (Protocolo Simples de Transferência de Email)

É o protocolo padrão usado para enviar emails na internet. Funciona assim:

```
Sua Aplicação  →  Servidor SMTP  →  Servidor de Email  →  Destinatário
                  (Gmail, SendGrid)   (Gmail do cliente)
```

#### O que é UTC?

UTC = Coordinated Universal Time (Tempo Universal Coordenado)

É o horário de referência mundial, sem fuso horário.

**Por que usar?**
- ✅ Evita problemas com horário de verão
- ✅ Funciona em qualquer país
- ✅ Facilita cálculos de tempo

**Conversão:**
```csharp
DateTime.UtcNow              → "2025-11-26 14:30:00" (UTC)
DateTime.Now                 → "2025-11-26 11:30:00" (Brasília = UTC-3)
```

---

### 🎓 CONCLUSÃO

Este sistema implementa um fluxo profissional e seguro de confirmação de pagamento que:

✅ **Protege o estoque** - Só subtrai após confirmação real do cliente
✅ **É seguro** - Usa tokens únicos, expira automaticamente, uso único
✅ **Tem UX excelente** - Emails bonitos, mensagens de erro claras
✅ **É escalável** - Pode adicionar filas, notificações, etc
✅ **É documentado** - Todo código tem comentários explicativos
✅ **Segue boas práticas** - DI, separação de responsabilidades, tratamento de erros

---