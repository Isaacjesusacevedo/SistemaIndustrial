# Capa de Servicios (Business Rules Layer)

## Estructura

```
SistemaIndustrial.BR/
├── DTOs/                    # ServiceResult, DTOs específicos (records)
│   ├── CommonDtos.cs        # ServiceResult, ServiceResult<T>
│   ├── AuthDtos.cs, SemanaDtos.cs, InsumoDtos.cs, StockDtos.cs
│   ├── CategoriaDtos.cs, TiradoProductoDtos.cs, TiradaDtos.cs
│   ├── ProductoCatalogoDtos.cs, RegistroTachoDtos.cs, DashboardDtos.cs
├── Interfaces/              # ISemanaService, ..., IAuthService, IProductoCatalogoService
│   ├── IServices.cs         # Semana, Insumo, Stock (+SyncFromTirado), Dashboard, Tirado (+bulk), Categoria
│   ├── IAuthService.cs
│   └── IProductoCatalogoService.cs
├── Services/                # Implementaciones
│   ├── SemanaService.cs
│   ├── InsumoService.cs
│   ├── StockService.cs      # + SyncFromTiradoAsync
│   ├── RegistroTachoService.cs # Código existente (sin registro DI)
│   ├── DashboardService.cs  # Métricas desde Tirado + Insumo + Stock
│   ├── AuthService.cs
│   ├── CategoriaService.cs
│   ├── TiradoProductoService.cs # + bulk, RegistrarTirada
│   └── ProductoCatalogoService.cs
├── Validators/              # Clases estáticas: Semana, Insumo, Stock, RegistroTacho, Categoria, TiradoProducto, ProductoCatalogo
└── SistemaIndustrial.BR.csproj
```

---

## Registro en DI Container (Program.cs)

```csharp
// Repositories (AD Layer) — DI directa en Program.cs
builder.Services.AddScoped<ISemanaRepository, SemanaRepository>();           // SPs
builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();           // SPs
builder.Services.AddScoped<IStockRepository, StockRepository>();             // SPs
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();         // SPs
builder.Services.AddScoped<ITiradoProductoRepository, TiradoProductoRepository>(); // EF directo
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();     // EF directo
builder.Services.AddScoped<IProductoCatalogoRepository, ProductoCatalogoRepository>(); // SPs

// Services (BR Layer)
builder.Services.AddScoped<ISemanaService, SemanaService>();
builder.Services.AddScoped<IInsumoService, InsumoService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITiradoProductoService, TiradoProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoCatalogoService, ProductoCatalogoService>();

// Helpers
builder.Services.AddScoped<SpInvoker>(); // Scoped (usa AppDbContext)
```

> **Nota:** `IRegistroTachoRepository` / `RegistroTachoService` existen en código pero no están registrados en DI y su endpoint no está mapeado; el flujo vigente de producción es TiradoProducto + Tirada. No existe módulo de Objetivos (`ObjetivoSemana` fue eliminado del modelo).

---

## SpInvoker (Data Access Layer - SistemaIndustrial.AD)

**Archivo**: `SistemaIndustrial.AD/SpInvoker.cs`

Helper centralizado para invocar Stored Procedures desde EF Core:

```csharp
public class SpInvoker
{
    private readonly AppDbContext _context;
    
    public SpInvoker(AppDbContext context) => _context = context;
    
    // Query que retorna lista de entidades
    public async Task<List<T>> QueryEntityAsync<T>(string sql, object? parameters = null) where T : class
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T>();
            
            // Mapeo simple por convención (propiedades = columnas)
            while (await reader.ReadAsync())
            {
                var entity = Activator.CreateInstance<T>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var propName = reader.GetName(i);
                    var prop = typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null && !reader.IsDBNull(i))
                    {
                        prop.SetValue(entity, reader.GetValue(i));
                    }
                }
                results.Add(entity);
            }
            return results;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    // Exists (retorna bool)
    public async Task<bool> ExistsAsync(string sql, object? parameters = null)
    {
        var result = await ScalarAsync(sql, parameters);
        return Convert.ToInt32(result) > 0;
    }
    
    // Scalar (INSERT con retorno ID, COUNT, etc.)
    public async Task<object?> ScalarAsync(string sql, object? parameters = null)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            return await command.ExecuteScalarAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    // NonQuery (UPDATE, DELETE)
    public async Task<int> NonQueryAsync(string sql, object? parameters = null)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            return await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
```

