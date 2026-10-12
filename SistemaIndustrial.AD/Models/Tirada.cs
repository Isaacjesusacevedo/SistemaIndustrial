namespace SistemaIndustrial.AD.Models;

public class Tirada
{
    public int Id { get; set; }
    public int TiradoProductoId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Cantidad { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Navigation property
    public virtual TiradoProducto TiradoProducto { get; set; } = null!;
}