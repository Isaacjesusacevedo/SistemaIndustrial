using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class InsumoEndpoints
{
    public static IEndpointRouteBuilder MapInsumoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/insumos"); // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        
        group.MapGet("/", async (
            IInsumoService insumoService,
            [FromQuery] int semanaId) =>
        {
            var result = await insumoService.GetBySemanaIdAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<InsumoResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetInsumosBySemana")
        .WithOpenApi();
        
        group.MapGet("/{id:int}", async (
            int id,
            IInsumoService insumoService) =>
        {
            var result = await insumoService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<InsumoResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetInsumoById")
        .WithOpenApi();
        
        group.MapPost("/", async (
            [FromBody] InsumoCreateRequest request,
            IInsumoService insumoService) =>
        {
            var dto = new InsumoCreateDto(request.SemanaId, request.Nombre, request.UnidadMedida, request.CantidadRequerida);
            var result = await insumoService.CreateAsync(dto);
            
            return result.Success
                ? Results.Created($"/api/insumos/{result.Data!.Id}", new ApiResponse<InsumoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("CreateInsumo")
        .WithOpenApi();
        
        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] InsumoUpdateRequest request,
            IInsumoService insumoService) =>
        {
            var dto = new InsumoUpdateDto(request.Nombre, request.UnidadMedida, request.CantidadRequerida, request.CantidadConsumida, request.StockDisponible);
            var result = await insumoService.UpdateAsync(id, dto);
            
            return result.Success
                ? Results.Ok(new ApiResponse<InsumoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("UpdateInsumo")
        .WithOpenApi();
        
        group.MapDelete("/{id:int}", async (
            int id,
            IInsumoService insumoService) =>
        {
            var result = await insumoService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireAdmin") comentado para acceso libre
        // .RequireAuthorization("RequireAdmin")
        .WithName("DeleteInsumo")
        .WithOpenApi();
        
        return app;
    }
}