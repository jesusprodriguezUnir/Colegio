---
name: dotnet-code-review
description: Analiza Pull Requests en proyectos .NET Core mediante revisión de código automatizada. Evalúa arquitectura, seguridad, rendimiento, convenciones y tests, generando un informe detallado.
license: Private
compatibility: Requiere .NET 8+ SDK. Funciona con diffs de Git y archivos C#.
---

# .NET Core — Revisión de Código de Pull Requests

Skill para analizar Pull Requests de proyectos .NET Core y generar informes de revisión de código detallados con puntuación y recomendaciones accionables.

## Cuándo Usar

- Al recibir una PR para revisión
- Antes de fusionar a `main` o `develop`
- Para auditorías de calidad de código
- Como segundo par de ojos tras una revisión humana

## Dimensiones de Evaluación

El análisis cubre 6 dimensiones, cada una con un peso diferente:

| Dimensión | Peso | Qué Evalúa |
|-----------|------|-------------|
| 🏗️ **Arquitectura** | 25% | Separación de capas, Inversión de Dependencias (DI), patrones |
| 🔒 **Seguridad** | 20% | Vulnerabilidades, secretos en código, validación de inputs |
| ⚡ **Rendimiento** | 15% | Problemas N+1, asignaciones de memoria, uso de async/await |
| 📐 **Convenciones** | 15% | Nomenclatura, formato, alineación con el estilo del equipo |
| 🧪 **Testing** | 15% | Cobertura de pruebas, calidad de los tests, casos límite |
| 📖 **Legibilidad** | 10% | Claridad, complejidad ciclomática, documentación |

## Proceso de Revisión

### Paso 1: Analizar el Contexto de la PR

Leer:
- **Título y descripción** de la PR
- **Commits** incluidos (mensajes y tamaño)
- **Archivos modificados** (estadísticas y lista)
- **Incidencia o ticket** vinculado (si lo hay)

Evaluar:
```
✅ La PR tiene un propósito único y claro
✅ Los commits son atómicos y descriptivos
✅ El tamaño es razonable (< 400 líneas idealmente)
⚠️ PR masiva (> 400 líneas) → Sugerir dividir
❌ PR sin descripción → Solicitar contexto
```

### Paso 2: Revisión de Arquitectura (25%)

#### Verificaciones
```
□ ¿Respeta la separación de capas (Domain → Application → Infrastructure)?
□ ¿Las dependencias fluyen hacia adentro (Regla de Dependencia)?
□ ¿Se registran correctamente los nuevos servicios en inyección de dependencias?
□ ¿Se usan interfaces para definir abstracciones clave?
□ ¿Están desacoplados los DTOs de las entidades de dominio?
□ ¿Se aplican los patrones estructurales del proyecto (CQRS, Repository, etc.)?
```

### Paso 3: Revisión de Seguridad (20%)

#### Lista de Verificación Crítica
```
□ No hay contraseñas o secretos hardcodeados
□ Se validan todas las entradas de usuario (FluentValidation / Data Annotations)
□ Se usa parametrización en queries a BBDD (protección SQLi)
□ Los endpoints tienen `[Authorize]` adecuadamente aplicado
□ No se exponen StackTraces ni de errores internos al cliente en Producción
□ Se sanitiza la salida (Output encoding contra XSS)
□ Manejo seguro en la subida de archivos (tipo, tamaño)
□ Configuración correcta de CORS
```

### Paso 4: Revisión de Rendimiento (15%)

#### Problemas Comunes a Vigilar
```
□ Consultas N+1 en Entity Framework (faltan métodos `.Include()`)
□ Uso asíncrono sin `await` (Launch and Forget) en contextos críticos
□ Asignación de memoria en bucles y rutas calientes (ej: excesivo `.ToList()`)
```

### Paso 5: Revisión de Convenciones (15%)

```
□ PascalCase para tipos y miembros públicos
□ camelCase para variables locales y firmas
□ _camelCase para campos privados de clase
□ Uso de sufijo `Async` en firmas Tasks o corutinas
□ Namespaces que coinciden fielmente con los directorios
```

### Paso 6: Revisión de Testing (15%)

```
□ ¿Hay tests nuevos que acompañan la lógica implementada?
□ ¿Se testean los "happy paths" y "edge cases"?
□ ¿Están correctamente isolados y libres de dependencias temporales/secuenciales?
□ ¿Se proveen buenos *Assertions* y comprobación del resultado esperado?
```

### Paso 7: Revisión de Legibilidad (10%)

```
□ Código descriptivo y auto-explicativo frente a redundancia de comentarios
□ Limitación de la complejidad ciclomática de los métodos
□ Principio de Única Responsabilidad visible por componente
```

## Formato de Entrega del Informe

Emplea Markdown para generar un informe estructurado como sigue (adaptado según conclusiones):

```markdown
# Revisión de Código — PR #{número}: {título}

## 📊 Puntuación General: {X}/100

| Dimensión | Puntuación | Estado |
|-----------|-----------|--------|
| 🏗️ Arquitectura | {X}/25 | {✅ ⚠️ ❌} |
... y el resto de la tabla

## Veredicto
{🟢 APROBAR | 🟡 APROBAR CON CAMBIOS MENORES | 🔴 SOLICITAR CAMBIOS}

### Cambios Requeridos (bloqueantes)
1. ...

### Sugerencias (no bloqueantes)
1. ...
```
