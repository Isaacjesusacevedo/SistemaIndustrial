using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class TiradoProductoEndpoints
{
    public static IEndpointRouteBuilder MapTiradoProductoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tirado");

        // GET /api/tirado/semana/{semanaId}
        group.MapGet("/semana/{semanaId:int}", async (
            int semanaId,
            ITiradoProductoService tiradoProductoService) =>
        {
            var result = await tiradoProductoService.GetBySemanaIdAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<TiradoProductoResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetTiradoProductosBySemana")
        .WithOpenApi();

        // GET /api/tirado/semana/{semanaId}/filtro
        group.MapGet("/semana/{semanaId:int}/filtro", async (
            int semanaId,
            ITiradoProductoService tiradoProductoService,
            [FromQuery] string? filtro) =>
        {
            var result = await tiradoProductoService.GetBySemanaConFiltroAsync(semanaId, filtro);
            return result.Success
                ? Results.Ok(new ApiResponse<List<TiradoProductoResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetTiradoProductosBySemanaConFiltro")
        .WithOpenApi();

        // GET /api/tirado/{id}
        group.MapGet("/{id:int}", async (
            int id,
            ITiradoProductoService tiradoProductoService) =>
        {
            var result = await tiradoProductoService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<TiradoProductoResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetTiradoProductoById")
        .WithOpenApi();

        // POST /api/tirado
        group.MapPost("/", async (
            [FromBody] TiradoProductoCreateRequest request,
            ITiradoProductoService tiradoProductoService) =>
        {
            var dto = new TiradoProductoCreateDto(
                request.SemanaId,
                request.Nombre,
                request.UnidadMedida,
                request.PedidoSemanal,
                request.StockInicial,
                request.PromedioPorTacho,
                request.CategoriaId,
                request.Observaciones
            );
            var result = await tiradoProductoService.CreateAsync(dto);

            return result.Success
                ? Results.Created($"/api/tirado/{result.Data!.Id}", new ApiResponse<TiradoProductoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("CreateTiradoProducto")
        .WithOpenApi();

        // PUT /api/tirado/{id}
        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] TiradoProductoUpdateRequest request,
            ITiradoProductoService tiradoProductoService) =>
        {
            var dto = new TiradoProductoUpdateDto(
                request.Nombre,
                request.UnidadMedida,
                request.PedidoSemanal,
                request.StockInicial,
                request.PromedioPorTacho,
                request.CategoriaId,
                request.Observaciones,
                request.CantidadTirada
            );
            var result = await tiradoProductoService.UpdateAsync(id, dto);

            return result.Success
                ? Results.Ok(new ApiResponse<TiradoProductoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("UpdateTiradoProducto")
        .WithOpenApi();

        // PATCH /api/tirado/{id}/tirar
        group.MapPatch("/{id:int}/tirar", async (
            int id,
            [FromBody] RegistrarTiradaRequest request,
            ITiradoProductoService tiradoProductoService) =>
        {
            var dto = new RegistrarTiradaDto(request.Cantidad, request.Fecha, request.Observaciones);
            var result = await tiradoProductoService.RegistrarTiradaAsync(id, dto);

            return result.Success
                ? Results.Ok(new ApiResponse<TiradoProductoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("RegistrarTirada")
        .WithOpenApi();

        // GET /api/tirado/semana/{semanaId}/tachos
        group.MapGet("/semana/{semanaId:int}/tachos", async (
            int semanaId,
            ITiradoProductoService tiradoProductoService) =>
        {
            var result = await tiradoProductoService.GetTiradasBySemanaAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<TiradaResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetTiradasBySemana")
        .WithOpenApi();

        // POST /api/tirado/semana/{semanaId}/bulk
        group.MapPost("/semana/{semanaId:int}/bulk", async (
            int semanaId,
            [FromBody] TiradoProductoBulkCreateDto dto,
            ITiradoProductoService tiradoProductoService) =>
        {
            var result = await tiradoProductoService.CreateBulkAsync(semanaId, dto);
            return result.Success
                ? Results.Ok(new ApiResponse<TiradoProductoBulkResultDto> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("CreateTiradoProductosBulk")
        .WithOpenApi();

        // DELETE /api/tirado/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            ITiradoProductoService tiradoProductoService) =>
        {
            var result = await tiradoProductoService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("DeleteTiradoProducto")
        .WithOpenApi();

        return app;
    }
}