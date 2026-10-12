using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repository;

    public CategoriaService(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<CategoriaResponse>> CreateAsync(CategoriaCreateDto dto)
    {
        var validation = CategoriaValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<CategoriaResponse>.Fail(validation.Message!);

        var entity = new Categoria
        {
            Nombre = dto.Nombre,
            Orden = dto.Orden,
            Activa = true
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<CategoriaResponse>.Ok(response, "Categoría creada correctamente.");
    }

    public async Task<ServiceResult<CategoriaResponse>> UpdateAsync(int id, CategoriaUpdateDto dto)
    {
        var validation = CategoriaValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<CategoriaResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<CategoriaResponse>.Fail($"La categoría con Id {id} no existe.");

        existing.Nombre = dto.Nombre;
        existing.Orden = dto.Orden;
        existing.Activa = dto.Activa;

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<CategoriaResponse>.Ok(response, "Categoría actualizada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"La categoría con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Categoría eliminada correctamente.");
    }

    public async Task<ServiceResult<CategoriaResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<CategoriaResponse>.Fail($"La categoría con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<CategoriaResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<CategoriaResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<CategoriaResponse>>.Ok(responses);
    }

    private static CategoriaResponse MapToResponse(Categoria entity)
    {
        return new CategoriaResponse(
            entity.Id,
            entity.Nombre,
            entity.Orden,
            entity.Activa,
            entity.Productos?.Count ?? 0
        );
    }
}