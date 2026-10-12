using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using SistemaIndustrial.API.Models;

namespace SistemaIndustrial.API.Test;

[TestClass]
public class ApiModelsValidationTests
{
    private static IList<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, true);
        return resultados;
    }

    [TestMethod]
    public void ProductoCatalogoCreateRequest_Valido_SinErrores()
    {
        var req = new ProductoCatalogoCreateRequest
        {
            Nombre = "Cherry",
            UnidadMedida = "Galón (4 L)",
            PedidoSemanalAproximado = 500,
            PromedioPorTacho = 250
        };
        Assert.AreEqual(0, Validar(req).Count);
    }

    [TestMethod]
    public void ProductoCatalogoCreateRequest_NombreVacio_Errores()
    {
        var req = new ProductoCatalogoCreateRequest { Nombre = "", UnidadMedida = "un" };
        Assert.IsTrue(Validar(req).Count > 0);
    }

    [TestMethod]
    public void TiradoProductoUpdateRequest_CantidadNegativa_Errores()
    {
        var req = new TiradoProductoUpdateRequest
        {
            Nombre = "Cherry",
            UnidadMedida = "Galón (4 L)",
            PedidoSemanal = 500,
            PromedioPorTacho = 250,
            CantidadTirada = -5
        };
        Assert.IsTrue(Validar(req).Count > 0);
    }

    [TestMethod]
    public void ApiResponse_Ok_MarcaSuccess()
    {
        var resp = ApiResponse<string>.Ok("hola");
        Assert.IsTrue(resp.Success);
        Assert.AreEqual("hola", resp.Data);
    }
}
