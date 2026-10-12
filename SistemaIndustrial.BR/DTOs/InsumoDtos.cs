namespace SistemaIndustrial.BR.DTOs;

public record InsumoCreateDto(int SemanaId, string Nombre, string UnidadMedida, decimal CantidadRequerida);
public record InsumoUpdateDto(string Nombre, string UnidadMedida, decimal CantidadRequerida, decimal CantidadConsumida, decimal StockDisponible);
public record InsumoResponse(int Id, int SemanaId, string Nombre, string UnidadMedida, decimal CantidadRequerida, decimal CantidadConsumida, decimal StockDisponible, DateTime FechaCreacion);