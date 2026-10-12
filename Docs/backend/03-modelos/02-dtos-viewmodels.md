# DTOs y ViewModels (BR Layer + API Layer)

## Ubicación
- **BR Layer**: `SistemaIndustrial.BR/DTOs/`
- **API Layer**: `SistemaIndustrial.API/Models/`

---

## BR Layer DTOs (`SistemaIndustrial.BR/DTOs/`)

### CommonDtos.cs — Result Pattern
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

---

### AuthDtos.cs
```csharp
// Request
public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string Nombre, string Role);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record RefreshTokenRequest(string RefreshToken);

// Response
public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, UserInfo User);

public record UserInfo(int Id, string Email, string Nombre, string Role);

public record TokenPayload(int UserId, string Email, string Role, DateTime Exp, DateTime Iat);
```

---

### SemanaDtos.cs
```csharp
// Input
public record SemanaCreateDto(
    int NumeroSemana,
    int Anio,
    DateTime FechaInicio,
    DateTime FechaFin
);

public record SemanaUpdateDto(
    int NumeroSemana,
    int Anio,
    DateTime FechaInicio,
    DateTime FechaFin,
    string Estado
);

// Output
public record SemanaResponse(
    int Id,
    int NumeroSemana,
    int Anio,
    DateTime FechaInicio,
    DateTime FechaFin,
    string Estado,
    DateTime FechaCreacion,
    int? InsumosCount
);
```

---

### CategoriaDtos.cs
```csharp
public record CategoriaCreateDto(string Nombre, int Orden);

public record CategoriaUpdateDto(string Nombre, int Orden, bool Activa);

public record CategoriaResponse(int Id, string Nombre, int Orden, bool Activa, int ProductosCount);
```

---

### TiradoProductoDtos.cs
```csharp
public record TiradoProductoCreateDto(
    int SemanaId,
    string Nombre,
    string UnidadMedida,
    decimal PedidoSemanal,
    decimal StockInicial,
    decimal PromedioPorTacho,
    int? CategoriaId,
    string? Observaciones);

public record TiradoProductoUpdateDto(
    string Nombre,
    string UnidadMedida,
    decimal PedidoSemanal,
    decimal StockInicial,
    decimal PromedioPorTacho,
    int? CategoriaId,
    string? Observaciones,
    decimal? CantidadTirada);

public record RegistrarTiradaDto(
    decimal Cantidad,
    DateTime? Fecha,
    string? Observaciones);

public record TiradoProductoResponse(
    int Id,
    int SemanaId,
    int? CategoriaId,
    string? CategoriaNombre,
    string Nombre,
    string UnidadMedida,
    decimal PedidoSemanal,
    decimal StockInicial,
    decimal PromedioPorTacho,
    decimal CantidadTirada,
    int CantidadTachos,
    decimal StockTotal,
    decimal TachosEstimados,
    string Estado,
    string? Observaciones,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);

// Bulk create: POST /api/tirado/semana/{semanaId}/bulk
public record TiradoProductoBulkItemDto(
    string Nombre,
    string UnidadMedida,
    decimal PromedioPorTacho,
    decimal StockInicial,
    decimal PedidoSemanal,
    int? CategoriaId,
    string? Observaciones);

public record TiradoProductoBulkCreateDto(
    List<TiradoProductoBulkItemDto> Items);

public record TiradoProductoBulkResultDto(
    int Creados,
    int Ignorados,
    List<string> Errores,
    List<TiradoProductoResponse> ProductosCreados);
```

---

### TiradaDtos.cs
```csharp
public record TiradaResponse(int Id, int TiradoProductoId, string ProductoNombre, DateTime Fecha, decimal Cantidad, string? Observaciones, DateTime FechaCreacion);
```

---

### ProductoCatalogoDtos.cs
```csharp
public record ProductoCatalogoCreateDto(string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, string? Observaciones);
public record ProductoCatalogoUpdateDto(string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, bool Activo, string? Observaciones);
public record ProductoCatalogoResponse(int Id, string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, bool Activo, string? Observaciones, DateTime FechaCreacion);
```

---

### InsumoDtos.cs
```csharp
public record InsumoCreateDto(int SemanaId, string Nombre, string UnidadMedida, decimal CantidadRequerida);

public record InsumoUpdateDto(string Nombre, string UnidadMedida, decimal CantidadRequerida, decimal CantidadConsumida, decimal StockDisponible);

public record InsumoResponse(
    int Id,
    int SemanaId,
    string Nombre,
    string UnidadMedida,
    decimal CantidadRequerida,
    decimal CantidadConsumida,
    decimal StockDisponible,
    DateTime FechaCreacion
);
```

---

