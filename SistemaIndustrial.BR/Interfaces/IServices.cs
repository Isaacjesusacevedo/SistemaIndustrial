using SistemaIndustrial.BR.DTOs;

namespace SistemaIndustrial.BR.Interfaces;

public interface ISemanaService
{
    Task<ServiceResult<SemanaResponse>> CreateAsync(SemanaCreateDto dto);
    Task<ServiceResult<SemanaResponse>> UpdateAsync(int id, SemanaUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<SemanaResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<SemanaResponse>>> GetAllAsync();
    Task<ServiceResult<List<SemanaResponse>>> GetByAnioAsync(int anio);
}

public interface IInsumoService
{
    Task<ServiceResult<InsumoResponse>> CreateAsync(InsumoCreateDto dto);
    Task<ServiceResult<InsumoResponse>> UpdateAsync(int id, InsumoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<InsumoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<InsumoResponse>>> GetBySemanaIdAsync(int semanaId);
}

public interface IStockService
{
    Task<ServiceResult<StockResponse>> CreateAsync(StockCreateDto dto);
    Task<ServiceResult<StockResponse>> UpdateAsync(int id, StockUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<StockResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<StockResponse>>> GetBySemanaIdAsync(int semanaId);
    Task<ServiceResult<List<StockResponse>>> SyncFromTiradoAsync(int semanaId);
}

public interface IDashboardService
{
    Task<ServiceResult<DashboardMetricasDto>> GetMetricasAsync(int semanaId);
    Task<ServiceResult<List<DashboardTendenciaDto>>> GetTendenciasAsync(int semanasAtras = 12);
    Task<ServiceResult<List<DashboardAlertaDto>>> GetAlertasAsync(int semanaId);
}

public interface ITiradoProductoService
{
    Task<ServiceResult<List<TiradoProductoResponse>>> GetAllAsync();
    Task<ServiceResult<TiradoProductoResponse>> CreateAsync(TiradoProductoCreateDto dto);
    Task<ServiceResult<TiradoProductoResponse>> UpdateAsync(int id, TiradoProductoUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<TiradoProductoResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaIdAsync(int semanaId);
    Task<ServiceResult<TiradoProductoResponse>> RegistrarTiradaAsync(int id, RegistrarTiradaDto dto);
    Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaConFiltroAsync(int semanaId, string? filtro = null);
    Task<ServiceResult<List<TiradaResponse>>> GetTiradasBySemanaAsync(int semanaId);
    Task<ServiceResult<TiradoProductoBulkResultDto>> CreateBulkAsync(int semanaId, TiradoProductoBulkCreateDto dto);
}
public interface ICategoriaService
{
    Task<ServiceResult<CategoriaResponse>> CreateAsync(CategoriaCreateDto dto);
    Task<ServiceResult<CategoriaResponse>> UpdateAsync(int id, CategoriaUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<CategoriaResponse>> GetByIdAsync(int id);
    Task<ServiceResult<List<CategoriaResponse>>> GetAllAsync();
}