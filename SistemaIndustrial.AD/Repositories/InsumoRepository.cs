using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class InsumoRepository : IInsumoRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;
    
    public InsumoRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }
    
    public async Task<List<InsumoSemana>> GetAllAsync()
    {
        return await _spInvoker.QueryEntityAsync<InsumoSemana>("EXEC sp_InsumosSemana_GetAll");
    }
    
    public async Task<InsumoSemana?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<InsumoSemana>("EXEC sp_InsumosSemana_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }
    
    public async Task<List<InsumoSemana>> GetBySemanaIdAsync(int semanaId)
    {
        return await _spInvoker.QueryEntityAsync<InsumoSemana>("EXEC sp_InsumosSemana_GetBySemanaId @SemanaId", new { SemanaId = semanaId });
    }
    
    public async Task<InsumoSemana> CreateAsync(InsumoSemana entity)
    {
        var id = await _spInvoker.ScalarAsync(
            "EXEC sp_InsumosSemana_Create @SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida",
            new { entity.SemanaId, entity.Nombre, entity.UnidadMedida, entity.CantidadRequerida });
        entity.Id = Convert.ToInt32(id);
        return entity;
    }
    
    public async Task UpdateAsync(int id, InsumoSemana entity)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_InsumosSemana_Update @Id, @Nombre, @UnidadMedida, @CantidadRequerida, @CantidadConsumida, @StockDisponible",
            new { Id = id, entity.Nombre, entity.UnidadMedida, entity.CantidadRequerida, entity.CantidadConsumida, entity.StockDisponible });
    }
    
    public async Task DeleteAsync(int id)
    {
        await _spInvoker.NonQueryAsync("EXEC sp_InsumosSemana_Delete @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_InsumosSemana_Exists @Id", new { Id = id });
    }
    
    public async Task<int> CountBySemanaIdAsync(int semanaId)
    {
        var list = await _spInvoker.QueryEntityAsync<InsumoSemana>("EXEC sp_InsumosSemana_GetBySemanaId @SemanaId", new { SemanaId = semanaId });
        return list.Count;
    }
}