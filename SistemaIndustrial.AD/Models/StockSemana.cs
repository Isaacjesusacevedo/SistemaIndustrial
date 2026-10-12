namespace SistemaIndustrial.AD.Models;

public class StockSemana
{
    public int Id { get; set; }
    public int SemanaId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockInicial { get; set; }
    public decimal Entradas { get; set; } = 0;
    public decimal Salidas { get; set; } = 0;
    public decimal StockFinal { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public virtual Semana Semana { get; set; } = null!;
}