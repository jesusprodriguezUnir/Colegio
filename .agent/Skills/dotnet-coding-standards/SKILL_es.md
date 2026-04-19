---
name: dotnet-coding-standards
description: Escribe código C# moderno y de alto rendimiento usando records, pattern matching, value objects, async/await, Span<T>/Memory<T> y patrones de diseño de API. Enfatiza programación funcional con C# 12+.
license: Private
compatibility: Requiere .NET 8+ SDK. Compatible con C# 12.
---

# Modern C# Coding Standards

## Cuándo usar esta skill

Úsala cuando vayas a:
- Escribir código nuevo o refactorizar C# existente.
- Diseñar la API pública para bibliotecas o servicios.
- Optimizar rutas de ejecución críticas a nivel de rendimiento.
- Modelar lógicas del dominio fuertemente tipadas.
- Orquestar operaciones asíncronas masivas (async/await pesado).
- Lidiar con representaciones binarias en memoria, buffers o entornos de alta concurrencia.

## Principios Centrales

1. **Inmutabilidad por defecto** - Preferencia por los tipos `record` y las propiedades `init`.
2. **Type Safety** - Uso de tipos de referencia anulables (`Nullable Reference Types`) y objetos de valor (Value Objects).
3. **Pattern Matching Moderno** - Dominio de expresiones `switch` y patrones variados de C# emergentes.
4. **Async por doquier** - Preferencia absoluta por APIs asíncronas delegando su Cancellation Token oportunamente.
5. **Zero-Allocation** - Dominio de `Span<T>` y `Memory<T>` para secciones críticas donde minimizar interacciones con el Garbage Collector es vital.
6. **Diseño de APIs** - Acepta abstracciones genéricas (ej: `IEnumerable`) pero retorna tipos razonablemente explícitos o sellados.
7. **Composición sobre Herencia** - Evita clases abstractas base, favoreciendo delegación e interefaces.
8. **Objetos de Valor (Value Objects) estructurados** - Implementados preferiblemente con `readonly record struct`.

---

## Patrones de Lenguaje

### Records para Datos Inmutables (C# 9+)

Usados en Modelos DTO, Mensajes asíncronos y Entidades de Dominio.

```csharp
// DTO Inmutable simple
public record CustomerDto(string Id, string Name, string Email);

// Evitar la mutabilidad con Listas: Usa interfaces de sólo lectura
public record ShoppingCart(
    string CartId,
    IReadOnlyList<CartItem> Items
)
{
    public decimal Total => Items.Sum(item => item.Price * item.Quantity);
}
```

### Objetos de Valor en readonly record struct

Todo VO debería ser empaquetado usando `readonly record struct` para un rendimiento ideal y semántica de valores directos.

```csharp
public readonly record struct OrderId(string Value)
{
    public OrderId(string value) : this(
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("El OrderId no puede ir vacío.", nameof(value)))
    { }
    public override string ToString() => Value;
}
```

### Composición por encima de herencia

**Evita las super-clases heredadas**.
Usa métodos estáticos para lógicas compartidas y prioriza los patrones factoría sobre interfaces cuando generes variaciones de un mismo ente.

---

## Patrones de Rendimiento

### Buenas Prácticas Async/Await

```csharp
// Inyección hasta el fondo con los CancellationToken
public async Task<Order> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
{
    return await _repository.GetAsync(orderId, cancellationToken);
}

// Empleo de ValueTask para operaciones probabilísticamente sincrónicas y veloces (como usar métodos que dependen mucho de Cachés locales)
public ValueTask<Order?> GetCachedOrderAsync(string orderId, CancellationToken cancellationToken)
{
    if (_cache.TryGetValue(orderId, out var order))
        return ValueTask.FromResult<Order?>(order);
        
    return GetFromDatabaseAsync(orderId, cancellationToken);
}
```

**Reglas de Oro:**
- NUNCA uses métodos `.Wait()` y `.Result` de una operación Task de manera asíncrona (conllevará *deadlocks* en subprocesos sincronizados).

## Tipos Result en lugar de Excepciones

Usa un modelo o tipo base `Result<T, TError>` en detrimento de excepciones programáticas. Las `throw Exception` se resguardan ÚNICAMENTE para errores y cataclismos no esperados de red/sistema o validaciones técnicas ineludibles.

## Resumen de Mejores Prácticas

### HAZLO:
- Usa `record` para todos los DTOs y Mensajería.
- Usa `readonly record struct` en los objetos de valor.
- Exige `CancellationToken` siempre que haya un `Task` asíncrono.
- Utiliza `ArrayPool<T>` si necesitas buffers gigantes durante poco tiempo.

### NO LO HAGAS:
- Evadir las advertencias de compilador sobre anulables: son tu primera línea de defensa contra `NullReferenceException`.
- Escribir `byte[]` en cascada si un simple `Span<byte>` te permite ojear subcadenas sin recolocarlas de nuevo en pila o memoria.
