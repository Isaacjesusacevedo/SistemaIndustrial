namespace SistemaIndustrial.BR.DTOs;

public record StockCreateDto(int SemanaId, string Producto, string UnidadMedida, decimal StockInicial);
public record StockUpdateDto(string Producto, string UnidadMedida, decimal StockInicial, decimal Entradas, decimal Salidas);
public record StockResponse(int Id, int SemanaId, string Producto, string UnidadMedida, decimal StockInicial, decimal Entradas, decimal Salidas, decimal StockFinal, DateTime FechaCreacion);