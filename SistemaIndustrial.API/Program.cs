using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SistemaIndustrial.AD;
using SistemaIndustrial.AD.Data;
using SistemaIndustrial.AD.Interfaces;
using SistemaIndustrial.AD.Repositories;
using SistemaIndustrial.BR.Interfaces;
using SistemaIndustrial.BR.Services;
using SistemaIndustrial.API.Endpoints;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[]
            {
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:4173"
            };
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JSON Options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// EF Core
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories (AD Layer)
builder.Services.AddScoped<ISemanaRepository, SemanaRepository>();
builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITiradoProductoRepository, TiradoProductoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoCatalogoRepository, ProductoCatalogoRepository>();

// Services (BR Layer)
builder.Services.AddScoped<ISemanaService, SemanaService>();
builder.Services.AddScoped<IInsumoService, InsumoService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITiradoProductoService, TiradoProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoCatalogoService, ProductoCatalogoService>();

// SpInvoker
builder.Services.AddScoped<SpInvoker>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    context.Response.Headers.Append("Token-Expired", "true");
                return Task.CompletedTask;
            }
        };
    });

// MODO DEMO: políticas de autorización deshabilitadas
builder.Services.AddAuthorization();

// OpenAPI nativo de .NET 9 (lo consume Scalar)
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware global de errores
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Error no manejado en {Path}", context.Request.Path);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new { success = false, message = "Ocurrió un error interno del servidor.", data = (object?)null, errors = (string[]?)null };
        await context.Response.WriteAsJsonAsync(response);
    });
});

// Scalar UI (reemplaza a Swagger UI)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sistema Industrial API")
               .WithTheme(ScalarTheme.BluePlanet);
    });
}

app.UseRouting();
app.UseCors("VueCors");
app.UseAuthentication();
app.UseAuthorization();

// Health checks (públicos)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow })).AllowAnonymous();
app.MapGet("/health/ready", () => Results.Ok(new { status = "Ready", database = "Connected" })).AllowAnonymous();
app.MapGet("/health/live", () => Results.Ok(new { status = "Alive" })).AllowAnonymous();

// Endpoints
app.MapAuthEndpoints();
app.MapSemanaEndpoints();
app.MapInsumoEndpoints();
app.MapStockEndpoints();
app.MapDashboardEndpoints();
app.MapTiradoProductoEndpoints();
app.MapCategoriaEndpoints();
app.MapProductoCatalogoEndpoints();

app.Run();