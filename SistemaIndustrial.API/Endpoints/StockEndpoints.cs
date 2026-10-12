using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stock"); // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        
        group.MapGet("/", async (
            IStockService stockService,
            [FromQuery] int semanaId) =>
        {
            var result = await stockService.GetBySemanaIdAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<StockResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetStockBySemana")
        .WithOpenApi();
        
        group.MapGet("/{id:int}", async (
            int id,
            IStockService stockService) =>
        {
            var result = await stockService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<StockResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetStockById")
        .WithOpenApi();
        
        group.MapPost("/", async (
            [FromBody] StockCreateRequest request,
            IStockService stockService) =>
        {
            var dto = new StockCreateDto(request.SemanaId, request.Producto, request.UnidadMedida, request.StockInicial);
            var result = await stockService.CreateAsync(dto);
            
            return result.Success
                ? Results.Created($"/api/stock/{result.Data!.Id}", new ApiResponse<StockResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("CreateStock")
        .WithOpenApi();
        
        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] StockUpdateRequest request,
            IStockService stockService) =>
        {
            var dto = new StockUpdateDto(request.Producto, request.UnidadMedida, request.StockInicial, request.Entradas, request.Salidas);
            var result = await stockService.UpdateAsync(id, dto);
            
            return result.Success
                ? Results.Ok(new ApiResponse<StockResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("UpdateStock")
        .WithOpenApi();
        
        group.MapDelete("/{id:int}", async (
            int id,
            IStockService stockService) =>
        {
            var result = await stockService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireAdmin") comentado para acceso libre
        // .RequireAuthorization("RequireAdmin")
        .WithName("DeleteStock")
        .WithOpenApi();

        group.MapPost("/sync-from-tirado/{semanaId:int}", async (
            int semanaId,
            IStockService stockService) =>
        {
            var result = await stockService.SyncFromTiradoAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<StockResponse>> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("SyncStockFromTirado")
        .WithOpenApi();
        
        return app;
    }
}