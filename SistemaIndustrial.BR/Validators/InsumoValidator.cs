using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class InsumoValidator
{
    public static ServiceResult ValidateCreate(InsumoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.CantidadRequerida < 0)
            return ServiceResult.Fail("La cantidad requerida no puede ser negativa.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(InsumoUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.CantidadRequerida < 0)
            return ServiceResult.Fail("La cantidad requerida no puede ser negativa.");

        if (dto.CantidadConsumida < 0)
            return ServiceResult.Fail("La cantidad consumida no puede ser negativa.");

        if (dto.StockDisponible < 0)
            return ServiceResult.Fail("El stock disponible no puede ser negativo.");

        return ServiceResult.Ok();
    }
}