using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface IProductoCatalogoRepository : IRepository<ProductoCatalogo>
{
    Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null);
}
