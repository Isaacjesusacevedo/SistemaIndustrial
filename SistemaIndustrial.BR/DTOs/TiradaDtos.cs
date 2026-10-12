namespace SistemaIndustrial.BR.DTOs;

public record TiradaResponse(int Id, int TiradoProductoId, string ProductoNombre, DateTime Fecha, decimal Cantidad, string? Observaciones, DateTime FechaCreacion);