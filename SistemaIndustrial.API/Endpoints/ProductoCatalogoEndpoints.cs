using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class ProductoCatalogoEndpoints
{
    public static IEndpointRouteBuilder MapProductoCatalogoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/productos"); // MODO DEMO: .RequireAuthorization() comentado para acceso libre

        group.MapGet("/", async (
            IProductoCatalogoService productoService) =>
        {
            var result = await productoService.GetAllAsync();
            return result.Success
                ? Results.Ok(new ApiResponse<List<ProductoCatalogoResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetProductosCatalogo")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (
            int id,
            IProductoCatalogoService productoService) =>
        {
            var result = await productoService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<ProductoCatalogoResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetProductoCatalogoById")
        .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] ProductoCatalogoCreateRequest request,
            IProductoCatalogoService productoService) =>
        {
            var dto = new ProductoCatalogoCreateDto(request.Nombre, request.UnidadMedida, request.PedidoSemanalAproximado, request.PromedioPorTacho, request.Observaciones);
            var result = await productoService.CreateAsync(dto);

            return result.Success
                ? Results.Created($"/api/productos/{result.Data!.Id}", new ApiResponse<ProductoCatalogoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        .WithName("CreateProductoCatalogo")
        .WithOpenApi();

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] ProductoCatalogoUpdateRequest request,
            IProductoCatalogoService productoService) =>
        {
            var dto = new ProductoCatalogoUpdateDto(request.Nombre, request.UnidadMedida, request.PedidoSemanalAproximado, request.PromedioPorTacho, request.Activo, request.Observaciones);
            var result = await productoService.UpdateAsync(id, dto);

            return result.Success
                ? Results.Ok(new ApiResponse<ProductoCatalogoResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireSupervisor") comentado para acceso libre
        .WithName("UpdateProductoCatalogo")
        .WithOpenApi();

        group.MapDelete("/{id:int}", async (
            int id,
            IProductoCatalogoService productoService) =>
        {
            var result = await productoService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization("RequireAdmin") comentado para acceso libre
        .WithName("DeleteProductoCatalogo")
        .WithOpenApi();

        return app;
    }
}
