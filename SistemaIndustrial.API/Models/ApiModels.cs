using System.ComponentModel.DataAnnotations;

namespace SistemaIndustrial.API.Models;

// ============================================================
// Response Wrappers
// ============================================================
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

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

// ============================================================
// Auth
// ============================================================
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

// ============================================================
// Semana
// ============================================================
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
    [RegularExpression("^(Planificada|EnProgreso|Completada|Cancelada)$", ErrorMessage = "Estado inválido")]
    public string Estado { get; set; } = string.Empty;
}

// ============================================================
// Insumo
// ============================================================
public class InsumoCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int SemanaId { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal CantidadRequerida { get; set; }
}

public class InsumoUpdateRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal CantidadRequerida { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CantidadConsumida { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockDisponible { get; set; }
}

// ============================================================
// Stock
// ============================================================
public class StockCreateRequest
{
    [Required, Range(1, int.MaxValue)]
    public int SemanaId { get; set; }

    [Required, StringLength(200)]
    public string Producto { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal StockInicial { get; set; }
}

public class StockUpdateRequest
{
    [Required, StringLength(200)]
    public string Producto { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal StockInicial { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Entradas { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Salidas { get; set; }
}

// ============================================================
// Categoria
// ============================================================
public class CategoriaCreateRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Orden { get; set; }
}

public class CategoriaUpdateRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Orden { get; set; }

    public bool Activa { get; set; } = true;
}

// ============================================================
// TiradoProducto (módulo Tirado / Armado)
// ============================================================
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

public class TiradoProductoUpdateRequest
{
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

    [Range(0, double.MaxValue)]
    public decimal? CantidadTirada { get; set; }
}

public class RegistrarTiradaRequest
{
    [Required, Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    public DateTime? Fecha { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

// ============================================================
// ProductoCatalogo (catálogo general de productos)
// ============================================================
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

public class ProductoCatalogoUpdateRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PedidoSemanalAproximado { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PromedioPorTacho { get; set; }

    public bool Activo { get; set; } = true;

    [StringLength(500)]
    public string? Observaciones { get; set; }
}