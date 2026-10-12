using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class SemanaValidator
{
    public static (bool valido, string? mensaje) ValidarFechasSemana(DateTime inicio, DateTime fin)
    {
        if (inicio.DayOfWeek != DayOfWeek.Monday)
            return (false, "La fecha de inicio debe ser un lunes.");

        if (fin.DayOfWeek != DayOfWeek.Saturday)
            return (false, "La fecha de fin debe ser un sábado.");

        if ((fin.Date - inicio.Date).Days != 5)
            return (false, "La semana debe durar exactamente 6 días (lunes a sábado).");

        if (fin <= inicio)
            return (false, "La fecha fin debe ser posterior a la fecha inicio.");

        return (true, null);
    }

    public static ServiceResult ValidateCreate(SemanaCreateDto dto)
    {
        if (dto.NumeroSemana < 1 || dto.NumeroSemana > 53)
            return ServiceResult.Fail("El número de semana debe estar entre 1 y 53.");

        if (dto.Anio < 2020 || dto.Anio > 2030)
            return ServiceResult.Fail("El año debe estar entre 2020 y 2030.");

        var fechas = ValidarFechasSemana(dto.FechaInicio, dto.FechaFin);
        if (!fechas.valido)
            return ServiceResult.Fail(fechas.mensaje!);

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(SemanaUpdateDto dto)
    {
        if (dto.NumeroSemana < 1 || dto.NumeroSemana > 53)
            return ServiceResult.Fail("El número de semana debe estar entre 1 y 53.");

        if (dto.Anio < 2020 || dto.Anio > 2030)
            return ServiceResult.Fail("El año debe estar entre 2020 y 2030.");

        var fechas = ValidarFechasSemana(dto.FechaInicio, dto.FechaFin);
        if (!fechas.valido)
            return ServiceResult.Fail(fechas.mensaje!);

        var estadosValidos = new[] { "Planificada", "EnProgreso", "Completada", "Cancelada" };
        if (!estadosValidos.Contains(dto.Estado))
            return ServiceResult.Fail($"El estado debe ser uno de: {string.Join(", ", estadosValidos)}.");

        return ServiceResult.Ok();
    }
}