using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> GetAllAsync()
    {
        return await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<Categoria?> GetByIdAsync(int id)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Categoria> CreateAsync(Categoria entity)
    {
        _context.Categorias.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(int id, Categoria entity)
    {
        var existing = await _context.Categorias.FindAsync(id);
        if (existing == null)
            throw new InvalidOperationException($"Categoría con Id {id} no encontrada.");

        _context.Entry(existing).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Categorias.FindAsync(id);
        if (entity != null)
        {
            _context.Categorias.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == id);
    }

    public async Task<List<int>> GetExistingIdsAsync(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        if (lista.Count == 0)
            return new List<int>();

        // EF.Constant: IN (1, 2, ...) en lugar de OPENJSON (requiere compat level >= 130).
        return await _context.Categorias
            .Where(c => EF.Constant(lista).Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();
    }
}