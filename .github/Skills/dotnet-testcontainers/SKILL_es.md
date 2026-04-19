---
name: dotnet-testcontainers
description: Escribe tests de integración usando TestContainers para .NET con xUnit. Cubre testing de infraestructura con bases de datos reales, colas de mensajes y cachés en contenedores Docker en lugar de mocks.
license: Private
compatibility: Requiere .NET 8+ SDK, Docker, xUnit 2.x.
---

# Integration Testing con TestContainers

## Cuándo usar esta skill

Deberás usar este material cuando:
- Crees tests de integración que toquen infraestructura vital (bases de datos transaccionales, cachés temporales, message brokers).
- Compruebes que tus repositorios interactúan limpiamente sobre un motor SQL vivo.
- Valides la recepción e inyección en colas de tipo RabbitMQ/Kafka.
- Quieras dejar atrás el frágil arte de *mockear* interfaces de infraestructura.
- Quieras prevenir errores de sintaxis o motor SQL garantizando el testeo en entornos Docker que imitan fehacientemente Producción (Shift-Left validation).

## Principios Críticos

1. **Infraestructura Real por encima de los Mocks**: Si la BBDD se invoca, lo hará contra un contenedor, nunca ante la abstracción de una interfaz de C#.
2. **Aislamiento Pulcro (Test Isolation)**: Todo arranque de entorno dota al test de una imagen virgen.
3. **Limpieza Automatizada y Autogestionada**: Olvida los scripts de desmontaje. Las apis de interop de *TestContainers* desmantelan las huellas locales.
4. **Resurrección y Reutilización**: Los Tests dentro de un `ClassFixture` pueden aprovechar la misma instancia arrancada.
5. **No hay Conflictos de Puertos**: TestContainers amarra la máquina Dockerizada a un puerto aleatorio de tu tarjeta local en cascada.

## El Gran Problema del Testing basado en Mocks

La burla (Mocking) frente a `IDbConnection` o `DbContext` produce complacencia, donde tus pruebas pasan en verde localmente, pero al entrar a producción el programa colapsa ante un fallo sutil de FK (Claves Foráneas) o una simple coma olvidada en un *String Interpolation*.

El ecosistema actual pide probar con el framework en frío y la BBDD a plena potencia, validando restricciones, transacciones puras y rendimientos genuinos.

## Receta Básica con Base de Datos

```csharp
public class OrderRepositoryTests : IAsyncLifetime // Requisito vital del ciclo de xUnit
{
    private readonly TestcontainersContainer _dbContainer;
    private IDbConnection _connection;

    public OrderRepositoryTests()
    {
        // Construimos la imagen al Vuelo
        _dbContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "Super_password123")
            .WithPortBinding(1433, true) // Activa el Port Randomization (true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync(); // Levantamos el contenedor
        var port = _dbContainer.GetMappedPublicPort(1433);
        var connectionString = $"Server=localhost,{port};Database=TestDb;User Id=sa;Password=Super_password123;TrustServerCertificate=true";
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();
        
        // Lanzamos un script generador de las tablas iniciales
        await RunMigrationsAsync(_connection);
    }
    
    // ... Tu Fact o Theory irá aquí operando sobre el _connection 

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _dbContainer.DisposeAsync(); // Mantenemos el PC de los devs limpio.
    }
}
```

## Buenas Prácticas Avanzadas

1. **Reutiliza Contenedores entre Suites Combinadas** - Arrancar MSSQL puede demorar 15 segundos. Usa `ICollectionFixture<T>` en xUnit para arrancar el SQL de pruebas solo vez por ejecución y purgar luego los datos intermedios con librerías eficientes.
2. **Purgado Ultrarápido: Patrón Respawn** - Antes de cada método Fact, limpia la Base de datos generada empleando el NuGet *Respawn*. Respeta las Foreing Keys y restablece tu contenedor sin recrearlo, arañando minutos en tus Pipelines de CI/CD.
3. **No fijes los puertos locales .WithPortBinding(1433, 1433)** - Esto detonará inmediatamente conflictos entre los tests asíncronos y paralelos destruyendo tus procesos y builds de GitHub Actions. Siempre deriva su responsabilidad al motor y captura el puerto variable con `GetMappedPublicPort()`.
