namespace SistemaIndustrial.AD.Models;

public class TiradoProducto
{
    public int Id { get; set; }
    public int SemanaId { get; set; }

    public int? CategoriaId { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;

    // Renombrado: CantidadRequerida → PedidoSemanal
    public decimal PedidoSemanal { get; set; }

    // ✅ Nuevos campos
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