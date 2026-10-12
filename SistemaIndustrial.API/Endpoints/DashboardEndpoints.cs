using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard"); // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        
        group.MapGet("/metricas/{semanaId:int}", async (
            int semanaId,
            IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetMetricasAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<DashboardMetricasDto> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetDashboardMetricas")
        .WithOpenApi();
        
        group.MapGet("/tendencias", async (
            IDashboardService dashboardService,
            [FromQuery] int semanasAtras = 12) =>
        {
            var result = await dashboardService.GetTendenciasAsync(semanasAtras);
            return result.Success
                ? Results.Ok(new ApiResponse<List<DashboardTendenciaDto>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetDashboardTendencias")
        .WithOpenApi();
        
        group.MapGet("/alertas/{semanaId:int}", async (
            int semanaId,
            IDashboardService dashboardService) =>
        {
            var result = await dashboardService.GetAlertasAsync(semanaId);
            return result.Success
                ? Results.Ok(new ApiResponse<List<DashboardAlertaDto>> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetDashboardAlertas")
        .WithOpenApi();
        
        return app;
    }
}