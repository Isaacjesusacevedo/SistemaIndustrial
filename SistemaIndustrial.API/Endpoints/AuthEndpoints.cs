using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIndustrial.API.Models;
using BRDTOs = SistemaIndustrial.BR.DTOs;
using SistemaIndustrial.BR.Interfaces;

namespace SistemaIndustrial.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").AllowAnonymous();
        
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService) =>
        {
            var result = await authService.LoginAsync(new BRDTOs.LoginRequest(request.Email, request.Password));
            return result.Success
                ? Results.Ok(new ApiResponse<BRDTOs.LoginResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.Unauthorized();
        })
        .WithName("Login")
        .WithOpenApi();
        
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(new BRDTOs.RefreshTokenRequest(request.RefreshToken));
            return result.Success
                ? Results.Ok(new ApiResponse<BRDTOs.LoginResponse> { Success = true, Message = result.Message, Data = result.Data })
                : Results.Unauthorized();
        })
        .WithName("RefreshToken")
        .WithOpenApi();
        
        group.MapPost("/logout", async (
            IAuthService authService,
            HttpContext httpContext) =>
        {
            // Obtener userId del token
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                await authService.LogoutAsync(userId);
            }
            return Results.Ok(new ApiResponse<object> { Success = true, Message = "Sesión cerrada" });
        })
        // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        // .RequireAuthorization()
        .WithName("Logout")
        .WithOpenApi();
        
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IAuthService authService) =>
        {
            // Solo permitir en Development o con rol Admin
            return Results.BadRequest(new ApiResponse<object> { Success = false, Message = "Registro no disponible en esta versión" });
        })
        // MODO DEMO: .RequireAuthorization("RequireAdmin") comentado para acceso libre
        // .RequireAuthorization("RequireAdmin")
        .WithName("Register")
        .WithOpenApi();
        
        group.MapPut("/password", async (
            [FromBody] ChangePasswordRequest request,
            IAuthService authService,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();
            
            var result = await authService.ChangePasswordAsync(userId, new BRDTOs.ChangePasswordRequest(request.CurrentPassword, request.NewPassword));
            return result.Success
                ? Results.Ok(new ApiResponse<object> { Success = true, Message = result.Message })
                : Results.BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
        })
        // MODO DEMO: .RequireAuthorization() comentado para acceso libre
        // .RequireAuthorization()
        .WithName("ChangePassword")
        .WithOpenApi();
        
        return app;
    }
}