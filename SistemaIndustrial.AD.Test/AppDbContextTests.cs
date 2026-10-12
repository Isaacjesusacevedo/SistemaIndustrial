using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Models;

namespace SistemaIndustrial.AD.Test;

[TestClass]
public class AppDbContextTests
{
    private static AppDbContext NuevoContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [TestMethod]
    public async Task Semana_CRUD_InMemory()
    {
        using var ctx = NuevoContexto();
        ctx.Semanas.Add(new Semana { NumeroSemana = 10, Anio = 2026, FechaInicio = DateTime.Today, FechaFin = DateTime.Today.AddDays(5) });
        await ctx.SaveChangesAsync();

        var leida = await ctx.Semanas.FirstAsync();
        Assert.AreEqual(10, leida.NumeroSemana);
        Assert.AreEqual("Planificada", leida.Estado);
    }

    [TestMethod]
    public async Task ProductoCatalogo_CRUD_InMemory()
    {
        using var ctx = NuevoContexto();
        ctx.ProductosCatalogo.Add(new ProductoCatalogo
        {
            Nombre = "Cherry",
            UnidadMedida = "Galón (4 L)",
            PedidoSemanalAproximado = 500,
            PromedioPorTacho = 250
        });
        await ctx.SaveChangesAsync();

        var leido = await ctx.ProductosCatalogo.FirstAsync(p => p.Nombre == "Cherry");
        Assert.IsTrue(leido.Activo);
        Assert.AreEqual(250, leido.PromedioPorTacho);
    }

    [TestMethod]
    public async Task TiradoProducto_ConTiradas_InMemory()
    {
        using var ctx = NuevoContexto();
        var producto = new TiradoProducto
        {
            SemanaId = 1,
            Nombre = "Cherry",
            UnidadMedida = "Galón (4 L)",
            PedidoSemanal = 500,
            StockInicial = 100,
            PromedioPorTacho = 250
        };
        producto.Tiradas.Add(new Tirada { Fecha = DateTime.Today, Cantidad = 120 });
        ctx.TiradosProducto.Add(producto);
        await ctx.SaveChangesAsync();

        var leido = await ctx.TiradosProducto.Include(p => p.Tiradas).FirstAsync();
        Assert.AreEqual(1, leido.Tiradas.Count);
        Assert.AreEqual(120, leido.Tiradas.First().Cantidad);
    }
}
