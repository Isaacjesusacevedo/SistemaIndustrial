# Patrones de Diseño

## Resumen

El backend utiliza patrones de diseño probados para mantener la separación de responsabilidades, testabilidad y mantenibilidad.

## Patrones Implementados

### 1. Repository Pattern

**Ubicación**: `SistemaIndustrial.AD/Repositories/`

**Propósito**: Abstraer el acceso a datos, desacoplar la lógica de negocio de la implementación de persistencia. Coexisten **dos variantes**: repositorios vía Stored Procedures con `SpInvoker` (Semana, Insumo, Stock, Usuario, RegistroTacho, ProductoCatalogo) y repositorios con **EF Core directo** (Categoria, TiradoProducto + Tiradas).

```csharp
// Interface genérica base
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(int id, T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

// Interfaces específicas por entidad (no genérica en runtime)
public interface ISemanaRepository : IRepository<Semana>
{
    Task<bool> ExistsByNumeroAnioAsync(int numeroSemana, int anio, int? excludeId = null);
    Task<bool> HasDependentDataAsync(int id);
}

// Implementación vía SPs (módulo clásico)
public class SemanaRepository : ISemanaRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;
    
    public SemanaRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }
    
    public async Task<List<Semana>> GetAllAsync()
        => await _spInvoker.QueryEntityAsync<Semana>("EXEC sp_Semanas_GetAll");
    
    public async Task<Semana?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<Semana>("EXEC sp_Semanas_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }
    
    // ... resto de métodos
}

// Implementación con EF Core directo (módulo Tirado / Categorías)
public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> GetAllAsync()
    {
        return await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    // ... resto de métodos con SaveChangesAsync / Include
}
```

**Beneficios**:
- Testabilidad: Mock fácil con Moq
- Cambio de BD: Solo tocar implementación
- SQL explícito: Control total sobre queries

---

### 2. Template Method (SpInvoker)

**Ubicación**: `SistemaIndustrial.AD/SpInvoker.cs`

**Propósito**: Definir el esqueleto de invocación a Stored Procedures, permitiendo variar la implementación concreta.

```csharp
public class SpInvoker
{
    private readonly AppDbContext _context;
    
    public SpInvoker(AppDbContext context) => _context = context;
    
    // Template method para queries que retornan entidades
    public async Task<List<T>> QueryEntityAsync<T>(string sql, object? parameters = null) where T : class
    {
        var cmd = _context.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text; // Para EXEC SP
        
        if (parameters != null)
            AddParameters(cmd, parameters);
        
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<T>();
            // Mapeo manual o usando reflection
            return results;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
    
    // Template method para EXISTS (retorna bool)
    public async Task<bool> ExistsAsync(string sql, object? parameters = null)
    {
        var result = await ScalarAsync(sql, parameters);
        return Convert.ToInt32(result) > 0;
    }
    
    // Template method para escalares (INSERT con retorno de ID, COUNT, etc.)
    public async Task<object?> ScalarAsync(string sql, object? parameters = null)
    {
        // Implementación con ExecuteScalarAsync
    }
    
    // Template method para operaciones sin retorno
    public async Task<int> NonQueryAsync(string sql, object? parameters = null)
    {
        // Implementación con ExecuteNonQueryAsync
    }
}
```

**Regla de Oro**: Nunca usar `FirstOrDefaultAsync()` sobre `FromSqlRaw`/`SqlQueryRaw` con `EXEC` — siempre `ToListAsync()` + `.FirstOrDefault()` en memoria.

---

### 3. Dependency Injection (Constructor Injection)

**Ubicación**: `SistemaIndustrial.API/Program.cs`, `SistemaIndustrial.BR/Services/`

**Propósito**: Inversión de control, ciclo de vida gestionado por contenedor.

```csharp
// Registro en Program.cs (API Layer) — DI directa, sin módulos
builder.Services.AddScoped<ISemanaRepository, SemanaRepository>();
builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITiradoProductoRepository, TiradoProductoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoCatalogoRepository, ProductoCatalogoRepository>();

builder.Services.AddScoped<ISemanaService, SemanaService>();
builder.Services.AddScoped<IInsumoService, InsumoService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITiradoProductoService, TiradoProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoCatalogoService, ProductoCatalogoService>();

builder.Services.AddScoped<SpInvoker>(); // Scoped (depende de AppDbContext)
```

```csharp
// Uso en Servicio (BR Layer)
public class TiradoProductoService : ITiradoProductoService
{
    private readonly ITiradoProductoRepository _repository;
    private readonly ISemanaRepository _semanaRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public TiradoProductoService(
        ITiradoProductoRepository repository,
        ISemanaRepository semanaRepository,
        ICategoriaRepository categoriaRepository)
    {
        _repository = repository;
        _semanaRepository = semanaRepository;
        _categoriaRepository = categoriaRepository;
    }
}
```

**Lifetimes**:
- **Scoped**: Una instancia por request HTTP (repositorios, servicios, `SpInvoker`, `AppDbContext`)
- **Transient**: Nueva instancia cada vez (poco usado aquí)

---

### 4. Result Pattern

**Ubicación**: `SistemaIndustrial.BR/DTOs/CommonDtos.cs`

**Propósito**: Manejo tipado de éxito/fallo sin excepciones para flujo de control esperado.

```csharp
// Sin datos de retorno
public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    
    public static ServiceResult Ok(string message = "Operación exitosa") => 
        new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => 
        new() { Success = false, Message = message };
}

// Con datos de retorno
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    
    public static ServiceResult<T> Ok(T data, string message = "Operación exitosa") => 
        new() { Success = true, Message = message, Data = data };
    public static ServiceResult<T> Fail(string message) => 
        new() { Success = false, Message = message };
}
```

