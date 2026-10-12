using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class ProductoCatalogoValidator
{
    public static ServiceResult ValidateCreate(ProductoCatalogoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 200)
            return ServiceResult.Fail("El nombre no puede exceder 200 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.PedidoSemanalAproximado < 0)
            return ServiceResult.Fail("El pedido semanal aproximado no puede ser negativo.");

        if (dto.PromedioPorTacho < 0)
            return ServiceResult.Fail("El promedio por tacho no puede ser negativo.");

        if (dto.Observaciones?.Length > 500)
            return ServiceResult.Fail("Las observaciones no pueden exceder 500 caracteres.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(ProductoCatalogoUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 200)
            return ServiceResult.Fail("El nombre no puede exceder 200 caracteres.");

        if (string.IsNullOrWhiteSpace(dto.UnidadMedida))
            return ServiceResult.Fail("La unidad de medida es obligatoria.");

        if (dto.UnidadMedida.Length > 50)
            return ServiceResult.Fail("La unidad de medida no puede exceder 50 caracteres.");

        if (dto.PedidoSemanalAproximado < 0)
            return ServiceResult.Fail("El pedido semanal aproximado no puede ser negativo.");

        if (dto.PromedioPorTacho < 0)
            return ServiceResult.Fail("El promedio por tacho no puede ser negativo.");

        if (dto.Observaciones?.Length > 500)
            return ServiceResult.Fail("Las observaciones no pueden exceder 500 caracteres.");

        return ServiceResult.Ok();
    }
}
