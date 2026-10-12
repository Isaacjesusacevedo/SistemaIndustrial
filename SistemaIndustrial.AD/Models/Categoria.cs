namespace SistemaIndustrial.AD.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;

    // Navigation property
    public virtual ICollection<TiradoProducto> Productos { get; set; } = new List<TiradoProducto>();
}