### StockDtos.cs
```csharp
public record StockCreateDto(int SemanaId, string Producto, string UnidadMedida, decimal StockInicial);

public record StockUpdateDto(string Producto, string UnidadMedida, decimal StockInicial, decimal Entradas, decimal Salidas);

public record StockResponse(
    int Id,
    int SemanaId,
    string Producto,
    string UnidadMedida,
    decimal StockInicial,
    decimal Entradas,
    decimal Salidas,
    decimal StockFinal,
    DateTime FechaCreacion
);
```

---

### RegistroTachoDtos.cs

> **Nota:** código existente; el servicio no está registrado en DI y el endpoint no está mapeado.

```csharp
public record RegistroTachoCreateDto(int SemanaId, int Turno, string Operador, decimal CantidadProducida, decimal CantidadRechazada, string EstadoCalidad, DateTime FechaRegistro);

public record RegistroTachoUpdateDto(int Turno, string Operador, decimal CantidadProducida, decimal CantidadRechazada, string EstadoCalidad, DateTime FechaRegistro);

public record RegistroTachoResponse(
    int Id,
    int SemanaId,
    int Turno,
    string Operador,
    decimal CantidadProducida,
    decimal CantidadRechazada,
    string EstadoCalidad,
    DateTime FechaRegistro,
    DateTime FechaCreacion
);
```

---

### DashboardDtos.cs
```csharp
public record DashboardMetricasDto(
    int SemanaId,
    int NumeroSemana,
    int Anio,
    decimal TotalProducido,   // Suma de Tiradas de la semana
    decimal TotalRechazado,   // Siempre 0 (reservado)
    decimal PorcentajeCalidad,
    decimal InsumosConsumidos,
    decimal InsumosRequeridos,
    decimal StockActual,
    int TurnosRegistrados,    // Días distintos con tiradas
    int OperadoresUnicos      // Siempre 0 (reservado)
);

public record DashboardTendenciaDto(
    string Periodo, // "2026-W01", "2026-W02"
    decimal Produccion,
    decimal Rechazos,
    decimal Calidad
);

public record DashboardAlertaDto(
    string Tipo, // "StockBajo", "SinRegistros", ...
    string Mensaje,
    string Severidad, // "Info", "Warning", "Critical"
    int? SemanaId,
    string? Detalle
);
```

---

## API Layer Models (`SistemaIndustrial.API/Models/ApiModels.cs`)

### Request DTOs (para binding automático)
```csharp
// Auth
public class LoginRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required, StringLength(50)]
    public string Role { get; set; } = "Operador";
}

public class ChangePasswordRequest
{
    [Required, StringLength(100, MinimumLength = 8)]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

// Semanas
public class SemanaCreateRequest
{
    [Required, Range(1, 53)]
    public int NumeroSemana { get; set; }
    
    [Required, Range(2020, 2030)]
    public int Anio { get; set; }
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
}

public class SemanaUpdateRequest
{
    [Required, Range(1, 53)]
    public int NumeroSemana { get; set; }
    
    [Required, Range(2020, 2030)]
    public int Anio { get; set; }
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
    
    [Required, StringLength(20)]
    public string Estado { get; set; } = string.Empty;
}

// Insumos, Stock — similares con DataAnnotations (ver ApiModels.cs)

// Categoría
public class CategoriaCreateRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

// TiradoProducto (módulo Tirado / Armado)
public class TiradoProductoCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int SemanaId { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = "un";

    [Required, Range(0.01, double.MaxValue)]
    public decimal PedidoSemanal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockInicial { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal PromedioPorTacho { get; set; }

    public int? CategoriaId { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

public class RegistrarTiradaRequest
{
    [Required, Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    public DateTime? Fecha { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

// ProductoCatalogo (catálogo general)
public class ProductoCatalogoCreateRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PedidoSemanalAproximado { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PromedioPorTacho { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
// (+ *UpdateRequest equivalentes con DataAnnotations; ver ApiModels.cs)
```

### Response Wrappers
```csharp
// Wrapper estándar para todas las respuestas API
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public static ApiResponse<T> Ok(T data, string message = "OK") => 
        new() { Success = true, Message = message, Data = data };
    
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => 
        new() { Success = false, Message = message, Errors = errors };
    
    public static ApiResponse<T> NotFound(string message = "Recurso no encontrado") => 
        new() { Success = false, Message = message };
    
    public static ApiResponse<T> Unauthorized(string message = "No autorizado") => 
        new() { Success = false, Message = message };
}

// Para listas paginadas (futuro)
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

### Mapeo BR DTOs ↔ API Models
```csharp
// En Endpoints - conversión manual o con Mapster/AutoMapper
var brResult = await _service.CreateAsync(new SemanaCreateDto(...));
if (brResult.Success)
{
    var apiResponse = new SemanaResponse(
        brResult.Data.Id, brResult.Data.NumeroSemana, ...);
    return Results.Created($"/api/semanas/{apiResponse.Id}", ApiResponse<SemanaResponse>.Ok(apiResponse));
}
return Results.BadRequest(ApiResponse<object>.Fail(brResult.Message));
```