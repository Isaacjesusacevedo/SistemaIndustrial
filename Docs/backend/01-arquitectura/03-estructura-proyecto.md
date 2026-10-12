# Estructura del Proyecto Backend (3 Capas: AD → BR → API)

## Árbol de Directorios (Solution)

```
SistemaIndustrial.sln
├── SistemaIndustrial.AD/              # Data Access Layer
│   ├── Data/                          # AppDbContext (EF Core)
│   │   └── AppDbContext.cs
│   ├── Exceptions/                    # Excepciones de dominio
│   │   ├── EntityNotFoundException.cs
│   │   ├── PersistenceException.cs
│   │   └── UnauthorizedAccessException.cs
│   ├── Interfaces/                    # Contratos de repositorios
│   │   ├── IRepository.cs             # Genérica base
│   │   ├── ISemanaRepository.cs       # + ExistsByNumeroAnio, HasDependentData (SPs)
│   │   ├── IInsumoRepository.cs       # SPs
│   │   ├── IStockRepository.cs        # SPs
│   │   ├── IRegistroTachoRepository.cs # SPs (código existente, sin DI ni endpoint mapeado)
│   │   ├── IUsuarioRepository.cs      # SPs
│   │   ├── ICategoriaRepository.cs    # EF Core directo
│   │   ├── ITiradoProductoRepository.cs # EF Core directo (+ Tiradas)
│   │   └── IProductoCatalogoRepository.cs # SPs (sp_ProductosCatalogo_*)
│   ├── Models/                        # Entidades de dominio (AD)
│   │   ├── Semana.cs
│   │   ├── InsumoSemana.cs
│   │   ├── StockSemana.cs
│   │   ├── RegistroTacho.cs           # Modelo existe; sin DbSet en AppDbContext
│   │   ├── Categoria.cs               # NUEVO (EF directo)
│   │   ├── TiradoProducto.cs          # NUEVO (EF directo)
│   │   ├── Tirada.cs                  # NUEVO (EF directo)
│   │   ├── ProductoCatalogo.cs        # NUEVO (catálogo general, SPs)
│   │   └── Usuario.cs
│   ├── Repositories/                  # Implementaciones (SPs o EF Core directo)
│   │   ├── SemanaRepository.cs        # SPs
│   │   ├── InsumoRepository.cs        # SPs
│   │   ├── StockRepository.cs         # SPs
│   │   ├── RegistroTachoRepository.cs # SPs (sin DI)
│   │   ├── UsuarioRepository.cs       # SPs
│   │   ├── CategoriaRepository.cs     # EF Core directo
│   │   ├── TiradoProductoRepository.cs # EF Core directo
│   │   └── ProductoCatalogoRepository.cs # SPs
│   ├── SpInvoker.cs                   # Helpers ADO.NET async para SPs (QueryEntityAsync, ExistsAsync, ScalarAsync, NonQueryAsync)
│   ├── SistemaIndustrial.AD.csproj
│   └── Dependencies: Microsoft.Data.SqlClient, Microsoft.EntityFrameworkCore.SqlServer
│
├── SistemaIndustrial.BR/              # Business Rules Layer
│   ├── DTOs/                          # DTOs de servicio (records)
│   │   ├── CommonDtos.cs              # ServiceResult, ServiceResult<T>
│   │   ├── AuthDtos.cs                # LoginRequest, LoginResponse, UserInfo, TokenPayload
│   │   ├── SemanaDtos.cs              # SemanaCreateDto, SemanaUpdateDto, SemanaResponse
│   │   ├── InsumoDtos.cs
│   │   ├── StockDtos.cs
│   │   ├── RegistroTachoDtos.cs       # Código existente (servicio sin DI)
│   │   ├── CategoriaDtos.cs           # NUEVO
│   │   ├── TiradoProductoDtos.cs      # NUEVO (+ bulk y RegistrarTirada)
│   │   ├── TiradaDtos.cs              # NUEVO
│   │   ├── ProductoCatalogoDtos.cs    # NUEVO
│   │   └── DashboardDtos.cs
│   ├── Interfaces/                    # Contratos de servicios
│   │   ├── IServices.cs               # ISemanaService, IInsumoService, IStockService (+SyncFromTirado), IDashboardService, ITiradoProductoService (+bulk), ICategoriaService
│   │   ├── IAuthService.cs
│   │   └── IProductoCatalogoService.cs # NUEVO
│   ├── Services/                      # Implementaciones de lógica de negocio
│   │   ├── SemanaService.cs
│   │   ├── InsumoService.cs
│   │   ├── StockService.cs            # + SyncFromTiradoAsync (usa ITiradoProductoService)
│   │   ├── RegistroTachoService.cs    # Código existente (sin registro DI)
│   │   ├── DashboardService.cs        # Métricas desde Tirado + Insumo + Stock
│   │   ├── AuthService.cs
│   │   ├── CategoriaService.cs        # NUEVO
│   │   ├── TiradoProductoService.cs   # NUEVO (+ bulk, RegistrarTirada)
│   │   └── ProductoCatalogoService.cs # NUEVO
│   ├── Validators/                    # Validaciones de negocio (clases estáticas)
│   │   ├── SemanaValidator.cs         # Lunes-sábado, 6 días exactos
│   │   ├── InsumoValidator.cs
│   │   ├── StockValidator.cs
│   │   ├── RegistroTachoValidator.cs
│   │   ├── CategoriaValidator.cs      # NUEVO
│   │   ├── TiradoProductoValidator.cs # NUEVO
│   │   └── ProductoCatalogoValidator.cs # NUEVO
│   ├── SistemaIndustrial.BR.csproj
│   └── Dependencies: SistemaIndustrial.AD, BCrypt.Net-Next, Microsoft.Extensions.Configuration
│
├── SistemaIndustrial.API/             # Presentation Layer (ASP.NET Core 9 Minimal APIs)
│   ├── Endpoints/                     # Extension methods por dominio (MapGroup)
│   │   ├── AuthEndpoints.cs           # /api/auth (AllowAnonymous)
│   │   ├── SemanaEndpoints.cs         # /api/semanas
│   │   ├── InsumoEndpoints.cs         # /api/insumos
│   │   ├── StockEndpoints.cs          # /api/stock (+ sync-from-tirado)
│   │   ├── TiradoProductoEndpoints.cs # /api/tirado (+ bulk, tirar, tachos, filtro) — NUEVO
│   │   ├── CategoriaEndpoints.cs      # /api/categorias — NUEVO
│   │   ├── ProductoCatalogoEndpoints.cs # /api/productos — NUEVO
│   │   ├── RegistroTachoEndpoints.cs  # Código existente, NO mapeado en Program.cs
│   │   └── DashboardEndpoints.cs      # /api/dashboard
│   ├── Models/                        # DTOs de API (Request/Response con DataAnnotations)
│   │   └── ApiModels.cs               # ApiResponse<T>, PagedResponse<T>, *CreateRequest/*UpdateRequest (+ Tirado, Categoría, Catálogo)
│   ├── Middleware/                    # Vacío actualmente (el manejo global de errores está inline en Program.cs)
│   ├── Program.cs                     # Composition root: DI directa, CORS "VueCors", JWT, Scalar (Dev), Exception Handler, health (/health, /health/ready, /health/live)
│   ├── appsettings.json               # Config base (LocalDB, 1 origen CORS)
│   ├── appsettings.Development.json   # Config desarrollo (SQL Auth + JWT + 3 orígenes CORS)
│   ├── SistemaIndustrial.API.csproj
│   └── Dependencies: SistemaIndustrial.BR, JWT Bearer, OpenAPI nativo + Scalar (sin Swashbuckle)
│
├── SistemaIndustrial.AD.Test/         # Unit tests AD (MSTest + Moq)
├── SistemaIndustrial.BR.Test/         # Unit tests BR (MSTest + Moq)
├── SistemaIndustrial.API.Test/        # Integration tests API (MSTest + WebApplicationFactory)
│
├── Database/                          # Scripts SQL compartidos (opcional, ver Docs/sql/)
│
├── Docs/                              # Documentación
│   └── (ver Docs/README.md)
│
├── Frontend/                          # Vue 3 + TypeScript + Vite (ver estructura en Docs/frontend/)
│
└── README.md
```

