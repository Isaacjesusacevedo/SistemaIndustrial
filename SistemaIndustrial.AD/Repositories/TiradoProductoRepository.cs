using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class TiradoProductoRepository : ITiradoProductoRepository
{
    private readonly AppDbContext _context;

    public TiradoProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TiradoProducto>> GetAllAsync()
    {
        return await _context.TiradosProducto
            .Include(p => p.Categoria)
            .Include(p => p.Tiradas)
            .ToListAsync();
    }

    public async Task<TiradoProducto?> GetByIdAsync(int id)
    {
        return await _context.TiradosProducto
            .Include(p => p.Categoria)
            .Include(p => p.Tiradas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<TiradoProducto> CreateAsync(TiradoProducto entity)
    {
        _context.TiradosProducto.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(int id, TiradoProducto entity)
    {
        var existing = await _context.TiradosProducto.FindAsync(id);
        if (existing == null)
            throw new InvalidOperationException($"TiradoProducto con Id {id} no encontrado.");

        _context.Entry(existing).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.TiradosProducto.FindAsync(id);
        if (entity != null)
        {
            _context.TiradosProducto.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TiradosProducto.AnyAsync(p => p.Id == id);
    }

    public async Task<List<TiradoProducto>> GetBySemanaIdAsync(int semanaId)
    {
        return await _context.TiradosProducto
            .Include(p => p.Categoria)
            .Include(p => p.Tiradas)
            .Where(p => p.SemanaId == semanaId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNombreSemanaAsync(string nombre, int semanaId, int? excludeId = null)
    {
        var query = _context.TiradosProducto.Where(p => p.Nombre == nombre && p.SemanaId == semanaId);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<Tirada> AddTiradaAsync(Tirada tirada)
    {
        _context.Tiradas.Add(tirada);
        await _context.SaveChangesAsync();
        return tirada;
    }

    public async Task<List<Tirada>> GetTiradasByProductoAsync(int productoId)
    {
        return await _context.Tiradas
            .Where(t => t.TiradoProductoId == productoId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync();
    }
}