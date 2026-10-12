using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class InsumoService : IInsumoService
{
    private readonly IInsumoRepository _repository;
    private readonly ISemanaRepository _semanaRepository;

    public InsumoService(IInsumoRepository repository, ISemanaRepository semanaRepository)
    {
        _repository = repository;
        _semanaRepository = semanaRepository;
    }

    public async Task<ServiceResult<InsumoResponse>> CreateAsync(InsumoCreateDto dto)
    {
        var validation = InsumoValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<InsumoResponse>.Fail(validation.Message!);

        if (!await _semanaRepository.ExistsAsync(dto.SemanaId))
            return ServiceResult<InsumoResponse>.Fail($"La semana con Id {dto.SemanaId} no existe.");

        var entity = new InsumoSemana
        {
            SemanaId = dto.SemanaId,
            Nombre = dto.Nombre,
            UnidadMedida = dto.UnidadMedida,
            CantidadRequerida = dto.CantidadRequerida,
            CantidadConsumida = 0,
            StockDisponible = 0,
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<InsumoResponse>.Ok(response, "Insumo creado correctamente.");
    }

    public async Task<ServiceResult<InsumoResponse>> UpdateAsync(int id, InsumoUpdateDto dto)
    {
        var validation = InsumoValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<InsumoResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<InsumoResponse>.Fail($"El insumo con Id {id} no existe.");

        existing.Nombre = dto.Nombre;
        existing.UnidadMedida = dto.UnidadMedida;
        existing.CantidadRequerida = dto.CantidadRequerida;
        existing.CantidadConsumida = dto.CantidadConsumida;
        existing.StockDisponible = dto.StockDisponible;

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<InsumoResponse>.Ok(response, "Insumo actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"El insumo con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Insumo eliminado correctamente.");
    }

    public async Task<ServiceResult<InsumoResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<InsumoResponse>.Fail($"El insumo con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<InsumoResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<InsumoResponse>>> GetBySemanaIdAsync(int semanaId)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<InsumoResponse>>.Fail($"La semana con Id {semanaId} no existe.");

        var entities = await _repository.GetBySemanaIdAsync(semanaId);
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<InsumoResponse>>.Ok(responses);
    }

    private static InsumoResponse MapToResponse(InsumoSemana entity)
    {
        return new InsumoResponse(
            entity.Id,
            entity.SemanaId,
            entity.Nombre,
            entity.UnidadMedida,
            entity.CantidadRequerida,
            entity.CantidadConsumida,
            entity.StockDisponible,
            entity.FechaCreacion
        );
    }
}