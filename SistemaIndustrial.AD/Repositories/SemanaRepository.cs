using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class SemanaRepository : ISemanaRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;
    
    public SemanaRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }
    
    public async Task<List<Semana>> GetAllAsync()
    {
        return await _spInvoker.QueryEntityAsync<Semana>("EXEC sp_Semanas_GetAll");
    }
    
    public async Task<Semana?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<Semana>("EXEC sp_Semanas_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }
    
    public async Task<Semana> CreateAsync(Semana entity)
    {
        var id = await _spInvoker.ScalarAsync(
            "EXEC sp_Semanas_Create @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado",
            new { entity.NumeroSemana, entity.Anio, entity.FechaInicio, entity.FechaFin, entity.Estado });
        entity.Id = Convert.ToInt32(id);
        return entity;
    }
    
    public async Task UpdateAsync(int id, Semana entity)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_Semanas_Update @Id, @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado",
            new { Id = id, entity.NumeroSemana, entity.Anio, entity.FechaInicio, entity.FechaFin, entity.Estado });
    }
    
    public async Task DeleteAsync(int id)
    {
        await _spInvoker.NonQueryAsync("EXEC sp_Semanas_Delete @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_Semanas_Exists @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsByNumeroAnioAsync(int numeroSemana, int anio, int? excludeId = null)
    {
        return await _spInvoker.ExistsAsync(
            "EXEC sp_Semanas_ExistsByNumeroAnio @NumeroSemana, @Anio, @ExcludeId",
            new { NumeroSemana = numeroSemana, Anio = anio, ExcludeId = excludeId });
    }
    
    public async Task<bool> HasDependentDataAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_Semanas_HasDependentData @Id", new { Id = id });
    }
}