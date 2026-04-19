---
name: dotnet-code-review
description: Analiza Pull Requests en proyectos .NET Core realizando revisión de código automatizada. Evalúa calidad, seguridad, rendimiento, convenciones y tests. Genera un informe detallado con puntuación y recomendaciones accionables. Úsala para revisiones de PRs o auditorías de código.
license: Private
compatibility: Requiere .NET 8+ SDK. Funciona con diffs de Git y archivos C#.
---

# .NET Core — Revisión de Código de Pull Requests

Skill para analizar Pull Requests de proyectos .NET Core y generar informes de revisión de código detallados con puntuación y recomendaciones.

## Cuándo Usar

- Al recibir una PR para revisión
- Antes de fusionar a `main` o `develop`
- Para auditorías de calidad de código
- Como segundo par de ojos tras revisión humana

## Dimensiones de Evaluación

El análisis cubre 6 dimensiones, cada una con peso diferente:

| Dimensión | Peso | Qué Evalúa |
|-----------|------|-------------|
| 🏗️ **Arquitectura** | 25% | Separación de capas, DI, patrones |
| 🔒 **Seguridad** | 20% | Vulnerabilidades, secretos, validación de entrada |
| ⚡ **Rendimiento** | 15% | Consultas N+1, asignaciones, async/await |
| 📐 **Convenciones** | 15% | Nomenclatura, formato, estilo del equipo |
| 🧪 **Testing** | 15% | Cobertura, calidad de tests, casos límite |
| 📖 **Legibilidad** | 10% | Claridad, complejidad, documentación |

## Proceso de Revisión

### Paso 1: Analizar el Contexto de la PR

Leer:
- **Título y descripción** de la PR
- **Commits** incluidos (mensajes y tamaño)
- **Archivos modificados** (lista y estadísticas)
- **Incidencia o ticket** vinculado

Evaluar:
```
✅ La PR tiene un propósito único y claro
✅ Los commits son atómicos y descriptivos
✅ El tamaño es razonable (< 400 líneas idealmente)
⚠️ PR grande (> 400 líneas) → sugerir dividir
❌ PR sin descripción → solicitar contexto
```

### Paso 2: Revisión de Arquitectura (25%)

#### Verificaciones

```
□ ¿Respeta la separación de capas (Domain → Application → Infrastructure)?
□ ¿Las dependencias apuntan hacia adentro (Inversión de Dependencias)?
□ ¿Los nuevos servicios se registran en la inyección de dependencias correctamente?
□ ¿Se usan interfaces para abstracciones?
□ ¿Los DTOs son independientes de las entidades de dominio?
□ ¿Se aplican los patrones del proyecto (CQRS, Repository, etc.)?
```

#### Señales de Alerta

```csharp
// ❌ Controller con lógica de negocio
[HttpPost]
public async Task<IActionResult> Create(CreateUserDto dto)
{
    // Validación manual en el controller
    if (string.IsNullOrEmpty(dto.Email)) return BadRequest();
    
    // Acceso directo a DbContext desde el controller
    var user = new User { Email = dto.Email };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    
    // Envío de email desde el controller
    await _emailService.Send(user.Email, "Bienvenido");
    
    return Ok(user);
}

// ✅ Debería delegarse a un handler/servicio
[HttpPost]
public async Task<IActionResult> Create(CreateUserCommand command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Created(result.Value) : BadRequest(result.Error);
}
```

### Paso 3: Revisión de Seguridad (20%)

#### Lista de Verificación Crítica

```
□ No hay secretos/contraseñas escritos directamente en el código
□ Los inputs del usuario se validan (FluentValidation / Data Annotations)
□ Las queries usan parámetros (no string interpolation de SQL)
□ Los endpoints tienen [Authorize] apropiado
□ No se exponen trazas de error en producción
□ Se sanitiza output para prevenir XSS
□ Los archivos subidos se validan (tipo, tamaño)
□ CORS está configurado correctamente
```

#### Patrones Peligrosos

```csharp
// ❌ Inyección SQL
var query = $"SELECT * FROM Users WHERE Email = '{email}'";

// ❌ Secreto escrito directamente en el código
var apiKey = "sk-1234567890abcdef";

// ❌ Información sensible en logs
_logger.LogInformation("User login: {Email}, Password: {Password}", email, password);

// ❌ Deserialización insegura
JsonConvert.DeserializeObject<object>(untrustedInput, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All // ¡PELIGROSO!
});

// ❌ Endpoint sin autorización
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id) // ¿Dónde está [Authorize]?
```

### Paso 4: Revisión de Rendimiento (15%)

#### Problemas Comunes

