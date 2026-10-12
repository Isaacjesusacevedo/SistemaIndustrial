namespace SistemaIndustrial.AD.Models;

public class InsumoSemana
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal CantidadRequerida { get; set; }
    public decimal CantidadConsumida { get; set; } = 0;
    public decimal StockDisponible { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public virtual Semana Semana { get; set; } = null!;
}