## Responsabilidades por Capa

### API Layer (`SistemaIndustrial.API`)

- **Reciben** HTTP requests, validan entrada (`ModelState`, DTOs)
- **Delegan** a servicios BR (no contienen lógica de negocio)
- **Manejan** excepciones de dominio → HTTP status codes
- **Retornan** JSON serializado (`ServiceResult<T>` / `ApiResponse<T>`)
- **Middleware**: CORS, JWT Authentication, Authorization, Global Exception Handler, Swagger (Dev)
- **Organización**: Endpoints en carpeta `Endpoints/` como extension methods `MapXxxEndpoints(this IEndpointRouteBuilder app)` con `MapGroup` por recurso; **modo demo**: `RequireAuthorization` comentado (acceso libre)

### BR Layer (`SistemaIndustrial.BR`)

- **Contienen** lógica de negocio y reglas de validación
- **Orquestan** validaciones cruzadas (ej. Objetivo valida Semana existe)
- **Ejecutan** operaciones via Repositories (AD)
- **Manejan** `ServiceResult<T>` pattern para éxito/fallo tipado
- **Servicios**: `SemanaService`, `InsumoService`, `StockService` (+`SyncFromTiradoAsync`), `DashboardService`, `AuthService`, `CategoriaService`, `TiradoProductoService` (+bulk), `ProductoCatalogoService` (`RegistroTachoService` existe pero sin registro DI)
- **Auth**: `AuthService` con BCrypt (workFactor: 12), JWT generation, Refresh tokens

