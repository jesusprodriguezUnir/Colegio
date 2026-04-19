---
name: dotnet-unit-tests
description: Genera y gestiona tests unitarios en proyectos .NET Core usando xUnit, NSubstitute y FluentAssertions. Aplica patrones AAA, nomenclatura estándar y cobertura mínima.
license: Private
compatibility: Requiere .NET 8+ SDK, xUnit 2.x, NSubstitute, FluentAssertions.
---

# .NET Core — Generación de Tests Unitarios

Crea, alinea y documenta tests de altísima calidad sin comprometer los tiempos de ejecución y usando directrices que mitigan la temida fragilidad de Testeo.

## Principios Base del Dominio de Tests

1. **Patrón AAA Orgánico** — Arrange (preparación), Act (invocación), Assert (comprobación analítica). Fúndelos en cada Factura o Theory.
2. **El poder de un Assert lógico** — Tu test se diseña en pos de verificar una única aserción vital de negocio (salvo en proyecciones masivas).
3. **Ignora los Ordenes** — Tu suite ha de ser ejecutada al azar (Random parallelization). Un test que demanda completarse tras su predecesor es un test parasitario.
4. **Legibilidad al extremo** — La nomenclatura `NombreDeMetodoBajoPrueba_EscenarioProvocado_EfectoOConsecuenciaValidada`.

## Tu Arsenal Moderno .NET

Asume este ecosistema en tus dependencias de proyectos Test:
- **xUnit** para orquestación de la suite 
- **NSubstitute** para engañar abstracciones (frente a Moq, gracias a su sintaxis lambda más expresiva)
- **FluentAssertions** para redactar validaciones como lenguaje natural
- **AutoFixture** (opcional y recomendable) para dejar de llenar constructores polimórficos de Dummy Texts.

## El Molde Central (Estructurador)

```csharp
using FluentAssertions;
using NSubstitute;
using Xunit;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly UserService _sut; // SUT = System Under Test

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserService>>();
        _sut = new UserService(_userRepository, _logger);
    }
}
```

## Modalidades por Categorías

### A) El Camino de las Rosas (Happy Path)
```csharp
[Fact]
public async Task GetById_ProvidedExistingId_ReturnsMatchingUser()
{
    // Arrange
    var userId = Guid.NewGuid();
    var expectedUser = new User { Id = userId, Name = "Juan" };
    _userRepository.GetByIdAsync(userId).Returns(expectedUser); // NSubstitute

    // Act
    var result = await _sut.GetByIdAsync(userId);

    // Assert (FluentAssertions)
    result.Should().NotBeNull();
    result!.Name.Should().Be("Juan");
}
```

### B) Escenarios Condensados (Parametrizados)
Ahorra código masivo si la firma y la ejecución es monótona:
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public async Task Create_WithCorruptNames_ThrowsValidationException(string? invalidName)
{
    var request = new CreateUserRequest { Name = invalidName };
    var act = () => _sut.CreateAsync(request);
    
    // Testamos la salida controlada al cataclismo evadiendo Assert.Throws clásico
    await act.Should().ThrowAsync<ValidationException>();
}
```

### C) Comportamiento Interno e Interacciones Puras
```csharp
[Fact]
public async Task Delete_UponSuccessfulExecution_TriggersLoggerAndDeleter()
{
    // Arrange & Act (Simplificados)
    await _sut.DeleteAsync(userId);

    // Assert Interaccional
    await _userRepository.Received(1).DeleteAsync(userId);
    _logger.Received(1).LogInformation(Arg.Is<string>(s => s.Contains("eliminado")));
}
```

## Evita los Anti-Patrones Fatales de Unity Testing

| Error Típico | Remedio Drástico |
|-------|----------|
| **Cebos Falsos (Thread.Sleep)** | Evade demoras manuales; inyecta interfaces de servicio `ITimeProvider` pre-mockeables. |
| **Asserts Múltiples de distintos espectros** | Truncamiento y fraccionamiento a Tests independientes. |
| **Uso de async void en Testing** | Un desastre que anula xUnit por el pozo ciego; devuelve `async Task` en el decorador. |
| **Instancias compartidas que se manchan** | Resetea en el Constructor de Instancia en lugar de depender de Setups monolíticos (xUnit insta a usar el `.ctor` como su Setup base individual y virgen). |
