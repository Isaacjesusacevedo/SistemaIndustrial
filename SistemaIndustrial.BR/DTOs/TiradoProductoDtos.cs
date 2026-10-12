namespace SistemaIndustrial.BR.DTOs;

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

public record TiradoProductoResponse(    int Id,
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