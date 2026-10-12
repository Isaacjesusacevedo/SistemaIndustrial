using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface IInsumoRepository : IRepository<InsumoSemana>
{
    Task<List<InsumoSemana>> GetBySemanaIdAsync(int semanaId);
    Task<int> CountBySemanaIdAsync(int semanaId);
}