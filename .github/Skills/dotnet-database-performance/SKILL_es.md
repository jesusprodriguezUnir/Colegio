---
name: dotnet-database-performance
description: Patrones de acceso a base de datos para rendimiento. Separa modelos de lectura/escritura, evita queries N+1, usa AsNoTracking, aplica límites de filas y nunca hagas joins en la aplicación.
license: Private
compatibility: Requiere .NET 8+ SDK con EF Core o Dapper.
---

# Patrones de Rendimiento en Bases de Datos
 
## Cuándo usar esta skill
 
Úsala cuando:
- Diseñas capas de acceso a datos de alto impacto.
- Tratas de optimizar "queries" o consultas lentas.
- Requieres elegir entre EF Core y Dapper para escenarios concretos.
- Quieres evitar los típicos obstáculos que tumban el rendimiento de una BBDD.
 
---
 
## Principios Base
 
1. **Separa los modelos de Lectura y Escritura** - Nunca utilices las mismas clases para leer y escribir.
2. **Pensamiento por lotes** - Elude el funesto problema de N+1 queries.
3. **Pide sólo lo que necesites** - Olvida los `SELECT *`.
4. **Acota el número de filas** - Siempre impón y acepta métodos de control (Limit / Take).
5. **Deja que SQL haga las proyecciones (Joins)** - Nunca las hagas en memoria C#.
6. **AsNoTracking para las lecturas** - El rastreo de EF Core cuesta ciclos e incrementa el GC.
 
---
 
## Separación de Lectura y Escritura (Patrón CQRS)
 
No reutilices la mítica entidad masiva `User` de manera transversal.
 
- **Modelos Listos (Lectura)** están desnormalizados, afinados a velocidad, y devuelven proyecciones específicas (`UserProfile`, `UserSummary`...).
- **Modelos Transaccionales (Escritura)** están normalizados, centrados en aplicar validación sólida y absorben comandos (`CreateUserCommand`).
 
### Arquitectura y Esquema
 
```
src/
  MyApp.Data/
    Users/
      # Área de Lecura - Proyecciones limpias
      IUserReadStore.cs
      PostgresUserReadStore.cs
 
      # Área de Escritura - Comandos asertivos
      IUserWriteStore.cs
      PostgresUserWriteStore.cs
```

### Reglas básicas

El entorno de Lectura devuelve múltiples variantes y DTOs distintos, actuando de forma **sin estado (Stateless)** y sin rastreo. En el otro lado, las implementaciones Write retornan `void` o sencillamente el ID tras validar la lógica de Command; el foco de este área no radica en "devolver los datos que acaban de guardarse".
 
---
 
## Acotar los resultados (Limit limits)
 
**No exportes listados colosales bajo ningún pretexto**. 
Impregna todas tus llamadas con el concepto del paginado nativo.
 
### Patrón Limit en EF Core:
 
```csharp
public async Task<PaginatedList<OrderSummary>> GetOrdersAsync(
    CustomerId customerId,
    Paginator paginator,
    CancellationToken ct = default)
{
    var query = _context.Orders
        .AsNoTracking()
        .Where(o => o.CustomerId == customerId.Value)
        .OrderByDescending(o => o.CreatedAt);
 
    var totalCount = await query.CountAsync(ct);
 
    var orders = await query
        .Skip((paginator.PageNumber - 1) * paginator.PageSize)
        .Take(paginator.PageSize)  // ¡SIEMPRE LIMITE O TAKE!
        .Select(o => new OrderSummary(new OrderId(o.Id), o.Total, o.Status, o.CreatedAt))
        .ToListAsync(ct);
 
    return new PaginatedList<OrderSummary>(orders, totalCount, paginator.PageSize, paginator.PageNumber);
}
```
 
---
 
## AsNoTracking
Actívalo por defecto. Retira la propiedad si persigues un estado de mutación inminente antes del `SaveChangesAsync()`.
 
```csharp
// CORRECTO - Apagar seguimiento
var users = await _context.Users.AsNoTracking().Where(u => u.IsActive).ToListAsync();
```

## Problema N+1 y Soluciones

El error N+1 arrastra penalizaciones exponenciales por lanzar una consulta a la BD por cada hijo en el bucle padre (es decir, en `n` saltos).

**Solución 1: Explotación del Include (EF Core)**
Aceptable para mallas pequeñas.
```csharp
var orders = await _context.Orders.AsNoTracking().Include(o => o.Items).ToListAsync();
```

**Solución 2: Batch Query con Dapper**
Formato ideal y óptimo para evitar productos cartesianos y reducir cargas si hay 10+ joins.
El código extrae primero Orders y luego los Items vinculándolos mediante C# una vez en memoria, usando 2 *viajes únicos* al motor SQL.

## Jamás cruces Joins en local!
Cede estas operaciones al relacional a no ser de que hayas ejecutado una operación batch como la de Dapper mencionada. C# sufre de desperdicio de O(n+m) al correlacionar en LINQ-To-Objects sobre tablas descargadas al RAM.

```csharp
// FATAL - Un desastre RAM
var result = customers.Select(c => new {
    Customer = c,
    Orders = orders.Where(o => o.CustomerId == c.Id).ToList() 
});
```

---

## Repositorios Genéricos: NO

Evita la trampa `IRepository<T>` a toda costa. Oculta la complejidad N+1, arruina optimizaciones específicas de SQL e impide aplicar Limit/Take orgánicos y eficientes. Crea Stores acotados (`IOrderReadStore` y análogos).

## Rendimientos Masivos (`ExecuteUpdateAsync`)
.NET 7 te permite evitar bajar a memoria las filas editadas:
```csharp
await _db.Orders
    .Where(o => o.ExpiresAt < DateTimeOffset.UtcNow)
    .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatus.Expired));
```
