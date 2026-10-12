using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.BR.Services;

public class DashboardService : IDashboardService
{
    private readonly ISemanaRepository _semanaRepository;
    private readonly IInsumoRepository _insumoRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ITiradoProductoRepository _tiradoProductoRepository;

    public DashboardService(
        ISemanaRepository semanaRepository,
        IInsumoRepository insumoRepository,
        IStockRepository stockRepository,
        ITiradoProductoRepository tiradoProductoRepository)
    {
        _semanaRepository = semanaRepository;
        _insumoRepository = insumoRepository;
        _stockRepository = stockRepository;
        _tiradoProductoRepository = tiradoProductoRepository;
    }

    public async Task<ServiceResult<DashboardMetricasDto>> GetMetricasAsync(int semanaId)
    {
        var semana = await _semanaRepository.GetByIdAsync(semanaId);
        if (semana == null)
            return ServiceResult<DashboardMetricasDto>.Fail($"La semana con Id {semanaId} no existe.");

        var productos = await _tiradoProductoRepository.GetBySemanaIdAsync(semanaId);
        var tiradas = new List<AD.Models.Tirada>();
        foreach (var p in productos)
            tiradas.AddRange(await _tiradoProductoRepository.GetTiradasByProductoAsync(p.Id));
        var insumos = await _insumoRepository.GetBySemanaIdAsync(semanaId);
        var stocks = await _stockRepository.GetBySemanaIdAsync(semanaId);

        var totalProducido = tiradas.Sum(t => t.Cantidad);
        var totalRechazado = 0m;
        var totalProcesado = totalProducido + totalRechazado;
        var porcentajeCalidad = totalProcesado > 0
            ? Math.Round((totalProducido / totalProcesado) * 100, 2)
            : 0;

        var insumosConsumidos = insumos.Sum(i => i.CantidadConsumida);
        var insumosRequeridos = insumos.Sum(i => i.CantidadRequerida);
        var stockActual = stocks.Sum(s => s.StockFinal);

        var turnosRegistrados = tiradas.Select(t => t.Fecha.Date).Distinct().Count();
        var operadoresUnicos = 0;

        var metricas = new DashboardMetricasDto(
            semanaId,
            semana.NumeroSemana,
            semana.Anio,
            totalProducido,
            totalRechazado,
            porcentajeCalidad,
            insumosConsumidos,
            insumosRequeridos,
            stockActual,
            turnosRegistrados,
            operadoresUnicos
        );

        return ServiceResult<DashboardMetricasDto>.Ok(metricas);
    }

    public async Task<ServiceResult<List<DashboardTendenciaDto>>> GetTendenciasAsync(int semanasAtras = 12)
    {
        var todasSemanas = await _semanaRepository.GetAllAsync();
        var semanasOrdenadas = todasSemanas
            .OrderByDescending(s => s.Anio)
            .ThenByDescending(s => s.NumeroSemana)
            .Take(semanasAtras)
            .OrderBy(s => s.Anio)
            .ThenBy(s => s.NumeroSemana)
            .ToList();

        var tendencias = new List<DashboardTendenciaDto>();

        foreach (var semana in semanasOrdenadas)
        {
            var productosSemana = await _tiradoProductoRepository.GetBySemanaIdAsync(semana.Id);
            var tiradasSemana = new List<AD.Models.Tirada>();
            foreach (var p in productosSemana)
                tiradasSemana.AddRange(await _tiradoProductoRepository.GetTiradasByProductoAsync(p.Id));

            var totalProducido = tiradasSemana.Sum(t => t.Cantidad);
            var totalRechazado = 0m;
            var totalProcesado = totalProducido + totalRechazado;
            var calidad = totalProcesado > 0
                ? Math.Round((totalProducido / totalProcesado) * 100, 2)
                : 0;

            var periodo = $"S{semana.NumeroSemana}-{semana.Anio}";
            tendencias.Add(new DashboardTendenciaDto(periodo, totalProducido, totalRechazado, calidad));
        }

        return ServiceResult<List<DashboardTendenciaDto>>.Ok(tendencias);
    }

    public async Task<ServiceResult<List<DashboardAlertaDto>>> GetAlertasAsync(int semanaId)
    {
        var semana = await _semanaRepository.GetByIdAsync(semanaId);
        if (semana == null)
            return ServiceResult<List<DashboardAlertaDto>>.Fail($"La semana con Id {semanaId} no existe.");

        var alertas = new List<DashboardAlertaDto>();

        var productosAlerta = await _tiradoProductoRepository.GetBySemanaIdAsync(semanaId);
        var tiradasAlerta = new List<AD.Models.Tirada>();
        foreach (var p in productosAlerta)
            tiradasAlerta.AddRange(await _tiradoProductoRepository.GetTiradasByProductoAsync(p.Id));

        if (!tiradasAlerta.Any())
        {
            alertas.Add(new DashboardAlertaDto(
                "Producción",
                "No hay tiradas registradas para esta semana",
                "Media",
                semanaId,
                "Se recomienda registrar al menos una tirada"
            ));
        }
        else
        {
            var totalProducido = tiradasAlerta.Sum(t => t.Cantidad);
            var totalRechazado = 0m;
            var totalProcesado = totalProducido + totalRechazado;
            var porcentajeCalidad = totalProcesado > 0
                ? Math.Round((totalProducido / totalProcesado) * 100, 2)
                : 0;

            if (porcentajeCalidad < 80)
            {
                alertas.Add(new DashboardAlertaDto(
                    "Calidad",
                    $"Porcentaje de calidad bajo: {porcentajeCalidad}%",
                    "Alta",
                    semanaId,
                    $"Rechazos: {totalRechazado} de {totalProcesado} unidades"
                ));
            }
        }

        var insumos = await _insumoRepository.GetBySemanaIdAsync(semanaId);
        var insumosCriticos = insumos.Where(i => i.StockDisponible < i.CantidadRequerida * 0.2m).ToList();
        if (insumosCriticos.Any())
        {
            foreach (var ins in insumosCriticos)
            {
                alertas.Add(new DashboardAlertaDto(
                    "Insumos",
                    $"Stock crítico de '{ins.Nombre}': {ins.StockDisponible} {ins.UnidadMedida}",
                    "Alta",
                    semanaId,
                    $"Requerido: {ins.CantidadRequerida}, Disponible: {ins.StockDisponible}"
                ));
            }
        }

        var stocks = await _stockRepository.GetBySemanaIdAsync(semanaId);
        var stocksNegativos = stocks.Where(s => s.StockFinal < 0).ToList();
        if (stocksNegativos.Any())
        {
            foreach (var st in stocksNegativos)
            {
                alertas.Add(new DashboardAlertaDto(
                    "Stock",
                    $"Stock negativo para '{st.Producto}': {st.StockFinal} {st.UnidadMedida}",
                    "Alta",
                    semanaId,
                    $"Inicial: {st.StockInicial}, Entradas: {st.Entradas}, Salidas: {st.Salidas}"
                ));
            }
        }

        return ServiceResult<List<DashboardAlertaDto>>.Ok(alertas);
    }
}