### AD Layer (`SistemaIndustrial.AD`)

- **Repositorios** tipados por entidad (`ISemanaRepository`, `ITiradoProductoRepository`, etc.): SPs vía `SpInvoker` (Semana, Insumo, Stock, Usuario, RegistroTacho, ProductoCatalogo) o EF Core directo (Categoria, TiradoProducto)
- **SpInvoker**: ADO.NET async para invocar Stored Procedures (`QueryEntityAsync`, `ExistsAsync`, `ScalarAsync`, `NonQueryAsync`); sin `FromSqlRaw` (mapeo manual por reflexión case-insensitive)
- **AppDbContext**: EF Core DbContext con DbSets para cada entidad
- **SQL**: Parameterizado, con SPs, sin reflexión innecesaria
- **Entidades AD**: Separadas de BR/API, sin dependencias externas
- **Excepciones**: `EntityNotFoundException`, `PersistenceException`, `UnauthorizedAccessException`

---

## Modelos de Dominio (AD Layer)

```mermaid
classDiagram
    class Usuario {
        +int Id
        +string Email
        +string PasswordHash
        +string Nombre
        +string Role
        +string? RefreshTokenHash
        +DateTime? RefreshTokenExpiry
        +bool Activo
        +DateTime FechaCreacion
    }

    class Semana {
        +int Id
        +int NumeroSemana
        +int Anio
        +DateTime FechaInicio
        +DateTime FechaFin
        +string Estado
        +DateTime FechaCreacion
    }

    class Categoria {
        +int Id
        +string Nombre
        +int Orden
        +bool Activa
    }

    class TiradoProducto {
        +int Id
        +int SemanaId
        +int? CategoriaId
        +string Nombre
        +string UnidadMedida
        +decimal PedidoSemanal
        +decimal StockInicial
        +decimal PromedioPorTacho
        +decimal CantidadTirada
        +decimal Stock
        +string? Observaciones
        +DateTime FechaCreacion
        +DateTime? FechaActualizacion
    }

    class Tirada {
        +int Id
        +int TiradoProductoId
        +DateTime Fecha
        +decimal Cantidad
        +string? Observaciones
        +DateTime FechaCreacion
    }

    class ProductoCatalogo {
        +int Id
        +string Nombre
        +string UnidadMedida
        +decimal PedidoSemanalAproximado
        +decimal PromedioPorTacho
        +bool Activo
        +string? Observaciones
        +DateTime FechaCreacion
        +DateTime? FechaActualizacion
    }

    class InsumoSemana {
        +int Id
        +int SemanaId
        +string Nombre
        +string UnidadMedida
        +decimal CantidadRequerida
        +decimal CantidadConsumida
        +decimal StockDisponible
        +DateTime FechaCreacion
    }

    class StockSemana {
        +int Id
        +int SemanaId
        +string Producto
        +string UnidadMedida
        +decimal StockInicial
        +decimal Entradas
        +decimal Salidas
        +decimal StockFinal
        +DateTime FechaCreacion
    }

    class RegistroTacho {
        +int Id
        +int SemanaId
        +int Turno
        +string Operador
        +decimal CantidadProducida
        +decimal CantidadRechazada
        +string EstadoCalidad
        +DateTime FechaRegistro
        +DateTime FechaCreacion
    }

    Semana "1" --> "0..*" InsumoSemana : requiere
    Semana "1" --> "0..*" StockSemana : gestiona
    Semana "1" --> "0..*" TiradoProducto : tira
    Categoria "1" --> "0..*" TiradoProducto : clasifica
    TiradoProducto "1" --> "0..*" Tirada : registra
    RegistroTacho "1" --> "1" Semana : referencia (modelo sin DbSet; endpoint no mapeado)
```

