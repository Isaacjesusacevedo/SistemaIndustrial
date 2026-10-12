using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Services;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class ProductoCatalogoServiceTests
{
    private static Mock<IProductoCatalogoRepository> NuevoRepoMock()
    {
        var mock = new Mock<IProductoCatalogoRepository>();
        mock.Setup(r => r.ExistsByNombreAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        mock.Setup(r => r.CreateAsync(It.IsAny<ProductoCatalogo>()))
            .ReturnsAsync((ProductoCatalogo e) => { e.Id = 7; return e; });
        return mock;
    }

    [TestMethod]
    public async Task CreateAsync_ProductoValido_CreaActivo()
    {
        var service = new ProductoCatalogoService(NuevoRepoMock().Object);

        var result = await service.CreateAsync(
            new ProductoCatalogoCreateDto("Cherry", "Galón (4 L)", 500, 250, null));

        Assert.IsTrue(result.Success);
        Assert.AreEqual(7, result.Data!.Id);
        Assert.IsTrue(result.Data.Activo);
    }

    [TestMethod]
    public async Task CreateAsync_NombreDuplicado_Rechaza()
    {
        var mock = NuevoRepoMock();
        mock.Setup(r => r.ExistsByNombreAsync("Cherry", null)).ReturnsAsync(true);
        var service = new ProductoCatalogoService(mock.Object);

        var result = await service.CreateAsync(
            new ProductoCatalogoCreateDto("Cherry", "Galón (4 L)", 500, 250, null));

        Assert.IsFalse(result.Success);
        mock.Verify(r => r.CreateAsync(It.IsAny<ProductoCatalogo>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_Inexistente_Rechaza()
    {
        var mock = NuevoRepoMock();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ProductoCatalogo?)null);
        var service = new ProductoCatalogoService(mock.Object);

        var result = await service.DeleteAsync(999);

        Assert.IsFalse(result.Success);
    }
}