**Regla de Oro**: Nunca usar `FirstOrDefaultAsync()` sobre `FromSqlRaw`/`SqlQueryRaw` con `EXEC` — siempre `ToListAsync()` + `.FirstOrDefault()` en memoria.

**Coexistencia con EF Core directo**: los repositorios nuevos (`CategoriaRepository`, `TiradoProductoRepository`) no usan `SpInvoker`; trabajan con `DbSet` + `Include` + `SaveChangesAsync`. `ProductoCatalogoRepository`, en cambio, sí usa `SpInvoker` (`sp_ProductosCatalogo_*`).

---

## Interfaces de Servicios

```csharp
// IServices.cs
public interface ISemanaService
{
    Task<ServiceResult<SemanaResponse>> CreateAsync(SemanaCreateDto dto);
    Task<ServiceResult<SemanaResponse>> UpdateAsync(int id, SemanaUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<SemanaResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<SemanaResponse>>> GetAllAsync();
    Task<ServiceResult<List<SemanaResponse>>> GetByAnioAsync(int anio);
}

public interface IInsumoService
{
    Task<ServiceResult<InsumoResponse>> CreateAsync(InsumoCreateDto dto);
    Task<ServiceResult<InsumoResponse>> UpdateAsync(int id, InsumoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<InsumoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<InsumoResponse>>> GetBySemanaIdAsync(int semanaId);
}

public interface IStockService
{
    Task<ServiceResult<StockResponse>> CreateAsync(StockCreateDto dto);
    Task<ServiceResult<StockResponse>> UpdateAsync(int id, StockUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<StockResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<StockResponse>>> GetBySemanaIdAsync(int semanaId);
    Task<ServiceResult<List<StockResponse>>> SyncFromTiradoAsync(int semanaId); // POST /api/stock/sync-from-tirado/{semanaId}
}

public interface ITiradoProductoService
{
    Task<ServiceResult<List<TiradoProductoResponse>>> GetAllAsync();
    Task<ServiceResult<TiradoProductoResponse>> CreateAsync(TiradoProductoCreateDto dto);
    Task<ServiceResult<TiradoProductoResponse>> UpdateAsync(int id, TiradoProductoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<TiradoProductoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaIdAsync(int semanaId);
    Task<ServiceResult<TiradoProductoResponse>> RegistrarTiradaAsync(int id, RegistrarTiradaDto dto);
    Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaConFiltroAsync(int semanaId, string? filtro = null);
    Task<ServiceResult<List<TiradaResponse>>> GetTiradasBySemanaAsync(int semanaId);
    Task<ServiceResult<TiradoProductoBulkResultDto>> CreateBulkAsync(int semanaId, TiradoProductoBulkCreateDto dto);
}

public interface ICategoriaService
{
    Task<ServiceResult<CategoriaResponse>> CreateAsync(CategoriaCreateDto dto);
    Task<ServiceResult<CategoriaResponse>> UpdateAsync(int id, CategoriaUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<CategoriaResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<CategoriaResponse>>> GetAllAsync();
}

// IProductoCatalogoService.cs
public interface IProductoCatalogoService
{
    Task<ServiceResult<ProductoCatalogoResponse>> CreateAsync(ProductoCatalogoCreateDto dto);
    Task<ServiceResult<ProductoCatalogoResponse>> UpdateAsync(int id, ProductoCatalogoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<ProductoCatalogoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<ProductoCatalogoResponse>>> GetAllAsync();
}

public interface IDashboardService
{
    Task<ServiceResult<DashboardMetricasDto>> GetMetricasAsync(int semanaId);
    Task<ServiceResult<List<DashboardTendenciaDto>>> GetTendenciasAsync(int semanasAtras = 12);
    Task<ServiceResult<List<DashboardAlertaDto>>> GetAlertasAsync(int semanaId);
}

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ServiceResult> LogoutAsync(int userId);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ServiceResult<UserInfo?>> GetUserByIdAsync(int id);
    Task<ServiceResult<bool>> ValidateTokenAsync(string token);
}
```

