namespace SistemaIndustrial.AD.Models;

public class Semana
{
    public int Id { get; set; }
    public int NumeroSemana { get; set; }
    public int Anio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "Planificada";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Navigation properties (para EF Core Include)
    public virtual ICollection<InsumoSemana> Insumos { get; set; } = new List<InsumoSemana>();
    public virtual ICollection<StockSemana> Stocks { get; set; } = new List<StockSemana>();
    public virtual ICollection<TiradoProducto> TiradosProducto { get; set; } = new List<TiradoProducto>();
}