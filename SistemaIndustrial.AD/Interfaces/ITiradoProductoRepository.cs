using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface ITiradoProductoRepository : IRepository<TiradoProducto>
{
    Task<List<TiradoProducto>> GetBySemanaIdAsync(int semanaId);
    Task<List<TiradoProducto>> GetBySemanaIdsAsync(IEnumerable<int> semanaIds);
    Task<bool> ExistsByNombreSemanaAsync(string nombre, int semanaId, int? excludeId = null);
    Task<List<string>> GetNombresBySemanaAsync(int semanaId);
    Task<List<TiradoProducto>> CreateRangeAsync(IReadOnlyCollection<TiradoProducto> entities);
    Task<Tirada> AddTiradaAsync(Tirada tirada);
    Task<List<Tirada>> GetTiradasByProductoAsync(int productoId);
}