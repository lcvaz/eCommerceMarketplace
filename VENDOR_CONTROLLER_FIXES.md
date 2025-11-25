# Correções para VendorController - Erros de Compilação

## Propriedades do Modelo Store

### ✅ Propriedades DISPONÍVEIS em Store.cs:
```csharp
// Propriedades básicas
public int Id { get; set; }
public string Name { get; set; }
public string? Description { get; set; }
public string? LogoUrl { get; set; }
public string? CNPJ { get; set; }
public DateTime CreatedAt { get; set; }
public StoreStatus Status { get; set; }

// Foreign Key
public string VendorId { get; set; }  // ← Use esta, NÃO "OwnerId"

// Navigation Properties
public ApplicationUser Vendor { get; set; }
public ICollection<Product> Products { get; set; }
public ICollection<ReviewStore> ReviewsStore { get; set; }

// Propriedades Calculadas (SOMENTE LEITURA - não podem ser atribuídas)
public double AverageRating { get; } // ← CALCULADA automaticamente
public int TotalReviews { get; }     // ← CALCULADA automaticamente
```

### ❌ Propriedades INEXISTENTES (causando erros):
- `Phone` - NÃO EXISTE
- `ContactEmail` - NÃO EXISTE
- `Address` - NÃO EXISTE
- `City` - NÃO EXISTE
- `State` - NÃO EXISTE
- `ZipCode` - NÃO EXISTE
- `OwnerId` - NÃO EXISTE (use `VendorId`)
- `UpdatedAt` - NÃO EXISTE

---

## Correções Necessárias nas Linhas 319-336

### Erro 1-6: Propriedades de contato e endereço

**PROBLEMA** (linhas 319-326):
```csharp
// ❌ CÓDIGO ERRADO - REMOVER ESTAS LINHAS:
store.Phone = model.Phone;                  // Linha 319
store.ContactEmail = model.ContactEmail;    // Linha 320
// ...
store.Address = model.Address;              // Linha 323
store.City = model.City;                    // Linha 324
store.State = model.State;                  // Linha 325
store.ZipCode = model.ZipCode;              // Linha 326
```

**SOLUÇÃO**:
Remova todas essas linhas. O modelo `Store` atual **não possui** campos de contato e endereço.

Se você precisa dessas informações, você tem duas opções:

**Opção A:** Adicionar as propriedades ao modelo Store.cs:
```csharp
// Adicionar em Store.cs:
[Phone]
public string? Phone { get; set; }

[EmailAddress]
public string? ContactEmail { get; set; }

[StringLength(200)]
public string? Address { get; set; }

[StringLength(100)]
public string? City { get; set; }

[StringLength(2)]
public string? State { get; set; }

[StringLength(10)]
[RegularExpression(@"^\d{5}-?\d{3}$")]
public string? ZipCode { get; set; }

public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
```

**Opção B (Recomendada):** Simplesmente remover essas linhas do VendorController se não forem essenciais agora.

---

### Erro 7: OwnerId → VendorId

**PROBLEMA** (linha 329):
```csharp
// ❌ CÓDIGO ERRADO:
store.OwnerId = vendorId;
```

**SOLUÇÃO**:
```csharp
// ✅ CÓDIGO CORRETO:
store.VendorId = vendorId;  // Use VendorId, não OwnerId
```

---

### Erro 8-9: Propriedades Calculadas (Somente Leitura)

**PROBLEMA** (linhas 333-334):
```csharp
// ❌ CÓDIGO ERRADO - REMOVER ESTAS LINHAS:
store.AverageRating = 0;  // Linha 333 - NÃO pode atribuir (é calculada)
store.TotalReviews = 0;   // Linha 334 - NÃO pode atribuir (é calculada)
```

**SOLUÇÃO**:
Remova essas linhas. `AverageRating` e `TotalReviews` são propriedades **calculadas automaticamente** com base nas reviews. Você **não pode** atribuir valores a elas.

---

### Erro 10: UpdatedAt não existe

**PROBLEMA** (linha 336):
```csharp
// ❌ CÓDIGO ERRADO:
store.UpdatedAt = DateTime.UtcNow;
```

**SOLUÇÃO**:
Remova esta linha, ou adicione a propriedade `UpdatedAt` ao modelo Store.cs (ver Opção A acima).

---

## Código Corrigido - Exemplo

```csharp
// ✅ EXEMPLO DE CÓDIGO CORRETO (linhas 310-340 aproximadamente):
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateStore(CreateStoreViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var vendorId = _userManager.GetUserId(User);

    var store = new Store
    {
        Name = model.Name,
        Description = model.Description,
        LogoUrl = model.LogoUrl,
        CNPJ = model.CNPJ,
        CreatedAt = DateTime.UtcNow,
        Status = StoreStatus.Active,
        VendorId = vendorId  // ✅ Use VendorId, não OwnerId
    };

    // ✅ NÃO tente atribuir:
    // - Phone, ContactEmail, Address, City, State, ZipCode (não existem)
    // - AverageRating, TotalReviews (são calculadas automaticamente)
    // - UpdatedAt (não existe no modelo atual)

    _context.Stores.Add(store);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Loja criada com sucesso!";
    return RedirectToAction("Dashboard");
}
```

---

## Resumo das Ações

1. **Remover linhas 319-326**: Atribuições de Phone, ContactEmail, Address, City, State, ZipCode
2. **Corrigir linha 329**: Trocar `OwnerId` por `VendorId`
3. **Remover linhas 333-334**: Atribuições de AverageRating e TotalReviews
4. **Remover linha 336**: Atribuição de UpdatedAt

OU

1. **Adicionar as propriedades faltantes** ao modelo Store.cs (ver Opção A)
2. **Manter apenas** as correções de OwnerId → VendorId
3. **Remover** as atribuições de AverageRating e TotalReviews (sempre)

---

## ⚠️ Importante

Após fazer as correções:
1. Execute `dotnet build` para verificar se os erros foram resolvidos
2. Se ainda houver erros, verifique se há outros locais usando propriedades inexistentes
3. Faça commit das alterações com mensagem descritiva
