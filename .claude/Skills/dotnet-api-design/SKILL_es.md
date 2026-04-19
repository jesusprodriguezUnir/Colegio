---
name: dotnet-api-design
description: Diseña APIs públicas estables y compatibles usando principios de diseño extend-only. Gestiona compatibilidad de API, de wire y de binario, y versionado para paquetes NuGet y sistemas distribuidos.
license: Private
compatibility: Requiere .NET 8+ SDK.
---

# Diseño de API Pública y Compatibilidad

## Cuándo usar esta habilidad

Recurre a esta habilidad cuando:
- Entables la creación y diseño del API pública de paquetes NuGet abiertos y extensivos.
- Te prepares para mutar o modificar firmas y trazas en APIs ya publicadas.
- Desarrolles o modifiques los serializadores ('Wire Formats') de tu proyecto, que entablan comunicación vía Red (Distributed Systems).
- Establezcas cómo versionar evolutivamente y planificar los ciclos de vida y desuso (Deprecation).

## 3 Vertientes de la Compatibilidad en C# / .NET

| Variante | Descripción Técnica y Definición | Alcance Global |
|------|------------|-------|
| **API/Source** | Permite que código externo sea recompilado sin cambiar ni una coma si se sube de versión | Firmas públicas y nombres de tipos |
| **Binaria** | El código binario existente (ya compilado) NO estalla ('Crash') ni se lanza sin modificar librerías DLL al ejecutarse bajo el paraguas actualizado | Layout orgánico y metadata de los métodos internos vinculados en la invocación (Method Tokens). |
| **Wire (Serialización)** | La data en Bytes viaja bidireccionalmente y puede traducirse libremente por nuevas y previas versiones | Json, Protocol Buffers, Data en Red |

Cualquier grieta o ruptura sobre estas reglas detona fricciones insoportables para los clientes adoptivos.

---

## Patrón de "Extend-Only" (Exclusivo por Ampliación)

El cimiento inamovible de un API a prueba de bombas reza así: **jamás mutiles ni mutans, únicamente amplia.**

### Tres Bastiones Fundamentales

1. **La maquinaria previa es Sacra (Inmutable)** - Ya subió a producción; queda blindada eternamente.
2. **Trazos nuevos sobre lienzos paralelos** - Despliega sobrecargas puras (`overloads`) o adhiere clases extra limitadas sin tocar la heráldica y el árbol genealógico del modelo primitivo.
3. **Muerte anunciada, no silenciosa** - Los retiros a los infiernos (`Deprecation / Remove`) necesitan ciclos de advertencias de Años, no Semanas.

---

## Puntos Clínicos en la Edición

### Modificaciones Seguras e Irreprochables (Safe Changes)

```csharp
// SEGURO: Desplegar una sobrecarga extra que interpele sigilosamente a tu propio ente primitivo
public void Process(Order order) { ... }  // Método inmaculado

// Nueva sobrecarga - Vía libre
public void Process(Order order, CancellationToken ct) { ... }

// SEGURO: Aportar nuevos contratos o Enumeradores al namespace público sin interponerse en la funcionalidad pasada.
public interface IOrderValidator { }
```

### Alteraciones Defectuosas o Fatales (Jamás hagas esto sin subir Mayor Release)

```csharp
// ERROR: Borrar cosas de la vista
public void ProcessOrder(Order order);  // Era: Process()

// ERROR ALARMISTA: Anadir parámetros extras (incluso opcionales!) en métodos vigentes 
// Destroza por completo la compatibilidad Binaria. Aunque tu API compilará
// los clientes antiguos no resolverán el "signature token" compilado sufriendo MissingMethodException 
public void Process(Order order, CancellationToken ct = default); // ❌ ¡CAOS BINARIO!
```

**Si deseas dar modernidad a un método carente de `CancellationToken`, debes conservar la antigua firma envolviéndola internamente para hacerla llamar a tu nuevo método ampliado.**

### Metódica de Clausura y Patrón Deprecation

```csharp
// FASE 1: Marcas como veneno radiactivo suave a los usuarios con aviso.
[Obsolete("En desuso en v1.5.0. Da el salto urgente a ProcessAsync")]
public void Process(Order order) { }

// FASE 2: Proporcionas la cura.
public Task ProcessAsync(Order order, CancellationToken ct = default);

// FASE 3: Eliminas todo (Sólo al propulsar cambio MASIVO de Versionado "Mayor v2.0")
```

---

## Tolerancia Estructural y Wire

Incluso superadas las DLLs locales, la mensajería serializada (Rabbit, Kafka, o persistencia local BBDD en JSON) arrastrará tu dominio para siempre y lo interpelarán microservicios ancianos en producción o consumidores desfasados.

- **Preferir discriminadores explícitos**: Explotar los discriminadores sobre tipologías de herencia que dejen patente la familia ("Type=Cat" frente a acoplamientos cerrados "$type=Namespace.Gato").
- **Tácticas Schema-First**: Los contratos `Protobuf` o `JsonSourceGen` obligan a indexar deudas sin sorpresas de reflection que arruinen los deserializadores opuestos. 
- **Nunca elimines un campo "Wire"**: Si desestimas un campo en un sistema interno de comunicaciones, mantenlo como "Obsoleto o reservado", e ignóralo, pero asume que viajará como un fantasma durante mucho tiempo provocado por escritores de versiones arcaicas del lado del Cliente.
