---
name: dotnet-concurrency
description: Elige la abstracción de concurrencia correcta en .NET — desde async/await para I/O hasta Channels para productor/consumidor y Akka.NET para gestión de entidades con estado.
license: Private
compatibility: Requiere .NET 8+ SDK.
---

# Concurrencia .NET: Eligiendo la herramienta adaptada

## Cuándo usar esta skill

Acude a este archivo cuando:
- Has de definir cómo atajar hilos de ejecución concurrentes en operaciones de .NET.
- Evaluando las directivas `async/await` clásicas contra `Channels`, u otros modelos en cascada.
- Tienes la tentación de lanzar primitivas de `lock` o semáforos a diestro y siniestro.
- Administras cuellos de botella controlados, agrupamiento por lotes (batching) o control de peticiones en desborde.

## La Filosofía .NET Contemporánea

**Comienza en lo simple, escala gradualmente y solo si hay pruebas físicas irrebatibles de hacerlo.**

El 95% de la concurrencia normal se solventa combinando con destreza `async/await`.

**Tus prioridades cuando de estado mutado concurrente se trata:**
1. **Prioridad Máxima**: Rediseña la estructura en aislamiento o delegación explícita (mensajería) para NO compartir estado.
2. **Prioridad Secundaria**: Usa variables o colecciones especializadas como `System.Collections.Concurrent` (Ejemplo: `ConcurrentDictionary`).
3. **Prioridad Terciaria**: Usa `Channel<T>` para canalizar peticiones sin trabar memoria simultáneamente.
4. **Resorte Primitivo (y Final)**: Utilización breve de primitivas limitadas con `lock`.

---

## Árbol de decisiones y atajos mentalese

```
¿Qué estamos intentando modelar o resolver?
│
├─► ¿Espera de lectura/escritura (BBDD, HTTP, FileSystem)?
│   └─► Usa async/await habitual
│
├─► ¿Acumular listas gigantescas y procesar las variables (Problemas con el CPu bound)?
│   └─► Parallel.ForEachAsync 
│
├─► ¿Modelo Productor / Consumidor?
│   └─► System.Threading.Channels
│
├─► ¿Acumulación temporal de eventos de interfaz y "debounce"?
│   └─► Extensiones y flujos Reactivos (Rx)
│
└─► ¿Unión o gestión de decenas de operaciones aisladas ya lanzadas?
    └─► Task.WhenAll / Task.WhenAny para aguardar su resolución conjunta o la primera completada
```

## Level 1: `async/await` tradicional

Es la capa fundacional. Para esperar a la red, acceso a disco, acceso relacional de BBDD. Asegúrate SIEMPRE de dotar y arrastrar el parámetro predefinido `CancellationToken ct` hasta el final. Nunca bloquees una tarea con un síncrono `.Result` sobre su cabecera Task.

## Level 2: `Parallel.ForEachAsync`
Apto para concurrencia dependiente estrechamente del procesador y recursos CPU donde las variables no interfieren entre sí.

## Level 3: `System.Threading.Channels` (Productor/Consumidor)

Utilizado masivamente para procesos y workers asíncronos que corren en Background sin acoplar dependencias a quienes los impulsan en el Request.
Provee utilidades sofisticadas para generar buffers en memoria local y definir reglas de desborde y Backpressure.

```csharp
_channel = Channel.CreateBounded<Order>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait // Bloquea al productor sutilmente mientras hay un pico
});
```

## Antipatrones Frecuentes

### Peligro: Gestión humana de hilos

```csharp
// INCORRECTO: Hilos manuales que escapan al hilo principal de procesos
var thread = new Thread(() => ProcessOrders());
thread.Start();

// CORRECTO: Manejo del Pool de Procesos asíncronos con envoltorio de supervisión controlada
_ = Task.Run(() => ProcessOrdersAsync(cancellationToken));
```

### Tareas superpuestas sin Mutex

```csharp
// INCORRECTO: Colecciones estándar perdiendo datos si hay dos threads
var results = new List<Result>();
await Parallel.ForEachAsync(items, async (item, ct) =>
{
    var result = await ProcessAsync(item, ct);
    results.Add(result); // Condiciones de Carrera en List<> causarán corrupción
});

// CORRECTO: Usar entidades concebidas para esta tarea
var results = new ConcurrentBag<Result>();
```
