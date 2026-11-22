using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Services;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class TiradoProductoServiceTests
{
    private const int SemanaId = 7;

    private static (Mock<ITiradoProductoRepository> Repo, Mock<ISemanaRepository> Semanas, Mock<ICategoriaRepository> Categorias) NuevosMocks()
    {
        var repo = new Mock<ITiradoProductoRepository>();
        var semanas = new Mock<ISemanaRepository>();
        var categorias = new Mock<ICategoriaRepository>();
        semanas.Setup(r => r.ExistsAsync(SemanaId)).ReturnsAsync(true);
        return (repo, semanas, categorias);
    }

    private static TiradoProducto Producto(int id, string nombre, params Tirada[] tiradas) => new()
    {
        Id = id,
        SemanaId = SemanaId,
        Nombre = nombre,
        UnidadMedida = "Unidad",
        PromedioPorTacho = 10,
        Tiradas = tiradas.ToList()
    };

    private static Tirada Tirada(int id, DateTime fecha, decimal cantidad) => new()
    {
        Id = id,
        Fecha = fecha,
        Cantidad = cantidad,
        Observaciones = "obs",
        FechaCreacion = fecha
    };

    private static TiradoProductoBulkItemDto Item(string nombre, int? categoriaId = null) =>
        new(nombre, "Unidad", 10, 100, 50, categoriaId, null);

    // ---------- GetTiradasBySemanaAsync ----------

    [TestMethod]
    public async Task GetTiradasBySemanaAsync_UsaTiradasIncluidas_OrdenaPorFechaDesc()
    {
        var (repo, semanas, categorias) = NuevosMocks();
        var hoy = new DateTime(2026, 11, 20, 12, 0, 0);
        repo.Setup(r => r.GetBySemanaIdAsync(SemanaId)).ReturnsAsync(new List<TiradoProducto>
        {
            Producto(1, "Cherry", Tirada(10, hoy.AddDays(-2), 5), Tirada(11, hoy, 3)),
            Producto(2, "Limón", Tirada(20, hoy.AddDays(-1), 8)),
            Producto(3, "Sin tiradas")
        });
        var service = new TiradoProductoService(repo.Object, semanas.Object, categorias.Object);

        var result = await service.GetTiradasBySemanaAsync(SemanaId);

        Assert.IsTrue(result.Success);
        CollectionAssert.AreEqual(new[] { 11, 20, 10 }, result.Data!.Select(t => t.Id).ToArray());
        var limon = result.Data!.Single(t => t.Id == 20);
        Assert.AreEqual(2, limon.TiradoProductoId);
        Assert.AreEqual("Limón", limon.ProductoNombre);
        Assert.AreEqual(8m, limon.Cantidad);

        repo.Verify(r => r.GetTiradasByProductoAsync(It.IsAny<int>()), Times.Never);
        // Con datos, no hace falta verificar la existencia de la semana.
        semanas.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task GetTiradasBySemanaAsync_SemanaInexistente_MismoError()
    {
        var (repo, semanas, categorias) = NuevosMocks();
        repo.Setup(r => r.GetBySemanaIdAsync(99)).ReturnsAsync(new List<TiradoProducto>());
        semanas.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);
        var service = new TiradoProductoService(repo.Object, semanas.Object, categorias.Object);

        var result = await service.GetTiradasBySemanaAsync(99);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("La semana con Id 99 no existe.", result.Message);
    }

    [TestMethod]
    public async Task GetTiradasBySemanaAsync_SemanaVacia_DevuelveListaVacia()
    {
        var (repo, semanas, categorias) = NuevosMocks();
        repo.Setup(r => r.GetBySemanaIdAsync(SemanaId)).ReturnsAsync(new List<TiradoProducto>());
        var service = new TiradoProductoService(repo.Object, semanas.Object, categorias.Object);

        var result = await service.GetTiradasBySemanaAsync(SemanaId);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data!.Count);
        semanas.Verify(r => r.ExistsAsync(SemanaId), Times.Once);
    }

    // ---------- CreateBulkAsync ----------

    [TestMethod]
    public async Task CreateBulkAsync_IgnoraDuplicados_YUsaUnSoloCreateRange()
    {
        var (repo, semanas, categorias) = NuevosMocks();
        repo.Setup(r => r.GetNombresBySemanaAsync(SemanaId)).ReturnsAsync(new List<string> { "Cherry" });
        categorias.Setup(r => r.GetExistingIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<int> { 1 });

        var id = 100;
        List<TiradoProducto>? insertados = null;
        repo.Setup(r => r.CreateRangeAsync(It.IsAny<IReadOnlyCollection<TiradoProducto>>()))
            .ReturnsAsync((IReadOnlyCollection<TiradoProducto> es) =>
            {
                foreach (var e in es) e.Id = id++;
                insertados = es.ToList();
                return es.ToList();
            });

        var service = new TiradoProductoService(repo.Object, semanas.Object, categorias.Object);
        var dto = new TiradoProductoBulkCreateDto(new List<TiradoProductoBulkItemDto>
        {
            Item("CHERRY"),           // duplicado contra la base (case-insensitive)
            Item("Limón", 1),
            Item("limón"),            // duplicado dentro del lote
            Item("Uva", 999),         // categoría inexistente
            Item("Frutilla"),
        });

        var result = await service.CreateBulkAsync(SemanaId, dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data!.Creados);
        Assert.AreEqual(3, result.Data.Ignorados);
        CollectionAssert.AreEqual(new[] { "Limón", "Frutilla" }, insertados!.Select(e => e.Nombre).ToArray());
        CollectionAssert.AreEqual(new[] { 100, 101 }, result.Data.ProductosCreados.Select(p => p.Id).ToArray());
        CollectionAssert.AreEqual(new[]
        {
            "'CHERRY': ya existe en esta semana (ignorado).",
            "'limón': ya existe en esta semana (ignorado).",
            "'Uva': la categoría no existe (ignorado).",
        }, result.Data.Errores);
        Assert.AreEqual("2 productos cargados correctamente. 3 ignorados.", result.Message);

        repo.Verify(r => r.CreateRangeAsync(It.IsAny<IReadOnlyCollection<TiradoProducto>>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<TiradoProducto>()), Times.Never);
        repo.Verify(r => r.ExistsByNombreSemanaAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
        repo.Verify(r => r.GetNombresBySemanaAsync(SemanaId), Times.Once);
        categorias.Verify(r => r.GetExistingIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
        categorias.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateBulkAsync_SiFallaElLote_ReintentaUnoPorUnoYReportaErrores()
    {
        var (repo, semanas, categorias) = NuevosMocks();
        repo.Setup(r => r.GetNombresBySemanaAsync(SemanaId)).ReturnsAsync(new List<string>());
        repo.Setup(r => r.CreateRangeAsync(It.IsAny<IReadOnlyCollection<TiradoProducto>>()))
            .ThrowsAsync(new InvalidOperationException("fallo lote"));
        repo.Setup(r => r.CreateAsync(It.Is<TiradoProducto>(e => e.Nombre == "Malo")))
            .ThrowsAsync(new InvalidOperationException("fallo item"));
        repo.Setup(r => r.CreateAsync(It.Is<TiradoProducto>(e => e.Nombre != "Malo")))
            .ReturnsAsync((TiradoProducto e) => e);

        var service = new TiradoProductoService(repo.Object, semanas.Object, categorias.Object);
        var dto = new TiradoProductoBulkCreateDto(new List<TiradoProductoBulkItemDto> { Item("Bueno"), Item("Malo") });

        var result = await service.CreateBulkAsync(SemanaId, dto);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data!.Creados);
        Assert.AreEqual(1, result.Data.Ignorados);
        CollectionAssert.AreEqual(new[] { "'Malo': fallo item" }, result.Data.Errores);
        repo.Verify(r => r.CreateAsync(It.IsAny<TiradoProducto>()), Times.Exactly(2));
        // Sin categorías en el lote no se consulta la tabla de categorías.
        categorias.Verify(r => r.GetExistingIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
    }
}
