---
name: dotnet-unit-tests
description: Genera y gestiona tests unitarios en proyectos .NET Core usando xUnit, NSubstitute y FluentAssertions. Aplica patrones AAA, nomenclatura estándar y cobertura mínima. Úsala cuando necesites crear, ampliar o auditar tests en soluciones .NET.
license: Private
compatibility: Requiere .NET 8+ SDK, xUnit 2.x, NSubstitute, FluentAssertions.
---

# .NET Core — Generación de Tests Unitarios

Skill para crear, organizar y mantener tests unitarios de alta calidad en proyectos .NET Core.

## Principios

1. **Patrón AAA** — Arrange, Act, Assert en cada test.
2. **Un assert lógico** por test (salvo validaciones de estado compuesto).
3. **Tests independientes** — nunca dependan del orden de ejecución.
4. **Nombres descriptivos** — `Método_Escenario_ResultadoEsperado`.

## Tecnologías Recomendadas

| Paquete | Propósito |
|---------|-----------|
| `xUnit` | Framework de testing |
| `NSubstitute` | Mocks y stubs |
| `FluentAssertions` | Assertions expresivas |
| `Coverlet` | Cobertura de código |
| `AutoFixture` | Generación de datos de prueba |

## Estructura de Proyecto

```
src/
├── MiApp.Domain/
├── MiApp.Application/
└── MiApp.Infrastructure/
tests/
├── MiApp.Domain.Tests/
├── MiApp.Application.Tests/
└── MiApp.Infrastructure.Tests/
```

### Convenciones de Carpetas dentro de Tests

```
MiApp.Application.Tests/
├── Services/
│   ├── UserServiceTests.cs
│   └── OrderServiceTests.cs
├── Handlers/
│   └── CreateOrderHandlerTests.cs
├── Validators/
│   └── CreateOrderValidatorTests.cs
└── Fixtures/
    └── DatabaseFixture.cs
```

## Proceso de Generación

### Paso 1: Analizar la Clase Objetivo

Antes de escribir tests, examina:
- Dependencias inyectadas (interfaces para mockear)
- Métodos públicos (superficie a testear)
- Flujos condicionales (ramas if/else, switch, guard clauses)
- Excepciones esperadas
- Valores de retorno

### Paso 2: Crear la Clase de Test

```csharp
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MiApp.Application.Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly UserService _sut; // Sistema Bajo Prueba

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserService>>();
        _sut = new UserService(_userRepository, _logger);
    }
}
```

### Paso 3: Generar Tests por Categoría

#### Camino Feliz (Happy Path)
```csharp
[Fact]
public async Task GetById_ConIdExistente_RetornaUsuario()
{
    // Arrange
    var userId = Guid.NewGuid();
    var expectedUser = new User { Id = userId, Name = "Juan" };
    _userRepository.GetByIdAsync(userId).Returns(expectedUser);

    // Act
    var result = await _sut.GetByIdAsync(userId);

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be("Juan");
}
```

#### Errores y Casos Límite
```csharp
[Fact]
public async Task GetById_ConIdInexistente_RetornaNull()
{
    // Arrange
    _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

    // Act
    var result = await _sut.GetByIdAsync(Guid.NewGuid());

    // Assert
    result.Should().BeNull();
}

[Fact]
public async Task Create_ConEmailDuplicado_LanzaBusinessException()
{
    // Arrange
    var request = new CreateUserRequest { Email = "ya@existe.com" };
    _userRepository.ExistsByEmailAsync(request.Email).Returns(true);

    // Act
    var act = () => _sut.CreateAsync(request);

    // Assert
    await act.Should().ThrowAsync<BusinessException>()
        .WithMessage("*email*duplicado*");
}
```

#### Tests Parametrizados
```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public async Task Create_ConNombreInvalido_LanzaValidationException(string? nombre)
{
    // Arrange
    var request = new CreateUserRequest { Name = nombre };

    // Act
    var act = () => _sut.CreateAsync(request);

    // Assert
    await act.Should().ThrowAsync<ValidationException>();
}
```

#### Verificación de Interacciones
```csharp
[Fact]
public async Task Delete_ConIdValido_LlamaRepositoryYLogger()
{
    // Arrange
    var userId = Guid.NewGuid();
    _userRepository.GetByIdAsync(userId).Returns(new User { Id = userId });

    // Act
    await _sut.DeleteAsync(userId);

    // Assert
    await _userRepository.Received(1).DeleteAsync(userId);
    _logger.Received(1).LogInformation(Arg.Is<string>(s => s.Contains("eliminado")));
}
```

## Cobertura Mínima

| Capa | Objetivo |
|------|----------|
| Domain | ≥ 90% |
| Application | ≥ 80% |
| Infrastructure | ≥ 60% (sin I/O real) |

### Comando para Medir Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool run reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

## Lista de Verificación de Calidad

- [ ] Cada método público tiene al menos un test de camino feliz
- [ ] Cada excepción documentada tiene un test
- [ ] Los mocks verifican interacciones clave
- [ ] No hay `Thread.Sleep` ni dependencias de tiempo real
- [ ] Tests corren en < 5 segundos individuales
- [ ] No hay dependencias entre tests (orden arbitrario)
- [ ] Nombres siguen `Método_Escenario_ResultadoEsperado`

## Errores Comunes

| Error | Solución |
|-------|----------|
| Test depende de BD real | Usar mocks o testcontainers |
| Asserts múltiples sin relación | Dividir en tests separados |
| Configuración compartida mutable | Usar constructor de xUnit (nuevo por test) |
| Ignorar async/await | Siempre retornar Task en tests asíncronos |
| Fechas fijas en el código | Inyectar `IDateTimeProvider` |

## Integración con CI/CD

```yaml
# Ejemplo en GitHub Actions
- name: Run Tests
  run: dotnet test --configuration Release --logger "trx" --results-directory TestResults
  
- name: Publish Results
  uses: dorny/test-reporter@v1
  with:
    name: .NET Tests
    path: TestResults/*.trx
    reporter: dotnet-trx
```
