# Tests Unitarios

Proyectos MSTest (.NET 9) con Moq y EF Core InMemory. Se corren con:

```bash
dotnet test SistemaIndustrial.sln
```

> **Nota:** detener la API en ejecución antes de compilar/testear. El target
> `CopyADtoAPI` copia los DLL a la carpeta de la API y falla si el proceso
> los tiene bloqueados (`error MSB3027/MSB3021`).

## SistemaIndustrial.BR.Test (24 tests)

Referencia a BR + AD. Sin base de datos (validators estáticos + Moq).

| Archivo | Qué cubre |
|---------|-----------|
| `SemanaValidatorTests.cs` (7) | `ValidarFechasSemana`: lunes→sábado válido, inicio no lunes, fin no sábado, duración ≠ 5 días, semana lunes→domingo rechazada, `NumeroSemana` fuera de rango |
| `ProductoCatalogoValidatorTests.cs` (7) | Nombre requerido/largo, unidad requerida, valores negativos, observaciones > 500, update con `Activo=false` |
| `TiradoProductoValidatorTests.cs` (4) | `CantidadTirada` negativa/cero/ausente en update, tirada negativa |
| `SemanaServiceTests.cs` (3, Moq `ISemanaRepository`) | Crea semana válida (estado `Planificada`), duplicado no toca el repository (`Times.Never`), fechas inválidas no tocan el repository |
| `ProductoCatalogoServiceTests.cs` (3, Moq `IProductoCatalogoRepository`) | Crea activo, nombre duplicado no toca el repository, delete inexistente |

## SistemaIndustrial.AD.Test (6 tests)

Referencia a AD. Sin SQL Server (defaults puros + EF Core InMemory).

| Archivo | Qué cubre |
|---------|-----------|
| `ModelDefaultsTests.cs` (3) | Defaults: `ProductoCatalogo.Activo=true`, `Semana.Estado="Planificada"`, `TiradoProducto.Tiradas` vacía |
| `AppDbContextTests.cs` (3) | El modelo compila (`OnModelCreating`) y CRUD InMemory de `Semana`, `ProductoCatalogo` y `TiradoProducto` con `Tiradas` |

## SistemaIndustrial.API.Test (4 tests)

Referencia a API + BR + AD. Sin servidor (DataAnnotations + wrappers, no `WebApplicationFactory`).

| Archivo | Qué cubre |
|---------|-----------|
| `ApiModelsValidationTests.cs` (4) | `ProductoCatalogoCreateRequest` válido/sin nombre, `TiradoProductoUpdateRequest` con cantidad negativa, `ApiResponse<T>.Ok` |

## Pendiente

- Tests de integración con `WebApplicationFactory` (`Microsoft.AspNetCore.Mvc.Testing` ya referenciado en API.Test) — requieren SQL Server o Testcontainers.
- Tests de `DashboardService` y `StockService.SyncFromTiradoAsync` (necesitan varios mocks; patrón disponible en `SemanaServiceTests`).
