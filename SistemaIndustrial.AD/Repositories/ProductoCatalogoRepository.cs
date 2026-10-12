using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class ProductoCatalogoRepository : IProductoCatalogoRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;

    public ProductoCatalogoRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }

    public async Task<List<ProductoCatalogo>> GetAllAsync()
    {
        return await _spInvoker.QueryEntityAsync<ProductoCatalogo>("EXEC sp_ProductosCatalogo_GetAll");
    }

    public async Task<ProductoCatalogo?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<ProductoCatalogo>("EXEC sp_ProductosCatalogo_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }

    public async Task<ProductoCatalogo> CreateAsync(ProductoCatalogo entity)
    {
        var id = await _spInvoker.ScalarAsync(
            "EXEC sp_ProductosCatalogo_Create @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Observaciones",
            new { entity.Nombre, entity.UnidadMedida, entity.PedidoSemanalAproximado, entity.PromedioPorTacho, entity.Observaciones });
        entity.Id = Convert.ToInt32(id);
        return entity;
    }

    public async Task UpdateAsync(int id, ProductoCatalogo entity)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_ProductosCatalogo_Update @Id, @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Activo, @Observaciones",
            new { Id = id, entity.Nombre, entity.UnidadMedida, entity.PedidoSemanalAproximado, entity.PromedioPorTacho, entity.Activo, entity.Observaciones });
    }

    public async Task DeleteAsync(int id)
    {
        await _spInvoker.NonQueryAsync("EXEC sp_ProductosCatalogo_Delete @Id", new { Id = id });
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_ProductosCatalogo_Exists @Id", new { Id = id });
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null)
    {
        return await _spInvoker.ExistsAsync(
            "EXEC sp_ProductosCatalogo_ExistsByNombre @Nombre, @ExcludeId",
            new { Nombre = nombre, ExcludeId = excludeId });
    }
}
