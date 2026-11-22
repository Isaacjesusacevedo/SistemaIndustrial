using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Services;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class SemanaServiceTests
{
    private static DateTime ProximoLunes()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Monday)
            d = d.AddDays(1);
        return d;
    }

    private static Mock<ISemanaRepository> NuevoRepoMock()
    {
        var mock = new Mock<ISemanaRepository>();
        mock.Setup(r => r.ExistsByNumeroAnioAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        mock.Setup(r => r.CreateAsync(It.IsAny<Semana>()))
            .ReturnsAsync((Semana s) => { s.Id = 1; return s; });
        return mock;
    }

    [TestMethod]
    public async Task CreateAsync_SemanaValida_Crea()
    {
        var lunes = ProximoLunes();
        var service = new SemanaService(NuevoRepoMock().Object);

        var result = await service.CreateAsync(new SemanaCreateDto(10, 2026, lunes, lunes.AddDays(5)));

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data!.Id);
        Assert.AreEqual("Planificada", result.Data.Estado);
    }

    [TestMethod]
    public async Task CreateAsync_SemanaDuplicada_RechazaAntesDelRepository()
    {
        var mock = NuevoRepoMock();
        mock.Setup(r => r.ExistsByNumeroAnioAsync(10, 2026, null)).ReturnsAsync(true);
        var service = new SemanaService(mock.Object);
        var lunes = ProximoLunes();

        var result = await service.CreateAsync(new SemanaCreateDto(10, 2026, lunes, lunes.AddDays(5)));

        Assert.IsFalse(result.Success);
        mock.Verify(r => r.CreateAsync(It.IsAny<Semana>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAsync_FechasInvalidas_NoTocaRepository()
    {
        var mock = NuevoRepoMock();
        var service = new SemanaService(mock.Object);
        var lunes = ProximoLunes();

        // Domingo como fin: no es sábado
        var result = await service.CreateAsync(new SemanaCreateDto(10, 2026, lunes, lunes.AddDays(6)));

        Assert.IsFalse(result.Success);
        mock.Verify(r => r.CreateAsync(It.IsAny<Semana>()), Times.Never);
    }

    // ---------- DeleteAsync ----------

    private const string MensajeConProduccion =
        "No se puede eliminar: la semana ya tiene producción registrada. Editala o eliminá primero sus tiradas.";

    private static Mock<ISemanaRepository> RepoParaBorrar(string estado, bool tieneTiradas)
    {
        var mock = NuevoRepoMock();
        mock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Semana { Id = 5, NumeroSemana = 10, Anio = 2026, Estado = estado });
        mock.Setup(r => r.HasTiradasAsync(5)).ReturnsAsync(tieneTiradas);
        return mock;
    }

    [TestMethod]
    public async Task DeleteAsync_PlanificadaSinTiradas_Elimina()
    {
        var mock = RepoParaBorrar("Planificada", tieneTiradas: false);
        var service = new SemanaService(mock.Object);

        var result = await service.DeleteAsync(5);

        Assert.IsTrue(result.Success);
        mock.Verify(r => r.DeleteAsync(5), Times.Once);
        mock.Verify(r => r.HasDependentDataAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_Completada_Rechaza()
    {
        var mock = RepoParaBorrar("Completada", tieneTiradas: false);
        var service = new SemanaService(mock.Object);

        var result = await service.DeleteAsync(5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(MensajeConProduccion, result.Message);
        mock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_ConTiradas_Rechaza()
    {
        var mock = RepoParaBorrar("EnProgreso", tieneTiradas: true);
        var service = new SemanaService(mock.Object);

        var result = await service.DeleteAsync(5);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(MensajeConProduccion, result.Message);
        mock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
}
