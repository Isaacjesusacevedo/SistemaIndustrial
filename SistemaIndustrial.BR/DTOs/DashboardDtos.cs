namespace SistemaIndustrial.BR.DTOs;

public record DashboardMetricasDto(int SemanaId, int NumeroSemana, int Anio, decimal TotalProducido, decimal TotalRechazado, decimal PorcentajeCalidad, decimal InsumosConsumidos, decimal InsumosRequeridos, decimal StockActual, int TurnosRegistrados, int OperadoresUnicos);
public record DashboardTendenciaDto(string Periodo, decimal Produccion, decimal Rechazos, decimal Calidad);
public record DashboardAlertaDto(string Tipo, string Mensaje, string Severidad, int? SemanaId, string? Detalle);