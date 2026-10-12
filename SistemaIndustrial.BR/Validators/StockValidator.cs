using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class StockValidator
{
    public static ServiceResult ValidateCreate(StockCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Producto))
            return ServiceResult.Fail("El producto es obligatorio.");

        if (dto.Producto.Length > 100)
            return ServiceResult.Fail("El producto no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.StockInicial < 0)
            return ServiceResult.Fail("El stock inicial no puede ser negativo.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(StockUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Producto))
            return ServiceResult.Fail("El producto es obligatorio.");

        if (dto.Producto.Length > 100)
            return ServiceResult.Fail("El producto no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.StockInicial < 0)
            return ServiceResult.Fail("El stock inicial no puede ser negativo.");

        if (dto.Entradas < 0)
            return ServiceResult.Fail("Las entradas no pueden ser negativas.");

        if (dto.Salidas < 0)
            return ServiceResult.Fail("Las salidas no pueden ser negativas.");

        return ServiceResult.Ok();
    }
}