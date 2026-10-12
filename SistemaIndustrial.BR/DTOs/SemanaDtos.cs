namespace SistemaIndustrial.BR.DTOs;

public record SemanaCreateDto(int NumeroSemana, int Anio, DateTime FechaInicio, DateTime FechaFin);
public record SemanaUpdateDto(int NumeroSemana, int Anio, DateTime FechaInicio, DateTime FechaFin, string Estado);
public record SemanaResponse(int Id, int NumeroSemana, int Anio, DateTime FechaInicio, DateTime FechaFin, string Estado, DateTime FechaCreacion, int? InsumosCount);