### DTOs (BR Layer)

**Input (Request):**
- `LoginRequest` (record) — email + password
- `RegisterRequest` — email + password + nombre + role
- `ChangePasswordRequest` — currentPassword + newPassword
- `SemanaCreateDto` — numeroSemana, anio, fechaInicio, fechaFin
- `SemanaUpdateDto` — numeroSemana, anio, fechaInicio, fechaFin, estado
- `InsumoCreateDto`, `InsumoUpdateDto`
- `StockCreateDto`, `StockUpdateDto`
- `RegistroTachoCreateDto`, `RegistroTachoUpdateDto` (código sin DI)
- `CategoriaCreateDto`, `CategoriaUpdateDto` (NUEVO)
- `TiradoProductoCreateDto`, `TiradoProductoUpdateDto`, `RegistrarTiradaDto`, `TiradoProductoBulkCreateDto` (NUEVO)
- `ProductoCatalogoCreateDto`, `ProductoCatalogoUpdateDto` (NUEVO)

**Output (Response):**
- `ServiceResult<T>` — wrapper éxito/fallo tipado
- `ServiceResult` — wrapper sin datos
- `LoginResponse` (record) — token, refreshToken, expiresAt, user
- `UserInfo` (record) — id, email, nombre, role
- `SemanaResponse` — id, numeroSemana, anio, fechaInicio, fechaFin, estado, fechaCreacion, insumosCount
- `TiradoProductoResponse` — incluye CategoriaNombre, CantidadTachos, StockTotal, TachosEstimados, Estado (calculados)
- `TiradaResponse`, `CategoriaResponse`, `ProductoCatalogoResponse`
- `DashboardMetricasDto` — KPIs desde Tirado + Insumo + Stock (sin objetivos)
- `DashboardTendenciaDto` — series temporales (produccion, rechazos, calidad)
- `DashboardAlertaDto` — alertas automáticas

---

## Exceptions (Cross-Cutting)

| Excepción | HTTP Status | Uso |
|-----------|-------------|-----|
| `EntityNotFoundException` | 404 | Recurso no encontrado |
| `PersistenceException` | 500 | Error BD / IO / Constraint violation |
| `UnauthorizedAccessException` | 401 | Credenciales inválidas / token expirado / sin permisos |

---

## Convenciones de Nombres

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| **Projects** | `SistemaIndustrial.{Layer}` | `SistemaIndustrial.AD`, `SistemaIndustrial.BR`, `SistemaIndustrial.API` |
| **Endpoints** | `{Entidad}Endpoints` | `SemanaEndpoints`, `AuthEndpoints` |
| **Services** | `{Entidad}Service` | `SemanaService`, `AuthService` |
| **Repositories** | `{Entidad}Repository` | `SemanaRepository`, `UsuarioRepository` |
| **Interfaces** | `I{Funcionalidad}` | `ISemanaRepository`, `ISemanaService`, `IAuthService` |
| **DTOs Input** | `{Accion}{Entidad}Dto` | `SemanaCreateDto`, `LoginRequest`, `ChangePasswordRequest` |
| **DTOs Output** | `{Entidad}Response` / `ServiceResult<T>` | `SemanaResponse`, `LoginResponse`, `ServiceResult<Semana>` |
| **Exceptions** | `{Contexto}Exception` | `PersistenceException`, `EntityNotFoundException` |
| **Result Pattern** | `ServiceResult<T>` | `ServiceResult<Semana>`, `ServiceResult` |
| **SQL Tables** | Plural PascalCase | `Semanas`, `Usuarios`, `TiradosProducto`, `ProductosCatalogo` |
| **SQL Columns** | PascalCase | `NumeroSemana`, `FechaInicio`, `PasswordHash` |
| **SQL Params** | `@NombreParametro` | `@NumeroSemana`, `@Anio`, `@Email` |
| **MapGroup** | `/api/{recurso}` | `/api/tirado`, `/api/productos`, `/api/categorias` |