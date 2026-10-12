using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(int id, T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}