---

## Servicios CRUD - Detalle

### SemanaService
```csharp
public class SemanaService : ISemanaService
{
    private readonly ISemanaRepository _repository;

    public SemanaService(ISemanaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<SemanaResponse>> CreateAsync(SemanaCreateDto dto)
    {
        // Validaciones (ver 03-modelos/03-validaciones.md, incluye lunes-sábado)
        var validation = SemanaValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<SemanaResponse>.Fail(validation.Message!);

        if (await _repository.ExistsByNumeroAnioAsync(dto.NumeroSemana, dto.Anio))
            return ServiceResult<SemanaResponse>.Fail($"Ya existe una semana {dto.NumeroSemana} del año {dto.Anio}.");

        var entity = new Semana
        {
            NumeroSemana = dto.NumeroSemana,
            Anio = dto.Anio,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = "Planificada",
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        return ServiceResult<SemanaResponse>.Ok(MapToResponse(created), "Semana creada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"La semana con Id {id} no existe.");

        // SP sp_Semanas_HasDependentData: insumos, stock, tirado
        if (await _repository.HasDependentDataAsync(id))
            return ServiceResult.Fail("No se puede eliminar la semana porque tiene datos asociados (insumos, stock, tirado).");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Semana eliminada correctamente.");
    }

    // GetByIdAsync / GetAllAsync / GetByAnioAsync (filtro en memoria por Anio)
    // mapean con MapToResponse (incluye Insumos?.Count como InsumosCount)
}
```

### TiradoProductoService
```csharp
public class TiradoProductoService : ITiradoProductoService
{
    private readonly ITiradoProductoRepository _repository;
    private readonly ISemanaRepository _semanaRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    // CreateAsync: valida (TiradoProductoValidator), semana existente,
    //   categoría opcional existente, unicidad Nombre+Semana.
    //   Crea con CantidadTirada = 0, Stock = 0.
    // UpdateAsync: valida, unicidad con excludeId, actualiza FechaActualizacion.
    // RegistrarTiradaAsync(id, dto): valida cantidad, crea Tirada
    //   (Fecha default Now), recalcula CantidadTirada/Stock del producto.
    // GetBySemanaConFiltroAsync: filtra por nombre (case-insensitive) en memoria.
    // GetTiradasBySemanaAsync: agrega tiradas de todos los productos, orden desc por Fecha.
    // CreateBulkAsync(semanaId, dto): crea en lote; items inválidos/duplicados
    //   se ignoran y se reportan en TiradoProductoBulkResultDto
    //   (Creados, Ignorados, Errores, ProductosCreados).
    // DeleteAsync: elimina (las Tiradas caen en cascada por FK).
}
```

### StockService — SyncFromTirado
```csharp
public class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly ISemanaRepository _semanaRepository;
    private readonly ITiradoProductoService _tiradoService;

    public async Task<ServiceResult<List<StockResponse>>> SyncFromTiradoAsync(int semanaId)
    {
        // 1. Semana debe existir
        // 2. Lee Tirado de la semana; crea StockSemana por cada producto
        //    cuyo nombre (case-insensitive) aún no exista en stock
        //    (StockInicial del Tirado, Entradas/Salidas = 0)
        // 3. Retorna stock final + "N productos sincronizados desde Tirado."
    }
}
```

