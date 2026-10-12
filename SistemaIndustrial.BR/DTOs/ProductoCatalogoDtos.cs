namespace SistemaIndustrial.BR.DTOs;

public record ProductoCatalogoCreateDto(string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, string? Observaciones);
public record ProductoCatalogoUpdateDto(string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, bool Activo, string? Observaciones);
public record ProductoCatalogoResponse(int Id, string Nombre, string UnidadMedida, decimal PedidoSemanalAproximado, decimal PromedioPorTacho, bool Activo, string? Observaciones, DateTime FechaCreacion);
