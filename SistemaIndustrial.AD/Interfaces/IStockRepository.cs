using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface IStockRepository : IRepository<StockSemana>
{
    Task<List<StockSemana>> GetBySemanaIdAsync(int semanaId);
    Task<int> CountBySemanaIdAsync(int semanaId);
}