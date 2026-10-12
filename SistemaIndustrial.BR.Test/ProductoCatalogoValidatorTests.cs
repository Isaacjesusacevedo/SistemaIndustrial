using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class ProductoCatalogoValidatorTests
{
    private static ProductoCatalogoCreateDto CrearDto(
        string nombre = "Cherry",
        string unidad = "Galón (4 L)",
        decimal pedido = 500,
        decimal promedio = 250,
        string? obs = null) =>
        new(nombre, unidad, pedido, promedio, obs);

    [TestMethod]
    public void ValidateCreate_DatosValidos_Acepta()
    {
        Assert.IsTrue(ProductoCatalogoValidator.ValidateCreate(CrearDto()).Success);
    }

    [TestMethod]
    public void ValidateCreate_NombreVacio_Rechaza()
    {
        var result = ProductoCatalogoValidator.ValidateCreate(CrearDto(nombre: "  "));
        Assert.IsFalse(result.Success);
        Assert.AreEqual("El nombre es obligatorio.", result.Message);
    }

    [TestMethod]
    public void ValidateCreate_NombreMuyLargo_Rechaza()
    {
        var result = ProductoCatalogoValidator.ValidateCreate(CrearDto(nombre: new string('X', 201)));
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ValidateCreate_UnidadVacia_Rechaza()
    {
        var result = ProductoCatalogoValidator.ValidateCreate(CrearDto(unidad: ""));
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ValidateCreate_ValoresNegativos_Rechaza()
    {
        Assert.IsFalse(ProductoCatalogoValidator.ValidateCreate(CrearDto(pedido: -1)).Success);
        Assert.IsFalse(ProductoCatalogoValidator.ValidateCreate(CrearDto(promedio: -0.5m)).Success);
    }

    [TestMethod]
    public void ValidateCreate_ObservacionesLargas_Rechaza()
    {
        var result = ProductoCatalogoValidator.ValidateCreate(CrearDto(obs: new string('X', 501)));
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ValidateUpdate_ActivoFalso_Acepta()
    {
        var dto = new ProductoCatalogoUpdateDto("Cherry", "Galón (4 L)", 500, 250, false, null);
        Assert.IsTrue(ProductoCatalogoValidator.ValidateUpdate(dto).Success);
    }
}
