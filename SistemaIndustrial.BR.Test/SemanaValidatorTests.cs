using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Test;

[TestClass]
public class SemanaValidatorTests
{
    private static DateTime ProximoLunes()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Monday)
            d = d.AddDays(1);
        return d;
    }

    private static SemanaCreateDto CrearDto(DateTime inicio, DateTime fin) =>
        new(10, 2026, inicio, fin);

    [TestMethod]
    public void ValidarFechasSemana_LunesASabado_Valido()
    {
        var lunes = ProximoLunes();
        var (valido, mensaje) = SemanaValidator.ValidarFechasSemana(lunes, lunes.AddDays(5));
        Assert.IsTrue(valido);
        Assert.IsNull(mensaje);
    }

    [TestMethod]
    public void ValidarFechasSemana_InicioNoLunes_Rechaza()
    {
        var martes = ProximoLunes().AddDays(1);
        var (valido, mensaje) = SemanaValidator.ValidarFechasSemana(martes, martes.AddDays(5));
        Assert.IsFalse(valido);
        Assert.AreEqual("La fecha de inicio debe ser un lunes.", mensaje);
    }

    [TestMethod]
    public void ValidarFechasSemana_FinNoSabado_Rechaza()
    {
        var lunes = ProximoLunes();
        var (valido, mensaje) = SemanaValidator.ValidarFechasSemana(lunes, lunes.AddDays(6));
        Assert.IsFalse(valido);
        Assert.AreEqual("La fecha de fin debe ser un sábado.", mensaje);
    }

    [TestMethod]
    public void ValidarFechasSemana_DuracionIncorrecta_Rechaza()
    {
        // Lunes a viernes (4 días) no es válido aunque empiece en lunes:
        // el fin debe ser sábado, así que este caso ya falla antes.
        // Lunes a sábado de otra semana (12 días) sí pasa día pero no duración.
        var lunes = ProximoLunes();
        var (valido, mensaje) = SemanaValidator.ValidarFechasSemana(lunes, lunes.AddDays(12));
        Assert.IsFalse(valido);
        Assert.AreEqual("La semana debe durar exactamente 6 días (lunes a sábado).", mensaje);
    }

    [TestMethod]
    public void ValidateCreate_FechasValidas_Acepta()
    {
        var lunes = ProximoLunes();
        var result = SemanaValidator.ValidateCreate(CrearDto(lunes, lunes.AddDays(5)));
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void ValidateCreate_NumeroSemanaInvalido_Rechaza()
    {
        var lunes = ProximoLunes();
        var result = SemanaValidator.ValidateCreate(new SemanaCreateDto(0, 2026, lunes, lunes.AddDays(5)));
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ValidateCreate_DomingoIncluido_Rechaza()
    {
        // Semana lunes-domingo (7 días): el fin no es sábado
        var lunes = ProximoLunes();
        var result = SemanaValidator.ValidateCreate(CrearDto(lunes, lunes.AddDays(6)));
        Assert.IsFalse(result.Success);
    }
}
