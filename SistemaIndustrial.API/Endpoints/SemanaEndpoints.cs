using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class SemanaEndpoints
{
    public static IEndpointRouteBuilder MapSemanaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/semanas"); // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        
        group.MapGet("/", async (
            ISemanaService semanaService,
            [FromQuery] int? anio,
            [FromQuery] string? estado) =>
        {
            var result = anio.HasValue 
                ? await semanaService.GetByAnioAsync(anio.Value)
                : await semanaService.GetAllAsync();
            
            return result.Success
                ? Results.Ok(new ApiResponse<List<SemanaResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetSemanas")
        .WithOpenApi();
        
        group.MapGet("/{id:int}", async (
            int id,
            ISemanaService semanaService) =>
        {
            var result = await semanaService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<SemanaResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetSemanaById")
        .WithOpenApi();
        
        group.MapPost("/", async (
            [FromBody] SemanaCreateRequest request,
            ISemanaService semanaService) =>
        {
            var dto = new SemanaCreateDto(request.NumeroSemana, request.Anio, request.FechaInicio, request.FechaFin);
            var result = await semanaService.CreateAsync(dto);
            
            return result.Success
                ? Results.Created($"/api/semanas/{result.Data!.Id}", new ApiResponse<SemanaResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("CreateSemana")
        .WithOpenApi();
        
        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] SemanaUpdateRequest request,
            ISemanaService semanaService) =>
        {
            var dto = new SemanaUpdateDto(request.NumeroSemana, request.Anio, request.FechaInicio, request.FechaFin, request.Estado);
            var result = await semanaService.UpdateAsync(id, dto);
            
            return result.Success
                ? Results.Ok(new ApiResponse<SemanaResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        // .RequireAuthorization("RequireSupervisor")
        .WithName("UpdateSemana")
        .WithOpenApi();
        
        group.MapDelete("/{id:int}", async (
            int id,
            ISemanaService semanaService) =>
        {
            var result = await semanaService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireAdmin") comentado para acceso libre
        // .RequireAuthorization("RequireAdmin")
        .WithName("DeleteSemana")
        .WithOpenApi();
        
        return app;
    }
}