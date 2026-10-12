using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;
    private readonly SpInvoker _spInvoker;
    
    public UsuarioRepository(AppDbContext context, SpInvoker spInvoker)
    {
        _context = context;
        _spInvoker = spInvoker;
    }
    
    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _spInvoker.QueryEntityAsync<Usuario>("EXEC sp_Usuarios_GetAll");
    }
    
    public async Task<Usuario?> GetByIdAsync(int id)
    {
        var list = await _spInvoker.QueryEntityAsync<Usuario>("EXEC sp_Usuarios_GetById @Id", new { Id = id });
        return list.FirstOrDefault();
    }
    
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var list = await _spInvoker.QueryEntityAsync<Usuario>("EXEC sp_Usuarios_GetByEmail @Email", new { Email = email });
        return list.FirstOrDefault();
    }
    
    public async Task<Usuario> CreateAsync(Usuario entity)
    {
        var id = await _spInvoker.ScalarAsync(
            "EXEC sp_Usuarios_Create @Email, @PasswordHash, @Nombre, @Role",
            new { entity.Email, entity.PasswordHash, entity.Nombre, entity.Role });
        entity.Id = Convert.ToInt32(id);
        return entity;
    }
    
    public async Task UpdateAsync(int id, Usuario entity)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_Usuarios_Update @Id, @Nombre, @Role, @Activo",
            new { Id = id, entity.Nombre, entity.Role, entity.Activo });
    }
    
    public async Task DeleteAsync(int id)
    {
        await _spInvoker.NonQueryAsync("EXEC sp_Usuarios_Delete @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_Usuarios_Exists @Id", new { Id = id });
    }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _spInvoker.ExistsAsync("EXEC sp_Usuarios_ExistsByEmail @Email", new { Email = email });
    }
    
    public async Task UpdateRefreshTokenAsync(int userId, string? refreshTokenHash, DateTime? expiry)
    {
        await _spInvoker.NonQueryAsync(
            "EXEC sp_Usuarios_UpdateRefreshToken @Id, @RefreshTokenHash, @RefreshTokenExpiry",
            new { Id = userId, RefreshTokenHash = refreshTokenHash, RefreshTokenExpiry = expiry });
    }
}