**Uso en Servicios**:
```csharp
public async Task<ServiceResult<SemanaResponse>> CreateAsync(SemanaCreateDto dto)
{
    var validation = SemanaValidator.ValidateCreate(dto);
    if (!validation.Success)
        return ServiceResult<SemanaResponse>.Fail(validation.Message!);

    if (await _repository.ExistsByNumeroAnioAsync(dto.NumeroSemana, dto.Anio))
        return ServiceResult<SemanaResponse>.Fail($"Ya existe una semana {dto.NumeroSemana} del año {dto.Anio}.");
    
    var entity = new Semana { /* mapeo */ };
    var created = await _repository.CreateAsync(entity);
    return ServiceResult<SemanaResponse>.Ok(MapToResponse(created), "Semana creada correctamente.");
}
```

**Uso en Endpoints**:
```csharp
app.MapPost("/api/semanas", async (SemanaCreateDto dto, ISemanaService service) =>
{
    var result = await service.CreateAsync(dto);
    return result.Success
        ? Results.Created($"/api/semanas/{result.Data!.Id}", result.Data)
        : Results.BadRequest(new { error = result.Message });
});
```

---

### 5. DTO Pattern (Data Transfer Objects)

**Propósito**: Desacoplar representación externa (API) de modelo de dominio (AD).

| Capa | Tipo | Ejemplo |
|------|------|---------|
| **API (Request)** | `*CreateDto`, `*UpdateDto` | `SemanaCreateDto`, `SemanaUpdateDto` |
| **API (Response)** | `*Response` | `SemanaResponse`, `LoginResponse` |
| **BR (Input)** | `*Dto` | `LoginRequest` (compartido con API) |
| **BR (Output)** | `ServiceResult<T>`, `*Result` | `ServiceResult<Semana>`, `UserInfo` |
| **AD (Domain)** | Entidades | `Semana`, `Usuario` |

**Reglas**:
- Entidades AD **nunca** salen de la capa AD
- API usa DTOs propios (`ApiModels`)
- BR usa DTOs compartidos o propios (`ServiceResult`, `CommonDtos`)

---

### 6. Exception Shielding (Cross-Cutting)

**Ubicación**: `SistemaIndustrial.AD/Exceptions/`, `SistemaIndustrial.API/Program.cs`

**Propósito**: Desacoplar errores de dominio de códigos HTTP, no exponer stack traces.

```csharp
// Excepciones de dominio (AD Layer)
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con Id {id} no fue encontrado.") { }
}

public class PersistenceException : Exception
{
    public PersistenceException(string message, Exception inner) : base(message, inner) { }
}

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message) { }
}
```

```csharp
// Manejo en Endpoints
try
{
    var result = await _service.CreateAsync(dto);
    return result.Success ? Results.Ok(result.Data) : Results.BadRequest(new { error = result.Message });
}
catch (EntityNotFoundException ex)
{
    return Results.NotFound(new { error = ex.Message });
}
catch (PersistenceException ex)
{
    return Results.Problem(title: "Error de base de datos", detail: ex.Message, statusCode: 500);
}
catch (UnauthorizedAccessException ex)
{
    return Results.Unauthorized();
}

// Global Exception Handler (red de seguridad)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Error interno del servidor." }));
    });
});
```

---

### 7. Validators estáticos por entidad

**Ubicación**: `SistemaIndustrial.BR/Validators/`

**Propósito**: Encapsular reglas de validación por entidad en clases estáticas (`SemanaValidator`, `TiradoProductoValidator`, `ProductoCatalogoValidator`, `CategoriaValidator`, `StockValidator`, `InsumoValidator`, `RegistroTachoValidator`), invocadas al inicio de cada operación del servicio.

```csharp
// En BR Layer - Validadores estáticos
public static class SemanaValidator
{
    public static (bool valido, string? mensaje) ValidarFechasSemana(DateTime inicio, DateTime fin)
    {
        if (inicio.DayOfWeek != DayOfWeek.Monday)
            return (false, "La fecha de inicio debe ser un lunes.");

        if (fin.DayOfWeek != DayOfWeek.Saturday)
            return (false, "La fecha de fin debe ser un sábado.");

        if ((fin.Date - inicio.Date).Days != 5)
            return (false, "La semana debe durar exactamente 6 días (lunes a sábado).");

        if (fin <= inicio)
            return (false, "La fecha fin debe ser posterior a la fecha inicio.");

        return (true, null);
    }

    public static ServiceResult ValidateCreate(SemanaCreateDto dto)
    {
        if (dto.NumeroSemana < 1 || dto.NumeroSemana > 53)
            return ServiceResult.Fail("El número de semana debe estar entre 1 y 53.");
        if (dto.Anio < 2020 || dto.Anio > 2030)
            return ServiceResult.Fail("El año debe estar entre 2020 y 2030.");

        var fechas = ValidarFechasSemana(dto.FechaInicio, dto.FechaFin);
        if (!fechas.valido)
            return ServiceResult.Fail(fechas.mensaje!);

        return ServiceResult.Ok();
    }
}
```

---

## Resumen de Patrones por Capa

| Capa | Patrones Principales |
|------|---------------------|
| **AD** | Repository (doble patrón: SPs vía `SpInvoker` + EF Core directo), Template Method (`SpInvoker`), Exception Shielding |
| **BR** | Result Pattern, DTO, Validators estáticos por entidad, DI |
| **API** | Minimal API (grupos `MapGroup` por recurso), Global Exception Handler, JWT Auth, CORS `VueCors`, Scalar |