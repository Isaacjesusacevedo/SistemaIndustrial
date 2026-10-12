using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class TiradoProductoValidator
{
    public static ServiceResult ValidateCreate(TiradoProductoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.PedidoSemanal <= 0)
            return ServiceResult.Fail("El pedido semanal debe ser mayor a cero.");

        if (dto.StockInicial < 0)
            return ServiceResult.Fail("El stock inicial no puede ser negativo.");

        if (dto.PromedioPorTacho <= 0)
            return ServiceResult.Fail("El promedio por tacho debe ser mayor a cero.");

        if (!string.IsNullOrWhiteSpace(dto.Observaciones) && dto.Observaciones.Length > 500)
            return ServiceResult.Fail("Las observaciones no pueden exceder 500 caracteres.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(TiradoProductoUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.PedidoSemanal <= 0)
            return ServiceResult.Fail("El pedido semanal debe ser mayor a cero.");

        if (dto.StockInicial < 0)
            return ServiceResult.Fail("El stock inicial no puede ser negativo.");

        if (dto.PromedioPorTacho <= 0)
            return ServiceResult.Fail("El promedio por tacho debe ser mayor a cero.");

        if (dto.CantidadTirada.HasValue && dto.CantidadTirada.Value < 0)
            return ServiceResult.Fail("La cantidad tirada no puede ser negativa.");

        if (!string.IsNullOrWhiteSpace(dto.Observaciones) && dto.Observaciones.Length > 500)
            return ServiceResult.Fail("Las observaciones no pueden exceder 500 caracteres.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateRegistrarTirada(RegistrarTiradaDto dto)
    {
        if (dto.Cantidad < 0)
            return ServiceResult.Fail("La cantidad tirada no puede ser negativa.");

        if (!string.IsNullOrWhiteSpace(dto.Observaciones) && dto.Observaciones.Length > 500)
            return ServiceResult.Fail("Las observaciones no pueden exceder 500 caracteres.");

        return ServiceResult.Ok();
    }
}