### DashboardService
```csharp
public class DashboardService : IDashboardService
{
    private readonly ISemanaRepository _semanaRepository;
    private readonly IInsumoRepository _insumoRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ITiradoProductoRepository _tiradoProductoRepository;

    public async Task<ServiceResult<DashboardMetricasDto>> GetMetricasAsync(int semanaId)
    {
        var semana = await _semanaRepository.GetByIdAsync(semanaId);
        if (semana == null)
            return ServiceResult<DashboardMetricasDto>.Fail($"La semana con Id {semanaId} no existe.");

        // Producción = suma de Tiradas de los productos de la semana
        var productos = await _tiradoProductoRepository.GetBySemanaIdAsync(semanaId);
        var tiradas = new List<Tirada>();
        foreach (var p in productos)
            tiradas.AddRange(await _tiradoProductoRepository.GetTiradasByProductoAsync(p.Id));

        var totalProducido = tiradas.Sum(t => t.Cantidad);
        // TotalRechazado = 0 (reservado), OperadoresUnicos = 0 (reservado)
        // TurnosRegistrados = días distintos con tiradas

        // Insumos: Consumidos vs Requeridos; Stock: suma de StockFinal
        // ...
    }

    public async Task<ServiceResult<List<DashboardTendenciaDto>>> GetTendenciasAsync(int semanasAtras = 12)
    {
        // Últimas N semanas (ordenadas por Anio + NumeroSemana) con
        // Periodo "AAAA-WNN", Produccion, Rechazos, Calidad
    }

    public async Task<ServiceResult<List<DashboardAlertaDto>>> GetAlertasAsync(int semanaId)
    {
        // Alertas desde métricas: StockBajo (Critical), SinRegistros (Info), etc.
    }
}
```

---

## AuthService

```csharp
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    
    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }
    
    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return ServiceResult<LoginResponse>.Fail("Email y contraseña son requeridos.");
        
        var user = await _usuarioRepository.GetByEmailAsync(request.Email.Trim().ToLower());
        if (user == null || !user.Activo)
            return ServiceResult<LoginResponse>.Fail("Credenciales inválidas.");
        
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<LoginResponse>.Fail("Credenciales inválidas.");
        
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        
        await _usuarioRepository.UpdateRefreshTokenAsync(user.Id, BCrypt.Net.BCrypt.HashPassword(refreshToken), DateTime.UtcNow.AddDays(7));
        
        return ServiceResult<LoginResponse>.Ok(new LoginResponse(
            token, refreshToken, DateTime.UtcNow.AddHours(8),
            new UserInfo(user.Id, user.Email, user.Nombre, user.Role)));
    }
    
    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // 1. El refresh token es un JWT: se lee el claim NameIdentifier (userId)
        // 2. Se busca el usuario y se verifica BCrypt(request.RefreshToken, RefreshTokenHash)
        // 3. Se genera nuevo access token (8h) + nuevo refresh token JWT (7d) con rotación
        //    (se actualiza el hash en BD)
    }
    
    public async Task<ServiceResult> LogoutAsync(int userId)
    {
        await _usuarioRepository.UpdateRefreshTokenAsync(userId, null, null);
        return ServiceResult.Ok("Sesión cerrada.");
    }
    
    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _usuarioRepository.GetByIdAsync(userId);
        if (user == null) return ServiceResult.Fail("Usuario no encontrado.");
        
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Contraseña actual incorrecta.");
        
        if (request.NewPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        await _usuarioRepository.UpdateAsync(userId, user);
        
        // Invalidar refresh tokens
        await _usuarioRepository.UpdateRefreshTokenAsync(userId, null, null);
        
        return ServiceResult.Ok("Contraseña cambiada correctamente.");
    }
    
    private string GenerateJwtToken(Usuario user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, user.Role),
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```