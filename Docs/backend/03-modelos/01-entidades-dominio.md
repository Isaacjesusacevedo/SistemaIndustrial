# Entidades de Dominio (AD Layer)

## Ubicación
`SistemaIndustrial.AD/Models/`

## Entidades

### Usuario
```csharp
public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Role { get; set; } = "Operador"; // Admin, Supervisor, Operador
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
```

**Validaciones Data Annotations:**
```csharp
[Required, EmailAddress, StringLength(255)]
public string Email { get; set; }

[Required, StringLength(255)]
public string PasswordHash { get; set; }

[Required, StringLength(100)]
public string Nombre { get; set; }

[Required, StringLength(50)]
public string Role { get; set; }
```

---

### Semana
```csharp
public class Semana
{
    public int Id { get; set; }
    public int NumeroSemana { get; set; }
    public int Anio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "Planificada"; // Planificada, EnProgreso, Completada, Cancelada
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Navigation properties (para EF Core Include)
    public virtual ICollection<InsumoSemana> Insumos { get; set; } = new List<InsumoSemana>();
    public virtual ICollection<StockSemana> Stocks { get; set; } = new List<StockSemana>();
    public virtual ICollection<TiradoProducto> TiradosProducto { get; set; } = new List<TiradoProducto>();
}
```

**Validaciones:**
```csharp
[Required, Range(1, 53)]
public int NumeroSemana { get; set; }

[Required, Range(2020, 2030)]
public int Anio { get; set; }

[Required]
public DateTime FechaInicio { get; set; }

[Required]
public DateTime FechaFin { get; set; }

[Required, StringLength(20)]
public string Estado { get; set; }
```

---

### Categoria
```csharp
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // Único (índice unique)
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;

    // Navigation property
    public virtual ICollection<TiradoProducto> Productos { get; set; } = new List<TiradoProducto>();
}
```

**Acceso a datos:** EF Core directo (`CategoriaRepository`, sin `SpInvoker`).

---

### TiradoProducto
```csharp
public class TiradoProducto
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public int? CategoriaId { get; set; } // Nullable, SetNull al borrar categoría
    public string Nombre { get; set; } = string.Empty; // Único por semana (índice SemanaId + Nombre)
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PedidoSemanal { get; set; }
    public decimal StockInicial { get; set; }
    public decimal PromedioPorTacho { get; set; }

    // Campos calculados — se actualizan al crear/eliminar una Tirada
    public decimal CantidadTirada { get; set; }
    public decimal Stock { get; set; }

    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation properties
    public virtual Semana Semana { get; set; } = null!;
    public virtual Categoria? Categoria { get; set; }
    public virtual ICollection<Tirada> Tiradas { get; set; } = new List<Tirada>();
}
```

**Acceso a datos:** EF Core directo con `Include(p => p.Categoria)` + `Include(p => p.Tiradas)` (`TiradoProductoRepository`, sin `SpInvoker`).

---

### Tirada
```csharp
public class Tirada
{
    public int Id { get; set; }
    public int TiradoProductoId { get; set; } // Cascade al borrar producto
    public DateTime Fecha { get; set; }
    public decimal Cantidad { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Navigation property
    public virtual TiradoProducto TiradoProducto { get; set; } = null!;
}
```

**Acceso a datos:** vía `TiradoProductoRepository` (`AddTiradaAsync`, `GetTiradasByProductoAsync`).

---

### ProductoCatalogo
```csharp
public class ProductoCatalogo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // Único (índice unique)
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PedidoSemanalAproximado { get; set; }
    public decimal PromedioPorTacho { get; set; }
    public bool Activo { get; set; } = true;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
}
```

**Notas:** catálogo general sin `SemanaId` (independiente de la semana). **Acceso a datos:** Stored Procedures vía `SpInvoker` (`sp_ProductosCatalogo_*`).

---

### InsumoSemana
```csharp
public class InsumoSemana
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty; // kg, lt, un, m, m2, m3
    public decimal CantidadRequerida { get; set; }
    public decimal CantidadConsumida { get; set; } = 0;
    public decimal StockDisponible { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
```

**Validaciones:**
```csharp
[Required, Range(1, int.MaxValue)]
public int SemanaId { get; set; }

[Required, StringLength(200)]
public string Nombre { get; set; }

[Required, StringLength(50)]
public string UnidadMedida { get; set; }

[Range(0, double.MaxValue)]
public decimal CantidadRequerida { get; set; }

[Range(0, double.MaxValue)]
public decimal CantidadConsumida { get; set; }

[Range(0, double.MaxValue)]
public decimal StockDisponible { get; set; }
```

---

