using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using System.Data;
using System.Reflection;

namespace SistemaIndustrial.AD;

public class SpInvoker
{
    private readonly AppDbContext _context;
    
    public SpInvoker(AppDbContext context) => _context = context;
    
    public async Task<List<T>> QueryEntityAsync<T>(string sql, object? parameters = null) where T : class
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T>();
            
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name.ToLower(), p => p);
            
            while (await reader.ReadAsync())
            {
                var entity = Activator.CreateInstance<T>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i).ToLower();
                    if (props.TryGetValue(columnName, out var prop) && !reader.IsDBNull(i))
                    {
                        var value = reader.GetValue(i);
                        if (value != DBNull.Value)
                        {
                            prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
                        }
                    }
                }
                results.Add(entity);
            }
            return results;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(string sql, object? parameters = null)
    {
        var result = await ScalarAsync(sql, parameters);
        return Convert.ToInt32(result) > 0;
    }
    
    public async Task<object?> ScalarAsync(string sql, object? parameters = null)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            return await command.ExecuteScalarAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    
    public async Task<int> NonQueryAsync(string sql, object? parameters = null)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var param = command.CreateParameter();
                    param.ParameterName = "@" + prop.Name;
                    param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(param);
                }
            }
            
            return await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}