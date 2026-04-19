---
name: dotnet-refactor
description: Analiza y refactoriza código .NET Core aplicando principios SOLID, código limpio y patrones de diseño. Detecta malos olores de código, propone mejoras estructurales y genera el código refactorizado. Úsala al limpiar deuda técnica o mejorar mantenibilidad.
license: Private
compatibility: Requiere .NET 8+ SDK. Compatible con C# 12.
---

# .NET Core — Refactorización Inteligente

Skill para analizar código C#/.NET Core, detectar problemas de diseño y aplicar refactorizaciones guiadas por principios SOLID y código limpio.

## Principios Guía

1. **Refactorizar sin cambiar comportamiento** — Los tests existentes deben seguir pasando.
2. **Pequeños pasos** — Cada refactorización es un commit atómico.
3. **Priorizar legibilidad** — Código que se lee como prosa > código "inteligente".
4. **YAGNI** — No añadas abstracciones que no necesitas hoy.

## Proceso de Análisis

### Paso 1: Detectar Malos Olores de Código

Revisa el código buscando estos indicadores:

| Indicador | Descripción | Severidad |
|-----------|-------------|-----------|
| **Método largo** | > 20 líneas de lógica | 🔴 Alta |
| **Clase grande** | > 300 líneas o > 5 responsabilidades | 🔴 Alta |
| **Parámetros excesivos** | > 4 parámetros en método | 🟡 Media |
| **Código duplicado** | Bloques repetidos en ≥ 2 lugares | 🔴 Alta |
| **Condicionales anidados** | > 2 niveles de if/else | 🟡 Media |
| **Nombres ambiguos** | Variables como `x`, `temp`, `data` | 🟡 Media |
| **Comentarios excesivos** | El código necesita explicación constante | 🟠 Media |
| **Envidia de funcionalidad** | Método usa más datos de otra clase | 🟡 Media |
| **Números mágicos** | Literales sin nombre semántico | 🟡 Media |
| **Código muerto** | Código inalcanzable o sin usar | 🟢 Baja |

### Paso 2: Clasificar por Principio SOLID Violado

```
[S] Responsabilidad Única    → Extraer clase/método
[O] Abierto/Cerrado          → Aplicar Strategy/Template Method
[L] Sustitución de Liskov    → Revisar herencia, preferir composición
[I] Segregación de Interfaces → Dividir interfaces grandes
[D] Inversión de Dependencias → Inyectar abstracciones
```

### Paso 3: Aplicar Refactorizaciones

## Catálogo de Refactorizaciones

### 1. Extraer Método
**Cuándo**: Bloque de código con comentario explicativo.

```csharp
// ❌ Antes
public void ProcessOrder(Order order)
{
    // Validar stock
    foreach (var item in order.Items)
    {
        var product = _repo.GetProduct(item.ProductId);
        if (product.Stock < item.Quantity)
            throw new InsufficientStockException(product.Name);
    }
    
    // Calcular total
    decimal total = 0;
    foreach (var item in order.Items)
    {
        total += item.Price * item.Quantity;
        if (item.HasDiscount)
            total -= item.DiscountAmount;
    }
    
    order.Total = total;
    _repo.Save(order);
}

// ✅ Después
public void ProcessOrder(Order order)
{
    ValidateStock(order);
    order.Total = CalculateTotal(order.Items);
    _repo.Save(order);
}

private void ValidateStock(Order order) { /* ... */ }
private decimal CalculateTotal(IEnumerable<OrderItem> items) { /* ... */ }
```

### 2. Reemplazar Condicional con Polimorfismo
**Cuándo**: Switch/if-else sobre un tipo para diferentes comportamientos.

```csharp
// ❌ Antes
public decimal CalculateShipping(Order order)
{
    switch (order.ShippingType)
    {
        case "standard": return order.Weight * 0.5m;
        case "express": return order.Weight * 1.5m + 10;
        case "overnight": return order.Weight * 3m + 25;
        default: throw new ArgumentException();
    }
}

// ✅ Después
public interface IShippingCalculator
{
    decimal Calculate(Order order);
}

public class StandardShipping : IShippingCalculator
{
    public decimal Calculate(Order order) => order.Weight * 0.5m;
}

// Registrar en inyección de dependencias:
services.AddKeyedScoped<IShippingCalculator, StandardShipping>("standard");
```

### 3. Introducir Objeto de Parámetros
**Cuándo**: Método con más de 3-4 parámetros relacionados.

```csharp
// ❌ Antes
public Report Generate(DateTime from, DateTime to, string department, 
                        bool includeInactive, string format)

// ✅ Después
public record ReportRequest(
    DateRange Period,
    string Department,
    bool IncludeInactive = false,
    string Format = "pdf"
);

public Report Generate(ReportRequest request)
```

### 4. Reemplazar Excepción con Resultado
**Cuándo**: Excepciones usadas como flujo de control.

```csharp
// ❌ Antes
public User GetUser(Guid id)
{
    var user = _repo.Find(id);
    if (user == null) throw new NotFoundException();
    return user;
}

// ✅ Después
public Result<User> GetUser(Guid id)
{
    var user = _repo.Find(id);
    return user is null
        ? Result<User>.Failure("Usuario no encontrado")
        : Result<User>.Success(user);
}
```

### 5. Extraer Servicio (Responsabilidad Única)
**Cuándo**: Clase con múltiples responsabilidades.

```csharp
// ❌ Antes — UserService hace de todo
public class UserService
{
    public User Create(CreateUserDto dto) { /* validar + crear + notificar */ }
    public void SendWelcomeEmail(User user) { /* ... */ }
    public byte[] GenerateAvatar(User user) { /* ... */ }
    public Report ExportUsers(ExportOptions opts) { /* ... */ }
}

// ✅ Después — Cada responsabilidad en su servicio
public class UserService { /* solo CRUD */ }
public class UserNotificationService { /* emails */ }
public class AvatarService { /* generación de imágenes */ }
public class UserExportService { /* reportes */ }
```

## Formato de Informe

Al finalizar el análisis, genera un informe con esta estructura:

```markdown
## Informe de Refactorización

### Resumen
- **Archivos analizados**: X
- **Malos olores detectados**: Y
- **Refactorizaciones aplicadas**: Z

### Hallazgos por Severidad
🔴 Críticos: N
🟡 Medios: N  
🟢 Bajos: N

### Detalle por Archivo
| Archivo | Smell | Refactorización | Estado |
|---------|-------|-----------------|--------|
| UserService.cs | Clase grande (450 líneas) | Extraer Servicio | ✅ Aplicada |
| OrderHandler.cs | Condicionales anidados (4 niveles) | Cláusulas de Guarda | ✅ Aplicada |

### Tests Verificados
- [ ] Tests existentes siguen pasando
- [ ] Nuevos tests añadidos para código extraído
```

## Lista de Verificación Post-Refactorización

- [ ] Todos los tests pasan (barra verde)
- [ ] No se agregó funcionalidad nueva (solo restructuración)
- [ ] Los nombres reflejan la intención del código
- [ ] No hay código duplicado
- [ ] Cada clase tiene una sola responsabilidad
- [ ] Las dependencias se inyectan, no se instancian
- [ ] Los métodos son ≤ 20 líneas
- [ ] La complejidad ciclomática es ≤ 10 por método
