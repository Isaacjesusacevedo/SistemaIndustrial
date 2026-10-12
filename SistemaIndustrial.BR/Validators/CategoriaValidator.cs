using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Validators;

public static class CategoriaValidator
{
    public static ServiceResult ValidateCreate(CategoriaCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (dto.Orden < 0)
            return ServiceResult.Fail("El orden debe ser mayor o igual a 0.");

        return ServiceResult.Ok();
    }

    public static ServiceResult ValidateUpdate(CategoriaUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return ServiceResult.Fail("El nombre es obligatorio.");

        if (dto.Nombre.Length > 100)
            return ServiceResult.Fail("El nombre no puede exceder 100 caracteres.");

        if (dto.Orden < 0)
            return ServiceResult.Fail("El orden debe ser mayor o igual a 0.");

        return ServiceResult.Ok();
    }
}