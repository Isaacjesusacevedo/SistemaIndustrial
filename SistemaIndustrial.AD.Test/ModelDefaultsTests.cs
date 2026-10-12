using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Test;

[TestClass]
public class ModelDefaultsTests
{
    [TestMethod]
    public void ProductoCatalogo_ValoresPorDefecto()
    {
        var p = new ProductoCatalogo();

        Assert.AreEqual(string.Empty, p.Nombre);
        Assert.IsTrue(p.Activo);
        Assert.IsNull(p.Observaciones);
        Assert.IsNull(p.FechaActualizacion);
        Assert.IsTrue((DateTime.Now - p.FechaCreacion).TotalSeconds < 5);
    }

    [TestMethod]
    public void Semana_EstadoInicial_Planificada()
    {
        var s = new Semana();
        Assert.AreEqual("Planificada", s.Estado);
        Assert.AreEqual(0, s.Insumos.Count);
    }

    [TestMethod]
    public void TiradoProducto_ColeccionTiradas_Vacia()
    {
        var t = new TiradoProducto();
        Assert.AreEqual(0, t.CantidadTirada);
        Assert.AreEqual(0, t.Tiradas.Count);
    }
}
