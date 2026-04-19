---
name: dotnet-minimal-apis
description: Diseña e implementa Minimal APIs en ASP.NET Core usando endpoints handler-first, route groups, filtros y composición ligera adaptada a servicios .NET modernos.
license: Private
compatibility: Requiere ASP.NET Core 6+, preferiblemente .NET 8+ para funcionalidad completa.
---

# Minimal APIs (APIs Mínimas)

## Cuándo usar estas instrucciones

- Vas a crear APIs nuevas bajo el paraguas moderno de ASP.NET Core.
- Estás forjando microservicios ágiles sin burocracia de capas.
- Debes discernir cuándo apostar por controladores MVC convencionales vs la agilidad de los Endpoints (Minimal APIs).
- Has de estructurar tu mapeo vía extensiones ("Route Groups").
- Te centras en validaciones de entradas bajo el sistema de *"Filtros"*.

## Minimal APIs vs MVC (Controladores Base)

| Usa Minimal API | Usa MVC Completo |
|------------------|-----------------|
| Rutas y API recien planteada | En Mantenimiento sobre legado MVC |
| Microservicios funcionales y sencillos | Fuerte unión a Data Binding complejo / HTML |
| Abstracción CRUD pura | Requerimientos OData, JsonPatch intensivo |
| Handlers ligeros y factoría in-line | Preferencia corporativa por `[Attributes]` y herencia rígida |
| Paradigmas post .NET 8 | Ecosistema altamente arraigado a `[ApiController]` |

## Flujo de Producción y Diseño

1. **Diseñarás en extensiones, NO** aglomerando el `Program.cs`.
2. Emplea **Grupos (Route Groups)** para unificar subapartados jerárquicos y filtros masivos.
3. Extrae tus métodos *Run/Task* a entidades independientes a medida que engordan.
4. Inyecta **Filtros de validación automáticos** que escaneen toda petición central de un grupo.
5. Emplea forzosamente **`TypedResults`**. Nada de objetos abstractos devueltos al aire.

## Aplicaciones Habituales

### Retornos Seguros (Strongly-Typed)
Ayudas a OpenAPI a describir sin errar tu respuesta:
```csharp
app.MapGet("/products/{id}", Results<Ok<Product>, NotFound> (int id, AppDb db) =>
{
    var product = db.Products.Find(id);
    return product is not null ? TypedResults.Ok(product) : TypedResults.NotFound();
});
// [FromServices] puede agregarse por legibilidad a parámetros inyectados en cascada
```

### Agrupación y Mapeo en Cadena
No expongas tu `Program.cs`. Genera clases estáticas. Otorga cohesión compartida:
```csharp
var api = app.MapGroup("/api").RequireAuthorization().AddEndpointFilter<ValidationFilter>();

var products = api.MapGroup("/products").WithTags("Products");
var orders = api.MapGroup("/orders").WithTags("Orders").RequireAuthorization("AdminOnly");

// Llenado de hojas
products.MapGet("/", GetAll);
products.MapPut("/{id}", Update);
```

## Filtros ("Endpoint Filters")

Se postulan como la navaja suiza actual para cortar problemas de raíz antes de que tus métodos reciban peticiones infractoras:

```csharp
// Un filtro genérico de validación sobre FluentValidation atado globalmente
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null) return Results.BadRequest("Invalid request body");

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(argument);
            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
        }
        return await next(context);
    }
}

// Conexión
products.MapPost("/", Create)
    .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();
```

## Prácticas DTO Puras

SÍ: Retorna Records paralelos llamados Response y Request.
NO: Arrojes una Entidad ORM (Domain Entities de Base de Datos crudos) al exterior.

## Patrones a Evitar ⛔

| Antipatrón | Coste/Por qué es nocivo | Remedio Directo |
|--------------|--------------|-----------------|
| Colapsar Program.cs con cientos de líneas | Inmantenible e inlegible | Métodos de extensión modulares |
| Escasez de Grupos | Verbosiad exhaustiva ("Repetir DRY") | Aduna lógicas comunes (`MapGroup`) |
| Validar en medio del método final | Fugas de control / Dificil de testear | Adosa validaciones al `Filter pipeline` |
| Devolver Entity DbContext ciego | Roturas por bucles (ReferenceLoop), Acoplamiento duro | Haz uso intensivo de los DTOs / Records |
| Olvidar `.WithOpenApi()` | Tu Swagger UI estará desierto | Auto-compón tus documentaciones |
