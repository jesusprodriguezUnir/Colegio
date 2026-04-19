---
name: dotnet-wiki-generator
description: Genera documentación wiki estructurada a partir de código fuente .NET Core. Extrae XML docs, analiza arquitectura, y produce páginas Markdown para GitHub Wiki o Confluence.
license: Private
compatibility: Requiere .NET 8+ SDK. Soporta salida Markdown y HTML.
---

# .NET Core — Generador de Wiki (Documentación Extendida)

Esta sub-aplicación y habilidad permite la conversión ágil y unificada del esfuerzo de código nativo documentado (XML comments y resúmenes relacionales o endpoints ASP.NET) a artefactos legibles, profesionales y anclables en intranets u hojas centralizadas externas (Confluence / Github Wiki).

## Capacidades Intrínsecas

1. **Extracción y barrido masivo** hacia la lectura de la semantica `<summary>`, `<param>`, y otros modificadores nativos C#.
2. **Scanner Arquitectónico** que disecciona tu dependencia y detecta límites limpios o polución.
3. Generación sintáctica de Diagramas y Mermaid auto-replegados según relaciones entre proyectos dll/csproj.
4. Tabla de Contenidos fluida y anclable a los headers Markdown.
5. Referenciador veloz y robusto de las APIs; convirtiendo los tipos y clases expuestas (Records / Request / Result) a ejemplos crudos HTTP REST.

## La Configuración del Ecosistema Resultante

Emanará una jerarquía organizada a nivel directorio de la siguiente envoltura:
```text
wiki/
├── Home.md                      # Index General y Manifiesto de entrada
├── getting-started.md           # Primeros Pasos del programador
├── architecture/
│   ├── overview.md              # Contexto de Integración 
│   ├── decisions.md             # Architecture Decision Records (ADR, tus confesiones técnicas)
│   └── dependencies.md          # Radiografía visual de Módulos (Mermaid)
├── api/
│   ├── endpoints.md             # Listado transaccional y Web
├── database/
│   ├── schema.md                # Esquema Db contextual de Entidades relacionales  
```

## Ciclo y Pasos Constructivos

### 1. Extracción e Identificación Endémica
Acudimos al XML Summary original:
```csharp
/// <summary>
/// Motor principal para la expedición de recibos en tránsito fiscal.
/// </summary>
/// <remarks>
/// Recrea el Patrón de Compensación Transaccional si la factura externa falla (Timeout/503).
/// </remarks>
public class ReceiptExpeditionService : IExpeditionOrchestrator
```
Transformamos estas sentencias en epígrafes limpios.

### 2. Extrusión hacia REST Endpoints

Un WebApi simple se volcará documentadamente como:

```markdown
## POST /api/invoices

Da a luz un recibo impositivo.

### Request Payloads (Payloads de Solicitud)

```json
{
  "fiscalId": "string (mandatory, DNI format)",
  "grossTotal": "decimal (> 0)"
}
```

### Respuestas Frecuentes

| Código HTTP | Detalle Técnico | Body Retornado |
|--------|-------------|------|
| 201 | Creado correctmente | `InvoiceCreatedDto` |
| 400 | Requisitos incumplidos (Validacion Interceptada) | `ProblemDetails` estándar de .NET |
```

### 3. Registro de Decisiones de Arquitectura (La Falsa Memoria)
Promueve la transparencia en entornos colaborativos con **ADR Documentation**:
```markdown
## ADR-005: Elección de Redis para la Memcached Sub-sistema

**Estado**: Confirmado  
**Fecha de Ratificación**: 2024-05-30

### Contexto Limitante
La capa SQL ha sobrepasado los 400 DTU de cómputo en horas pico ante búsquedas sobre tipologías inalterables.

### Resumen Consensuado
Uso de Caching Distribuido, abrazando el nuget nativo IDistributedCache y empleando a Redis como Broker y BackStore.

### Secuelas a Vigilar
- ✅ Elivación abrumadora de cuellos de botella CPU sobre SQL originario.
- ⚠️ Nos condena contractualmente a orquestar invalidaciones pro-activas de los bloques para prevenir datos arcaicos o desconfiguraciones catastróficas.
```

## Consejos de Sostenibilidad y Mantenimiento

Integra estas utilidades en tu canal (Pipeline) CI/CD.
Aprovechando herramientas base como **DocFX** (mítico .NET Doc framework) automatizando a cada `Merge de PR en Main Branch` tu Github Pages o carpeta de Assets será rearmada desde cero garantizando que el abismo entre la Wiki del proyecto y lo codificado realmente en C# se mantenga insustancial y mínimo.
No documentes tu API a mano, déjalo emanar de tu flujo de trabajo de programación C#.
