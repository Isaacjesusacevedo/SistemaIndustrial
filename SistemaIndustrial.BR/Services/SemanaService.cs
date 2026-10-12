using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class SemanaService : ISemanaService
{
    private readonly ISemanaRepository _repository;

    public SemanaService(ISemanaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<SemanaResponse>> CreateAsync(SemanaCreateDto dto)
    {
        var validation = SemanaValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<SemanaResponse>.Fail(validation.Message!);

        if (await _repository.ExistsByNumeroAnioAsync(dto.NumeroSemana, dto.Anio))
            return ServiceResult<SemanaResponse>.Fail($"Ya existe una semana {dto.NumeroSemana} del año {dto.Anio}.");

        var entity = new Semana
        {
            NumeroSemana = dto.NumeroSemana,
            Anio = dto.Anio,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = "Planificada",
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<SemanaResponse>.Ok(response, "Semana creada correctamente.");
    }

    public async Task<ServiceResult<SemanaResponse>> UpdateAsync(int id, SemanaUpdateDto dto)
    {
        var validation = SemanaValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<SemanaResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<SemanaResponse>.Fail($"La semana con Id {id} no existe.");

        if (await _repository.ExistsByNumeroAnioAsync(dto.NumeroSemana, dto.Anio, id))
            return ServiceResult<SemanaResponse>.Fail($"Ya existe una semana {dto.NumeroSemana} del año {dto.Anio}.");

        existing.NumeroSemana = dto.NumeroSemana;
        existing.Anio = dto.Anio;
        existing.FechaInicio = dto.FechaInicio;
        existing.FechaFin = dto.FechaFin;
        existing.Estado = dto.Estado;

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<SemanaResponse>.Ok(response, "Semana actualizada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"La semana con Id {id} no existe.");

        if (await _repository.HasDependentDataAsync(id))
            return ServiceResult.Fail("No se puede eliminar la semana porque tiene datos asociados (insumos, stock, tirado).");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Semana eliminada correctamente.");
    }

    public async Task<ServiceResult<SemanaResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<SemanaResponse>.Fail($"La semana con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<SemanaResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<SemanaResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<SemanaResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<List<SemanaResponse>>> GetByAnioAsync(int anio)
    {
        var entities = await _repository.GetAllAsync();
        var filtered = entities.Where(s => s.Anio == anio).Select(MapToResponse).ToList();
        return ServiceResult<List<SemanaResponse>>.Ok(filtered);
    }

    private static SemanaResponse MapToResponse(Semana entity)
    {
        return new SemanaResponse(
            entity.Id,
            entity.NumeroSemana,
            entity.Anio,
            entity.FechaInicio,
            entity.FechaFin,
            entity.Estado,
            entity.FechaCreacion,
            entity.Insumos?.Count
        );
    }
}