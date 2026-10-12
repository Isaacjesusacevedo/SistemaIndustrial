using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface ISemanaRepository : IRepository<Semana>
{
    Task<bool> ExistsByNumeroAnioAsync(int numeroSemana, int anio, int? excludeId = null);
    Task<bool> HasDependentDataAsync(int id);
}