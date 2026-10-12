using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly ISemanaRepository _semanaRepository;
    private readonly ITiradoProductoService _tiradoService;

    public StockService(
        IStockRepository repository,
        ISemanaRepository semanaRepository,
        ITiradoProductoService tiradoService)
    {
        _repository = repository;
        _semanaRepository = semanaRepository;
        _tiradoService = tiradoService;
    }

    public async Task<ServiceResult<StockResponse>> CreateAsync(StockCreateDto dto)
    {
        var validation = StockValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<StockResponse>.Fail(validation.Message!);

        if (!await _semanaRepository.ExistsAsync(dto.SemanaId))
            return ServiceResult<StockResponse>.Fail($"La semana con Id {dto.SemanaId} no existe.");

        var entity = new StockSemana
        {
            SemanaId = dto.SemanaId,
            Producto = dto.Producto,
            UnidadMedida = dto.UnidadMedida,
            StockInicial = dto.StockInicial,
            Entradas = 0,
            Salidas = 0,
            StockFinal = dto.StockInicial,
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<StockResponse>.Ok(response, "Stock creado correctamente.");
    }

    public async Task<ServiceResult<StockResponse>> UpdateAsync(int id, StockUpdateDto dto)
    {
        var validation = StockValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<StockResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<StockResponse>.Fail($"El stock con Id {id} no existe.");

        existing.Producto = dto.Producto;
        existing.UnidadMedida = dto.UnidadMedida;
        existing.StockInicial = dto.StockInicial;
        existing.Entradas = dto.Entradas;
        existing.Salidas = dto.Salidas;
        existing.StockFinal = dto.StockInicial + dto.Entradas - dto.Salidas;

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<StockResponse>.Ok(response, "Stock actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"El stock con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Stock eliminado correctamente.");
    }

    public async Task<ServiceResult<StockResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<StockResponse>.Fail($"El stock con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<StockResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<StockResponse>>> GetBySemanaIdAsync(int semanaId)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<StockResponse>>.Fail($"La semana con Id {semanaId} no existe.");

        var entities = await _repository.GetBySemanaIdAsync(semanaId);
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<StockResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<List<StockResponse>>> SyncFromTiradoAsync(int semanaId)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<StockResponse>>.Fail("Semana no encontrada.");

        var tiradoResult = await _tiradoService.GetBySemanaIdAsync(semanaId);
        if (!tiradoResult.Success || tiradoResult.Data == null)
            return ServiceResult<List<StockResponse>>.Fail("Error al leer los productos de tirado.");

        var tirados = tiradoResult.Data;
        var stocksExistentes = await _repository.GetBySemanaIdAsync(semanaId);
        var nombresExistentes = stocksExistentes.Select(s => s.Producto.ToLower()).ToHashSet();

        int creados = 0;
        foreach (var t in tirados)
        {
            if (nombresExistentes.Contains(t.Nombre.ToLower())) continue;

            var nuevoStock = new StockSemana
            {
                SemanaId = semanaId,
                Producto = t.Nombre,
                UnidadMedida = t.UnidadMedida,
                StockInicial = t.StockInicial,
                Entradas = 0,
                Salidas = 0,
                StockFinal = t.StockInicial,
                FechaCreacion = DateTime.Now
            };

            await _repository.CreateAsync(nuevoStock);
            creados++;
        }

        var finales = await _repository.GetBySemanaIdAsync(semanaId);
        var responses = finales.Select(MapToResponse).ToList();

        var msg = creados > 0
            ? $"{creados} productos sincronizados desde Tirado."
            : "Todos los productos ya estaban sincronizados.";

        return ServiceResult<List<StockResponse>>.Ok(responses, msg);
    }

    private static StockResponse MapToResponse(StockSemana entity)
    {
        return new StockResponse(
            entity.Id,
            entity.SemanaId,
            entity.Producto,
            entity.UnidadMedida,
            entity.StockInicial,
            entity.Entradas,
            entity.Salidas,
            entity.StockFinal,
            entity.FechaCreacion
        );
    }
}