### StockSemana
```csharp
public class StockSemana
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockInicial { get; set; }
    public decimal Entradas { get; set; } = 0;
    public decimal Salidas { get; set; } = 0;
    public decimal StockFinal { get; set; } // Calculado: Inicial + Entradas - Salidas
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
```

**Validaciones:**
```csharp
[Required, Range(1, int.MaxValue)]
public int SemanaId { get; set; }

[Required, StringLength(200)]
public string Producto { get; set; }

[Required, StringLength(50)]
public string UnidadMedida { get; set; }

[Range(0, double.MaxValue)]
public decimal StockInicial { get; set; }

[Range(0, double.MaxValue)]
public decimal Entradas { get; set; }

[Range(0, double.MaxValue)]
public decimal Salidas { get; set; }

[Range(0, double.MaxValue)]
public decimal StockFinal { get; set; }
```

---

### RegistroTacho

> **Nota:** el modelo existe en `AD/Models/`, pero **no tiene `DbSet` en `AppDbContext`**, su repositorio/servicio no están registrados en DI y su grupo de endpoints no está mapeado en `Program.cs`. Se documenta por completitud; el flujo de producción actual usa **TiradoProducto + Tirada**.

```csharp
public class RegistroTacho
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public int Turno { get; set; } // 1=Mañana, 2=Tarde, 3=Noche
    public string Operador { get; set; } = string.Empty;
    public decimal CantidadProducida { get; set; }
    public decimal CantidadRechazada { get; set; } = 0;
    public string EstadoCalidad { get; set; } = "Aprobado"; // Aprobado, Revision, Rechazado
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
```

**Validaciones:**
```csharp
[Required, Range(1, int.MaxValue)]
public int SemanaId { get; set; }

[Required, Range(1, 3)]
public int Turno { get; set; }

[Required, StringLength(100)]
public string Operador { get; set; }

[Range(0, double.MaxValue)]
public decimal CantidadProducida { get; set; }

[Range(0, double.MaxValue)]
public decimal CantidadRechazada { get; set; }

[Required, StringLength(20)]
public string EstadoCalidad { get; set; }

[Required]
public DateTime FechaRegistro { get; set; }
```

---

## Relaciones (Navigation Properties - Solo para EF Core)

```csharp
// En Semana
public virtual ICollection<InsumoSemana> Insumos { get; set; } = new List<InsumoSemana>();
public virtual ICollection<StockSemana> Stocks { get; set; } = new List<StockSemana>();
public virtual ICollection<TiradoProducto> TiradosProducto { get; set; } = new List<TiradoProducto>();

// En InsumoSemana
public virtual Semana Semana { get; set; } = null!;

// En StockSemana
public virtual Semana Semana { get; set; } = null!;

// En RegistroTacho (modelo sin DbSet)
public virtual Semana Semana { get; set; } = null!;

// En Categoria
public virtual ICollection<TiradoProducto> Productos { get; set; } = new List<TiradoProducto>();

// En TiradoProducto
public virtual Semana Semana { get; set; } = null!;
public virtual Categoria? Categoria { get; set; }
public virtual ICollection<Tirada> Tiradas { get; set; } = new List<Tirada>();

// En Tirada
public virtual TiradoProducto TiradoProducto { get; set; } = null!;
```

**Nota**: las navigation properties se usan con `Include()` en los repositorios de EF Core directo (Categoria, TiradoProducto). Los repositorios vía SPs usan mapeo manual y no las cargan.

---

## AppDbContext

```csharp
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
        
        // Configuración de precisión decimal
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
        
        // Semanas
        modelBuilder.Entity<Semana>(e =>
        {
            e.HasIndex(s => new { s.NumeroSemana, s.Anio }).IsUnique();
            e.Property(s => s.Estado).HasDefaultValue("Planificada");
        });
        
        // Insumos
        modelBuilder.Entity<InsumoSemana>(e =>
        {
            e.HasOne(i => i.Semana).WithMany(s => s.Insumos).HasForeignKey(i => i.SemanaId).OnDelete(DeleteBehavior.Cascade);
            e.Property(i => i.CantidadConsumida).HasDefaultValue(0m);
            e.Property(i => i.StockDisponible).HasDefaultValue(0m);
        });
        
        // Stock
        modelBuilder.Entity<StockSemana>(e =>
        {
            e.HasOne(s => s.Semana).WithMany(sem => sem.Stocks).HasForeignKey(s => s.SemanaId).OnDelete(DeleteBehavior.Cascade);
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
        
        // Usuarios
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("Operador");
            e.Property(u => u.Activo).HasDefaultValue(true);
        });
    }
}
```