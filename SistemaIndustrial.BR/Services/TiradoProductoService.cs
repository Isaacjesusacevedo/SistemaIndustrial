using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Validators;

namespace SistemaIndustrial.BR.Services;

public class TiradoProductoService : ITiradoProductoService
{
    private readonly ITiradoProductoRepository _repository;
    private readonly ISemanaRepository _semanaRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public TiradoProductoService(
        ITiradoProductoRepository repository,
        ISemanaRepository semanaRepository,
        ICategoriaRepository categoriaRepository)
    {
        _repository = repository;
        _semanaRepository = semanaRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<ServiceResult<TiradoProductoResponse>> CreateAsync(TiradoProductoCreateDto dto)
    {
        var validation = TiradoProductoValidator.ValidateCreate(dto);
        if (!validation.Success)
            return ServiceResult<TiradoProductoResponse>.Fail(validation.Message!);

        if (!await _semanaRepository.ExistsAsync(dto.SemanaId))
            return ServiceResult<TiradoProductoResponse>.Fail($"La semana con Id {dto.SemanaId} no existe.");

        if (dto.CategoriaId.HasValue && !await _categoriaRepository.ExistsAsync(dto.CategoriaId.Value))
            return ServiceResult<TiradoProductoResponse>.Fail($"La categoría con Id {dto.CategoriaId.Value} no existe.");

        if (await _repository.ExistsByNombreSemanaAsync(dto.Nombre, dto.SemanaId))
            return ServiceResult<TiradoProductoResponse>.Fail($"Ya existe un producto '{dto.Nombre}' para esta semana.");

        if (dto.PromedioPorTacho <= 0)
            return ServiceResult<TiradoProductoResponse>.Fail("El promedio por tacho debe ser mayor a 0.");

        var entity = new TiradoProducto
        {
            SemanaId = dto.SemanaId,
            CategoriaId = dto.CategoriaId,
            Nombre = dto.Nombre,
            UnidadMedida = dto.UnidadMedida,
            PedidoSemanal = dto.PedidoSemanal,
            StockInicial = dto.StockInicial,
            PromedioPorTacho = dto.PromedioPorTacho,
            CantidadTirada = 0,
            Stock = 0,
            Observaciones = dto.Observaciones ?? string.Empty,
            FechaCreacion = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        var response = MapToResponse(created);
        return ServiceResult<TiradoProductoResponse>.Ok(response, "Producto tirado creado correctamente.");
    }

    public async Task<ServiceResult<TiradoProductoResponse>> UpdateAsync(int id, TiradoProductoUpdateDto dto)
    {
        var validation = TiradoProductoValidator.ValidateUpdate(dto);
        if (!validation.Success)
            return ServiceResult<TiradoProductoResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<TiradoProductoResponse>.Fail($"El producto tirado con Id {id} no existe.");

        if (dto.CategoriaId.HasValue && !await _categoriaRepository.ExistsAsync(dto.CategoriaId.Value))
            return ServiceResult<TiradoProductoResponse>.Fail($"La categoría con Id {dto.CategoriaId.Value} no existe.");

        if (await _repository.ExistsByNombreSemanaAsync(dto.Nombre, existing.SemanaId, id))
            return ServiceResult<TiradoProductoResponse>.Fail($"Ya existe un producto '{dto.Nombre}' para esta semana.");

        if (dto.PromedioPorTacho <= 0)
            return ServiceResult<TiradoProductoResponse>.Fail("El promedio por tacho debe ser mayor a 0.");

        existing.Nombre = dto.Nombre;
        existing.UnidadMedida = dto.UnidadMedida;
        existing.PedidoSemanal = dto.PedidoSemanal;
        existing.StockInicial = dto.StockInicial;
        existing.PromedioPorTacho = dto.PromedioPorTacho;
        existing.CategoriaId = dto.CategoriaId;
        existing.Observaciones = dto.Observaciones ?? string.Empty;
        existing.FechaActualizacion = DateTime.Now;

        // Edición manual del total tirado (ej: corrección o reseteo a 0).
        // Se registra una Tirada de ajuste con el delta para no perder historial.
        if (dto.CantidadTirada.HasValue && dto.CantidadTirada.Value != existing.CantidadTirada)
        {
            var delta = dto.CantidadTirada.Value - existing.CantidadTirada;
            existing.CantidadTirada = dto.CantidadTirada.Value;
            existing.Stock = existing.StockInicial + existing.CantidadTirada - existing.PedidoSemanal;

            await _repository.AddTiradaAsync(new Tirada
            {
                TiradoProductoId = id,
                Fecha = DateTime.Now,
                Cantidad = delta,
                Observaciones = "Ajuste manual de cantidad",
                FechaCreacion = DateTime.Now
            });
        }

        await _repository.UpdateAsync(id, existing);
        var response = MapToResponse(existing);
        return ServiceResult<TiradoProductoResponse>.Ok(response, "Producto tirado actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult.Fail($"El producto tirado con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Producto tirado eliminado correctamente.");
    }

    public async Task<ServiceResult<TiradoProductoResponse>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return ServiceResult<TiradoProductoResponse>.Fail($"El producto tirado con Id {id} no existe.");

        var response = MapToResponse(entity);
        return ServiceResult<TiradoProductoResponse>.Ok(response);
    }

    public async Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaIdAsync(int semanaId)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<TiradoProductoResponse>>.Fail($"La semana con Id {semanaId} no existe.");

        var entities = await _repository.GetBySemanaIdAsync(semanaId);
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<TiradoProductoResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<List<TiradoProductoResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<TiradoProductoResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<List<TiradoProductoResponse>>> GetBySemanaConFiltroAsync(int semanaId, string? filtro = null)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<TiradoProductoResponse>>.Fail($"La semana con Id {semanaId} no existe.");

        var entities = await _repository.GetBySemanaIdAsync(semanaId);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var filtroLower = filtro.ToLower();
            entities = entities.Where(e =>
                e.Nombre.ToLower().Contains(filtroLower) ||
                e.UnidadMedida.ToLower().Contains(filtroLower) ||
                (e.Observaciones?.ToLower().Contains(filtroLower) ?? false) ||
                (e.Categoria?.Nombre.ToLower().Contains(filtroLower) ?? false)
            ).ToList();
        }

        var responses = entities.Select(MapToResponse).ToList();
        return ServiceResult<List<TiradoProductoResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<TiradoProductoResponse>> RegistrarTiradaAsync(int id, RegistrarTiradaDto dto)
    {
        var validation = TiradoProductoValidator.ValidateRegistrarTirada(dto);
        if (!validation.Success)
            return ServiceResult<TiradoProductoResponse>.Fail(validation.Message!);

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<TiradoProductoResponse>.Fail($"El producto tirado con Id {id} no existe.");

        existing.CantidadTirada += dto.Cantidad;
        existing.Stock = existing.StockInicial + existing.CantidadTirada - existing.PedidoSemanal;
        existing.Observaciones = dto.Observaciones ?? existing.Observaciones;
        existing.FechaActualizacion = DateTime.Now;

        var tirada = new Tirada
        {
            TiradoProductoId = id,
            Fecha = dto.Fecha ?? DateTime.Now,
            Cantidad = dto.Cantidad,
            Observaciones = dto.Observaciones ?? string.Empty,
            FechaCreacion = DateTime.Now
        };

        await _repository.AddTiradaAsync(tirada);

        var response = MapToResponse(existing);
        return ServiceResult<TiradoProductoResponse>.Ok(response, "Tirada registrada correctamente.");
    }

    public async Task<ServiceResult<List<TiradaResponse>>> GetTiradasBySemanaAsync(int semanaId)
    {
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<List<TiradaResponse>>.Fail($"La semana con Id {semanaId} no existe.");

        var productos = await _repository.GetBySemanaIdAsync(semanaId);
        var tiradas = new List<TiradaResponse>();

        foreach (var producto in productos)
        {
            var tiradasProducto = await _repository.GetTiradasByProductoAsync(producto.Id);
            foreach (var tirada in tiradasProducto)
            {
                tiradas.Add(new TiradaResponse(
                    tirada.Id,
                    producto.Id,
                    producto.Nombre,
                    tirada.Fecha,
                    tirada.Cantidad,
                    tirada.Observaciones,
                    tirada.FechaCreacion
                ));
            }
        }

        tiradas = tiradas.OrderByDescending(t => t.Fecha).ToList();
        return ServiceResult<List<TiradaResponse>>.Ok(tiradas);
    }

    public async Task<ServiceResult<TiradoProductoBulkResultDto>> CreateBulkAsync(
        int semanaId,
        TiradoProductoBulkCreateDto dto)
    {
        // 1. Validar que la semana existe
        if (!await _semanaRepository.ExistsAsync(semanaId))
            return ServiceResult<TiradoProductoBulkResultDto>.Fail(
                $"La semana con Id {semanaId} no existe.");

        // 2. Validar que la lista no esté vacía
        if (dto.Items == null || dto.Items.Count == 0)
            return ServiceResult<TiradoProductoBulkResultDto>.Fail(
                "Debe incluir al menos un producto.");

        var creados = new List<TiradoProducto>();
        var errores = new List<string>();
        int ignorados = 0;

        // 3. Para cada item:
        foreach (var item in dto.Items)
        {
            try
            {
                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(item.Nombre))
                {
                    errores.Add("Producto sin nombre fue ignorado.");
                    ignorados++;
                    continue;
                }

                if (item.PromedioPorTacho <= 0)
                {
                    errores.Add($"'{item.Nombre}': promedio por tacho debe ser > 0.");
                    ignorados++;
                    continue;
                }

                if (item.PedidoSemanal < 0 || item.StockInicial < 0)
                {
                    errores.Add($"'{item.Nombre}': stock y pedido no pueden ser negativos.");
                    ignorados++;
                    continue;
                }

                if (item.CategoriaId.HasValue && !await _categoriaRepository.ExistsAsync(item.CategoriaId.Value))
                {
                    errores.Add($"'{item.Nombre}': la categoría no existe (ignorado).");
                    ignorados++;
                    continue;
                }

                // Verificar si ya existe en la semana
                if (await _repository.ExistsByNombreSemanaAsync(item.Nombre, semanaId))
                {
                    errores.Add($"'{item.Nombre}': ya existe en esta semana (ignorado).");
                    ignorados++;
                    continue;
                }

                var entity = new TiradoProducto
                {
                    SemanaId = semanaId,
                    CategoriaId = item.CategoriaId,
                    Nombre = item.Nombre,
                    UnidadMedida = item.UnidadMedida,
                    PedidoSemanal = item.PedidoSemanal,
                    StockInicial = item.StockInicial,
                    PromedioPorTacho = item.PromedioPorTacho,
                    CantidadTirada = 0,
                    Stock = item.StockInicial - item.PedidoSemanal,
                    Observaciones = item.Observaciones ?? string.Empty,
                    FechaCreacion = DateTime.Now
                };

                var created = await _repository.CreateAsync(entity);
                creados.Add(created);
            }
            catch (Exception ex)
            {
                errores.Add($"'{item.Nombre}': {ex.Message}");
                ignorados++;
            }
        }

        var result = new TiradoProductoBulkResultDto(
            Creados: creados.Count,
            Ignorados: ignorados,
            Errores: errores,
            ProductosCreados: creados.Select(MapToResponse).ToList()
        );

        var msg = creados.Count > 0
            ? $"{creados.Count} productos cargados correctamente. " +
              (ignorados > 0 ? $"{ignorados} ignorados." : "")
            : "No se pudo cargar ningún producto.";

        return ServiceResult<TiradoProductoBulkResultDto>.Ok(result, msg);
    }

    private static TiradoProductoResponse MapToResponse(TiradoProducto entity)
    {
        var stockTotal = entity.StockInicial + entity.CantidadTirada - entity.PedidoSemanal;
        var tachosEstimados = entity.PromedioPorTacho > 0 
            ? Math.Round((decimal)stockTotal / entity.PromedioPorTacho, 2) 
            : 0;

        string estado;
        if (stockTotal < 0)
            estado = "Falta";
        else if (stockTotal == 0)
            estado = "Listo";
        else
            estado = "Sobra";

        return new TiradoProductoResponse(
            entity.Id,
            entity.SemanaId,
            entity.CategoriaId,
            entity.Categoria?.Nombre,
            entity.Nombre,
            entity.UnidadMedida,
            entity.PedidoSemanal,
            entity.StockInicial,
            entity.PromedioPorTacho,
            entity.CantidadTirada,
            entity.Tiradas?.Count ?? 0,
            stockTotal,
            tachosEstimados,
            estado,
            entity.Observaciones,
            entity.FechaCreacion,
            entity.FechaActualizacion
        );
    }
}