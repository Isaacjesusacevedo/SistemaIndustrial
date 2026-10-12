namespace SistemaIndustrial.AD.Models;

public class ProductoCatalogo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PedidoSemanalAproximado { get; set; }
    public decimal PromedioPorTacho { get; set; }
    public bool Activo { get; set; } = true;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
}
