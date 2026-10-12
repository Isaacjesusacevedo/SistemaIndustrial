using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class ProductoCatalogoService : IProductoCatalogoService
{
    private readonly IProductoCatalogoRepository _repository;

    public ProductoCatalogoService(IProductoCatalogoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<ProductoCatalogoResponse>> CreateAsync(ProductoCatalogoCreateDto dto)
    {
        var validation = ProductoCatalogoValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<ProductoCatalogoResponse>.Fail(validation.Message!);

        if (await _repository.ExistsByNombreAsync(dto.Nombre))
            return ServiceResult<ProductoCatalogoResponse>.Fail($"Ya existe un producto con el nombre '{dto.Nombre}'.");

        var entity = new ProductoCatalogo
        {
            Nombre = dto.Nombre,
            UnidadMedida = dto.UnidadMedida,
            PedidoSemanalAproximado = dto.PedidoSemanalAproximado,
            PromedioPorTacho = dto.PromedioPorTacho,
            Activo = true,
            Observaciones = dto.Observaciones,
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<ProductoCatalogoResponse>.Ok(response, "Producto creado correctamente.");
    }

    public async Task<ServiceResult<ProductoCatalogoResponse>> UpdateAsync(int id, ProductoCatalogoUpdateDto dto)
    {
        var validation = ProductoCatalogoValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<ProductoCatalogoResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<ProductoCatalogoResponse>.Fail($"El producto con Id {id} no existe.");

        if (await _repository.ExistsByNombreAsync(dto.Nombre, id))
            return ServiceResult<ProductoCatalogoResponse>.Fail($"Ya existe otro producto con el nombre '{dto.Nombre}'.");

        existing.Nombre = dto.Nombre;
        existing.UnidadMedida = dto.UnidadMedida;
        existing.PedidoSemanalAproximado = dto.PedidoSemanalAproximado;
        existing.PromedioPorTacho = dto.PromedioPorTacho;
        existing.Activo = dto.Activo;
        existing.Observaciones = dto.Observaciones;
        existing.FechaActualizacion = DateTime.Now;

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<ProductoCatalogoResponse>.Ok(response, "Producto actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"El producto con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Producto eliminado correctamente.");
    }

    public async Task<ServiceResult<ProductoCatalogoResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<ProductoCatalogoResponse>.Fail($"El producto con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<ProductoCatalogoResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<ProductoCatalogoResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<ProductoCatalogoResponse>>.Ok(responses);
    }

    private static ProductoCatalogoResponse MapToResponse(ProductoCatalogo entity)
    {
        return new ProductoCatalogoResponse(
            entity.Id,
            entity.Nombre,
            entity.UnidadMedida,
            entity.PedidoSemanalAproximado,
            entity.PromedioPorTacho,
            entity.Activo,
            entity.Observaciones,
            entity.FechaCreacion
        );
    }
}
