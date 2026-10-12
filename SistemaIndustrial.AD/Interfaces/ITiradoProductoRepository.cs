using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface ITiradoProductoRepository : IRepository<TiradoProducto>
{
    Task<List<TiradoProducto>> GetBySemanaIdAsync(int semanaId);
    Task<bool> ExistsByNombreSemanaAsync(string nombre, int semanaId, int? excludeId = null);
    Task<Tirada> AddTiradaAsync(Tirada tirada);
    Task<List<Tirada>> GetTiradasByProductoAsync(int productoId);
}