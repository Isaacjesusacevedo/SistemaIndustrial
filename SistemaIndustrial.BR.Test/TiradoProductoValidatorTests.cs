using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class TiradoProductoValidatorTests
{
    [TestMethod]
    public void ValidateUpdate_CantidadTiradaNegativa_Rechaza()
    {
        var dto = new TiradoProductoUpdateDto("Cherry", "Galón (4 L)", 500, 100, 250, null, null, -5);
        var result = TiradoProductoValidator.ValidateUpdate(dto);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("La cantidad tirada no puede ser negativa.", result.Message);
    }

    [TestMethod]
    public void ValidateUpdate_CantidadTiradaCero_Acepta()
    {
        var dto = new TiradoProductoUpdateDto("Cherry", "Galón (4 L)", 500, 100, 250, null, null, 0);
        Assert.IsTrue(TiradoProductoValidator.ValidateUpdate(dto).Success);
    }

    [TestMethod]
    public void ValidateUpdate_SinCantidadTirada_Acepta()
    {
        var dto = new TiradoProductoUpdateDto("Cherry", "Galón (4 L)", 500, 100, 250, null, null, null);
        Assert.IsTrue(TiradoProductoValidator.ValidateUpdate(dto).Success);
    }

    [TestMethod]
    public void ValidateRegistrarTirada_CantidadNegativa_Rechaza()
    {
        Assert.IsFalse(TiradoProductoValidator.ValidateRegistrarTirada(new RegistrarTiradaDto(-1, null, null)).Success);
    }
}
