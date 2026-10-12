using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;
    
    public StockRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }
    
    public async Task<List<StockSemana>> GetAllAsync()
    {
        return await _spInvoker.QueryEntityAsync<StockSemana>("EXEC sp_StockSemana_GetAll");
    }
    
    public async Task<StockSemana?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<StockSemana>("EXEC sp_StockSemana_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }
    
    public async Task<List<StockSemana>> GetBySemanaIdAsync(int semanaId)
    {
        return await _spInvoker.QueryEntityAsync<StockSemana>("EXEC sp_StockSemana_GetBySemanaId @SemanaId", new { SemanaId = semanaId });
    }
    
    public async Task<StockSemana> CreateAsync(StockSemana entity)
    {
        var id = await _spInvoker.ScalarAsync(
            "EXEC sp_StockSemana_Create @SemanaId, @Producto, @UnidadMedida, @StockInicial",
            new { entity.SemanaId, entity.Producto, entity.UnidadMedida, entity.StockInicial });
        entity.Id = Convert.ToInt32(id);
        return entity;
    }
    
    public async Task UpdateAsync(int id, StockSemana entity)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_StockSemana_Update @Id, @Producto, @UnidadMedida, @StockInicial, @Entradas, @Salidas",
            new { Id = id, entity.Producto, entity.UnidadMedida, entity.StockInicial, entity.Entradas, entity.Salidas });
    }
    
    public async Task DeleteAsync(int id)
    {
        await _spInvoker.NonQueryAsync("EXEC sp_StockSemana_Delete @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_StockSemana_Exists @Id", new { Id = id });
    }
    
    public async Task<int> CountBySemanaIdAsync(int semanaId)
    {
        var list = await _spInvoker.QueryEntityAsync<StockSemana>("EXEC sp_StockSemana_GetBySemanaId @SemanaId", new { SemanaId = semanaId });
        return list.Count;
    }
}