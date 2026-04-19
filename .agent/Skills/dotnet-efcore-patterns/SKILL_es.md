---
name: dotnet-efcore-patterns
description: Buenas prácticas de Entity Framework Core incluyendo NoTracking por defecto, query splitting para colecciones de navegación, gestión de migraciones, servicios dedicados de migración y errores comunes a evitar.
license: Private
compatibility: Requiere .NET 8+ SDK con EF Core 8+.
---

# Patrones Entity Framework Core

## Cuándo Usar Esta Skill

- Al configurar EF Core en un proyecto recién lanzado
- Para optimizar cuellos de botella y rendimiento de queries
- Mantener y administrar migraciones base
- Instalar y adaptar Aspire junto a EF
- Entender desajustes con el ChangeTracker o prevenir Cartesianos en colecciones anidadas

## Los Principios Fundacionales

1. **NoTracking por Default** - Asume lectura; obliga confirmación y tracking individual en las actualizaciones.
2. **Las migraciones no se tocan a mano** - Usa CLI siempre.
3. **Workers Exclusivos de Migración** - Separa este delicadísimo paso del encendido global del Core App.
4. **Sigue un ExecutionStrategy para Reintentos** - Las tripas transitorias de las BBDD nubosas fallan; protégete.

---

## Patrón 1: Ausencia de Tracking por Defecto

Afinará drásticamente la latencia de toda operación de lecturas. 

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
}
```

### Implicación Directa

Si tratas de modificar un registro invocado sin `.AsTracking()` no surtirá efecto. Deberás añadir el comando expreso `_db.Orders.Update(order);` para revincular el mapeo antes de `SaveChangesAsync()`.
¿La alternativa? Declarar un `.AsTracking()` dentro de la sentencia linq de invocación.

| Caso | ¿Tracker? | Motivo |
|----------|---------------|-----|
| Render en el UI / Front | No | Lectura superficial |
| Microservicio GET | No | Data estática / DTOs retornados |
| Comando PUT/PATCH/UPDATE | Sí | Requiere volcado de información |
| Operaciones BULK (borrado/edición masiva) | No | Más rápido si usas `.ExecuteUpdateAsync` |

---

## Patrón 2: Mantén la CLI a cargo de tus Archivos Core de SQL Históricos

NUNCA (a no ser de que necesites meter vistas, sprocs de T-SQL o RAW queries en los puentes `Up() / Down()`):
- Edites los nombres de las clases de migración
- Borres las migraciones .cs del proyecto
- Importes migraciones sin usar las utilidades `ef migrations`

**Genera Scripts idempotentes de Despliegue para CD/CI:**
```bash
dotnet ef migrations script --idempotent \
    --project src/MyApp.Infrastructure --startup-project src/MyApp.Api
```

---

## Patrón 3: Migrations-as-a-Service, el futuro del Aspire

Extrae las migraciones de `Program.cs`. Genera un Worker Service adyacente. Los `HostedServices` puros o `BackgroundWorker` asegurarán el Seed y los alter-tables bloqueando e inmovilizando la ApplicationApi hasta que el semáforo devuelva OK. Revisa e inspecciona detenidamente *MyApp.MigrationService* si operas en arquitecturas serias o distribuidas de Microservicios.

---

## Patrón 4: Transient Failures (Temblores de Red Transitorios) y Retries

Si usas Cloud, las BBDD se "desconecta" microsegundos. Tu resiliencia viene dada por el `CreateExecutionStrategy()`.

```csharp
var strategy = _dbContext.Database.CreateExecutionStrategy();

await strategy.ExecuteAsync(async () =>
{
    // TODA LAS OPERACIONES Y TRANSACCIONES DENTRO DEL LAMBDA DE EJECUCIÓN 
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});
```

---

## Patrón 5: Operaciones Masivas (Vía Rápida) `ExecuteUpdateAsync`

`// EF Core 7+` permite alterar esquemas de BBDD sin transbordar esos gigabytes a variables de la RAM o usar Bucles:

```csharp
// BORRAR FILAS MASIVAMENTE SUPER RÁPIDO
await _db.Orders
    .Where(o => o.Status == OrderStatus.Cancelled && o.CreatedAt < cutoffDate)
    .ExecuteDeleteAsync();
```

---

## Patrón 6: Atajar las Explosiones Cartesianas (Query Splitting)

Las inclusiones en cadena `.Include(o => o.Padres).Include(o => o.Productos)` provocan explosión de filas debido al CROSS JOIN subyacente que devuelve SQL. Una fila de pedido, vinculada a 100 reseñas y 20 imágenes... multiplicará 1x100x20 = 2000 filas de bytes a tu red.

**Remedio**: Establece globalmente la bandera `QuerySplittingBehavior.SplitQuery`. EF core particionará esa solicitud monstruosa en pequeñas consultas individuales paralelizables o asíncronas independientes por rama jerárquica. Retoma el SingleQuery o Join total `AsSingleQuery()` para árboles enanos que garantices que no pasarán de 3 ramificaciones.
