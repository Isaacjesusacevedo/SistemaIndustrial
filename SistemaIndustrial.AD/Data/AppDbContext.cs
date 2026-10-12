using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Semana> Semanas => Set<Semana>();
    public DbSet<InsumoSemana> InsumosSemana => Set<InsumoSemana>();
    public DbSet<StockSemana> StockSemana => Set<StockSemana>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ProductoCatalogo> ProductosCatalogo => Set<ProductoCatalogo>();
    public DbSet<TiradoProducto> TiradosProducto => Set<TiradoProducto>();
    public DbSet<Tirada> Tiradas => Set<Tirada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de precisión decimal global
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
        
        // Usuario
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("Operador");
            e.Property(u => u.Activo).HasDefaultValue(true);
        });
        
        // Semana
        modelBuilder.Entity<Semana>(e =>
        {
            e.HasIndex(s => new { s.NumeroSemana, s.Anio }).IsUnique();
            e.Property(s => s.Estado).HasDefaultValue("Planificada");
        });
        
        // InsumoSemana
        modelBuilder.Entity<InsumoSemana>(e =>
        {
            e.HasOne(i => i.Semana)
             .WithMany(s => s.Insumos)
             .HasForeignKey(i => i.SemanaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(i => i.CantidadConsumida).HasDefaultValue(0m);
            e.Property(i => i.StockDisponible).HasDefaultValue(0m);
        });
        
        // StockSemana
        modelBuilder.Entity<StockSemana>(e =>
        {
            e.HasOne(s => s.Semana)
             .WithMany(sem => sem.Stocks)
             .HasForeignKey(s => s.SemanaId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(s => s.Entradas).HasDefaultValue(0m);
            e.Property(s => s.Salidas).HasDefaultValue(0m);
        });
        
        // Categoria
        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasIndex(c => c.Nombre).IsUnique();
            e.Property(c => c.Activa).HasDefaultValue(true);
            e.Property(c => c.Orden).HasDefaultValue(0);
        });

        // ProductoCatalogo (catálogo general, sin SemanaId)
        modelBuilder.Entity<ProductoCatalogo>(e =>
        {
            e.HasIndex(p => p.Nombre).IsUnique();
            e.Property(p => p.Activo).HasDefaultValue(true);
            e.Property(p => p.PedidoSemanalAproximado).HasDefaultValue(0m);
            e.Property(p => p.PromedioPorTacho).HasDefaultValue(0m);
            e.Property(p => p.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        // TiradoProducto
        modelBuilder.Entity<TiradoProducto>(e =>
        {
            e.HasOne(p => p.Semana)
             .WithMany(s => s.TiradosProducto)
             .HasForeignKey(p => p.SemanaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Productos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(p => new { p.SemanaId, p.Nombre }).IsUnique();

            e.Property(p => p.CantidadTirada).HasDefaultValue(0m);
            e.Property(p => p.Stock).HasDefaultValue(0m);
            e.Property(p => p.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        // Tirada
        modelBuilder.Entity<Tirada>(e =>
        {
            e.HasOne(t => t.TiradoProducto)
             .WithMany(p => p.Tiradas)
             .HasForeignKey(t => t.TiradoProductoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(t => t.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });
    }
}