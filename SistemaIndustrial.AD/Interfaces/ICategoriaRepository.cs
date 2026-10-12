using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    new Task<List<Categoria>> GetAllAsync();
}