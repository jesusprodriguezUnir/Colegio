---
name: dotnet-refactor
description: Analiza y refactoriza código .NET Core aplicando principios SOLID, código limpio y patrones de diseño. Detecta malos olores, propone mejoras estructurales y devuelve tu fragmento reconstruido.
license: Private
compatibility: Requiere .NET 8+ SDK. Compatible con C# 12.
---

# .NET Core — Refactorización Inteligente de Deuda Técnica

Emplea esta habilidad para diseccionar piezas y entresijos de componentes (o arquitecturas concretas) y promover una reforma drástica en base a Clean Code, refactorizando a un estado legible, elástico y de calidad sin quebrar las regresiones lógicas subyacentes.

## Mandamientos Principales de la Acción

1. **La refactorización es un proceso Ciego al Comportamiento**: No alteres reglas ni cálculos; el Output idéntico debe persistir inmutable.
2. **Pequeños pasos incrementales**: Propagación atómica, aislando cada cirugía pequeña en sí.
3. **Escritura amigable con el Humano**: Redactar para compresión de lectura humana y fluida prevalece a tratar de ganar milisegundos de CPU usando operadores "crípticos".
4. **Acero YAGNI**: Sólo desarrolla y extrae lo que te permita escalar y solventar las molestias actuales. Evita interfaces genéricas prematuras pensando a cinco años vista.

## El Escáner de Olores Sintomáticos

Afronta el escrutinio de los archivos buscando estas lacras estructurales:

| Detonante Identificable | ¿Por qué y Magnitud? | Alarma |
|-----------|-------------|-----------|
| **Bloques Mastodónticos** | Funciones que engloban +20 lógicas operativas | 🔴 Critica |
| **Monstruos Dioses (Clases)** | > 300 franjas o usurpadoras de > 5 quehaceres desvinculados | 🔴 Crítica |
| **Batería de Atributos excesiva** | Métodos aglutinando firmas de 5 parámetros o más | 🟡 Intermedia |
| **Copy/Paste descarado** | Sentencias iterando igual en varios frentes | 🔴 Critica |
| **Anidaciones Interminables (Arrow Code)** | Profundidad de validaciones `if/if/if/for/if` de alto calibre | 🟡 Intermedia |
| **Bautizos ambiguos** | Operaciones sin contexto visual tipo `res`, `dt1`, `finalVal` | 🟡 Intermedia |
| **Comentarios intrusivos que suplantan a buenos métodos** | Si has de explicar QUÉ hace en 4 líneas para que se entienda, ese bloque es un método encubierto carente de nombre independiente | 🟠 Intermedia |
| **Envidia ajena extrema** | Una clase invirtiendo energía en llamar múltiples veces a otra en lugar de pedir que ella misma se componga y opere las matemáticas | 🟡 Intermedia |

## Arsenal Refactorizador según los Agravios Diagnosticados

### Táctica Básica 1: Extraer Método Auxiliar o Extensión (Responsabilidad Única)
Secciona los monstruos en fracciones de trabajo más menudas. 

### Táctica 2: Polimorfismo frente a Switch Gigantes
Suele emplearse el Patrón Estrategia (Strategy Pattern) o inyecciones condicionadas (Keyed Services dictados en .NET 8) para delegar las lógicas de `switch`.

### Táctica 3: Enclaustrar Multitud Parámetros en una Pildora C#9
Convierte 6 parámetros independientes en un bonito, moderno e inmutable `Objeto DTO / Request Record`:
```csharp
public record ReportRequest(DateRange Period, string Department, bool IncludeInactive = false, string Format = "pdf");
```

### Táctica 4: Tipar Resultados Explícitos para obviar Exceptions
Eliminar la propagación de validaciones o bucles por ramas falsas que se apoyen en pesadas instrucciones de Throw/Catch. Usa patrones base genéricos de clase envoltorio tipo `Result<T>`.

### Táctica 5: Extracción de Clases o Escisión de Servicios
Separa un `UserService` generador de avatares, correos y usuarios en las subunidades `AvatarService`, `NotificationSystem` y `UserMgmt`.

## Formato del Checklist Final y Verificación Post-Refactor

Al redactarle el documento de validación, somete tu operación a esta rigurosa rúbrica visual:

- [ ] Todos los Tests siguen indemnes y pasando impunemente.
- [ ] No me he colado y he metido características funcionales extrañas o nuevas (sólo he "limpiado").
- [ ] Ya no persisten clones de código duplicado ni "ruído".
- [ ] La "Complejidad Ciclomática" real ha bajado; los métodos de validaciones guardan formato de Return Preventiva prematura (Cláusulas guarda).
- [ ] He erradicado los "Numbers Mágicos" introducidos a fuego por variables de Contexto y const nombradas.
