using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class CategoriaEndpoints
{
    public static IEndpointRouteBuilder MapCategoriaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categorias");

        // GET /api/categorias
        group.MapGet("/", async (
            ICategoriaService categoriaService) =>
        {
            var result = await categoriaService.GetAllAsync();
            return result.Success
                ? Results.Ok(new ApiResponse<List<CategoriaResponse>> { Success = true, Message = "OK", Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetCategorias")
        .WithOpenApi();

        // GET /api/categorias/{id}
        group.MapGet("/{id:int}", async (
            int id,
            ICategoriaService categoriaService) =>
        {
            var result = await categoriaService.GetByIdAsync(id);
            return result.Success
                ? Results.Ok(new ApiResponse<CategoriaResponse> { Success = true, Message = "OK", Data = result.Data })
                : Results.NotFound(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("GetCategoriaById")
        .WithOpenApi();

        // POST /api/categorias
        group.MapPost("/", async (
            [FromBody] CategoriaCreateRequest request,
            ICategoriaService categoriaService) =>
        {
            var dto = new CategoriaCreateDto(request.Nombre, request.Orden);
            var result = await categoriaService.CreateAsync(dto);

            return result.Success
                ? Results.Created($"/api/categorias/{result.Data!.Id}", new ApiResponse<CategoriaResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("CreateCategoria")
        .WithOpenApi();

        // PUT /api/categorias/{id}
        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] CategoriaUpdateRequest request,
            ICategoriaService categoriaService) =>
        {
            var dto = new CategoriaUpdateDto(request.Nombre, request.Orden, request.Activa);
            var result = await categoriaService.UpdateAsync(id, dto);

            return result.Success
                ? Results.Ok(new ApiResponse<CategoriaResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("UpdateCategoria")
        .WithOpenApi();

        // DELETE /api/categorias/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            ICategoriaService categoriaService) =>
        {
            var result = await categoriaService.DeleteAsync(id);
            return result.Success
                ? Results.NoContent()
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        .WithName("DeleteCategoria")
        .WithOpenApi();

        return app;
    }
}