```csharp
// ❌ Consulta N+1
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    order.Items = await _context.OrderItems
        .Where(i => i.OrderId == order.Id)
        .ToListAsync(); // ¡N queries adicionales!
}

// ✅ Carga anticipada
var orders = await _context.Orders
    .Include(o => o.Items)
    .ToListAsync();

// ❌ Async sin await (lanzar y olvidar)
public void ProcessOrder(Order order)
{
    _emailService.SendAsync(order.Email, "Confirmación"); // ¡No se espera!
}

// ❌ Asignación innecesaria en ruta crítica
public string GetDisplayName(User user)
{
    return $"{user.FirstName} {user.LastName}".Trim(); // Nuevo string cada vez
}

// ❌ ToList() prematuro
var count = _context.Users.ToList().Count(); // ¡Carga TODA la tabla!
// ✅
var count = await _context.Users.CountAsync();
```

### Paso 5: Revisión de Convenciones (15%)

```
□ PascalCase para clases, métodos, propiedades públicas
□ camelCase para variables locales y parámetros
□ _camelCase para campos privados
□ Sufijo Async en métodos asíncronos
□ Interfaces con prefijo I
□ Los Namespaces coinciden con la estructura de carpetas
□ Archivos nombrados como la clase que contienen
□ Regiones evitadas (señal de clase grande)
□ Sentencias Using ordenadas y sin duplicados
```

### Paso 6: Revisión de Testing (15%)

```
□ ¿Los cambios incluyen tests nuevos o actualizados?
□ ¿Los tests cubren camino feliz Y casos límite?
□ ¿Los tests son independientes (no dependen del orden)?
□ ¿Los mocks verifican interacciones correctas?
□ ¿Hay tests de integración para cambios de infraestructura?
□ ¿La cobertura del código nuevo es ≥ 80%?
```

#### Señales de Alerta en Tests

```csharp
// ❌ Test sin verificación
[Fact]
public async Task Create_ShouldWork()
{
    await _sut.CreateAsync(new CreateUserDto()); // ¿Y qué verificamos?
}

// ❌ Test que pasa siempre
[Fact]
public void Validate_ShouldValidate()
{
    Assert.True(true); // Siempre verde
}

// ❌ Test acoplado a implementación
[Fact]
public void Delete_ShouldCallRepositoryThenLoggerThenCacheThenEventBus()
{
    // Verifica el orden exacto de 4 interacciones internas
    // → Frágil, se rompe con cualquier refactor
}
```

### Paso 7: Revisión de Legibilidad (10%)

```
□ ¿El código es autoexplicativo sin comentarios?
□ ¿Los nombres revelan la intención?
□ ¿La complejidad ciclomática es ≤ 10?
□ ¿Los métodos hacen una sola cosa?
□ ¿Los comentarios explican "por qué", no "qué"?
```

## Formato del Informe

```markdown
# Revisión de Código — PR #{número}: {título}

## 📊 Puntuación General: {X}/100

| Dimensión | Puntuación | Estado |
|-----------|-----------|--------|
| 🏗️ Arquitectura | {X}/25 | {✅ ⚠️ ❌} |
| 🔒 Seguridad | {X}/20 | {✅ ⚠️ ❌} |
| ⚡ Rendimiento | {X}/15 | {✅ ⚠️ ❌} |
| 📐 Convenciones | {X}/15 | {✅ ⚠️ ❌} |
| 🧪 Testing | {X}/15 | {✅ ⚠️ ❌} |
| 📖 Legibilidad | {X}/10 | {✅ ⚠️ ❌} |

## Veredicto

{🟢 APROBAR | 🟡 APROBAR CON CAMBIOS MENORES | 🔴 SOLICITAR CAMBIOS}

### Cambios Requeridos (bloqueantes)
1. {descripción del problema + archivo:línea + solución sugerida}

### Sugerencias (no bloqueantes)
1. {descripción + por qué mejoraría el código}

### Puntos Positivos ✨
1. {algo bien hecho que vale la pena destacar}

## Detalle por Archivo

### `src/Application/Handlers/CreateUserHandler.cs`

**Línea 34**: ❌ La validación de email debería estar en el Validator, no en el Handler.
```csharp
// Actual
if (!IsValidEmail(command.Email)) throw new Exception("Invalid");

// Sugerido
// Mover a CreateUserValidator usando FluentValidation
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

**Línea 67**: ⚠️ Considerar usar `CancellationToken` en la llamada async.
```csharp
// Actual
await _repository.SaveAsync();
// Sugerido
await _repository.SaveAsync(cancellationToken);
```
```

## Escala de Puntuación

| Rango | Veredicto | Acción |
|-------|-----------|--------|
| 90-100 | 🟢 Excelente | Aprobar directamente |
| 70-89 | 🟡 Bueno | Aprobar con sugerencias menores |
| 50-69 | 🟠 Necesita mejoras | Solicitar cambios específicos |
| 0-49 | 🔴 Requiere reescritura | Discutir enfoque con el autor |

## Etiquetas de Severidad

- `❌ Bloqueante` — Debe corregirse antes de fusionar
- `⚠️ Importante` — Debería corregirse, pero no bloquea
- `💡 Sugerencia` — Mejora opcional
- `❓ Pregunta` — Necesita aclaración del autor
- `✨ Bien hecho` — Refuerzo positivo
