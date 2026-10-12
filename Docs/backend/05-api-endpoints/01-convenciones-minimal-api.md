# Convenciones Minimal API

## Estándares REST

| Recurso | GET Collection | GET Single | POST | PUT | DELETE |
|---------|----------------|------------|------|-----|--------|
| Semanas | `/api/semanas` | `/api/semanas/{id}` | `/api/semanas` | `/api/semanas/{id}` | `/api/semanas/{id}` |
| Insumos | `/api/insumos?semanaId=` | `/api/insumos/{id}` | `/api/insumos` | `/api/insumos/{id}` | `/api/insumos/{id}` |
| Stock | `/api/stock?semanaId=` | `/api/stock/{id}` | `/api/stock` | `/api/stock/{id}` | `/api/stock/{id}` |
| Tirado | `/api/tirado/semana/{semanaId}` | `/api/tirado/{id}` | `/api/tirado` | `/api/tirado/{id}` | `/api/tirado/{id}` |
| Categorías | `/api/categorias` | `/api/categorias/{id}` | `/api/categorias` | `/api/categorias/{id}` | `/api/categorias/{id}` |
| Productos (catálogo) | `/api/productos` | `/api/productos/{id}` | `/api/productos` | `/api/productos/{id}` | `/api/productos/{id}` |
| Dashboard | `/api/dashboard/metricas/{semanaId}` | - | - | - | - |

### Subrutas especiales
- `POST /api/tirado/semana/{semanaId}/bulk` — creación en lote
- `PATCH /api/tirado/{id}/tirar` — registrar tirada (actualiza CantidadTirada/Stock)
- `GET /api/tirado/semana/{semanaId}/tachos` — tiradas de la semana
- `GET /api/tirado/semana/{semanaId}/filtro?filtro=` — búsqueda por nombre
- `POST /api/stock/sync-from-tirado/{semanaId}` — crea stock faltante desde Tirado
- `GET /api/dashboard/tendencias?semanasAtras=12`, `GET /api/dashboard/alertas/{semanaId}`
- `GET /health`, `GET /health/ready`, `GET /health/live` (públicos)

> **Nota:** `RegistroTachoEndpoints.cs` (`/api/registrotacho`) existe en código pero no está mapeado en `Program.cs`. No existe `/api/objetivos` (`ObjetivoSemana` fue eliminado del modelo).

### Convenciones de Nombrado
- **Rutas**: minúsculas, plural (`/api/categorias`, `/api/productos`); Tirado usa singular (`/api/tirado`)
- **Parámetros ruta**: `{id}` (int), `{semanaId}` (int)
- **Query strings**: camelCase (`?semanaId=1`, `?anio=2026`, `?filtro=`)
- **Body**: JSON, camelCase properties (serializador camelCase case-insensitive)

---

## Status Codes

| Código | Uso | Cuándo |
|--------|-----|--------|
| `200 OK` | GET exitoso, PUT exitoso | Respuesta con datos |
| `201 Created` | POST exitoso | Con header `Location: /api/recurso/{id}` |
| `204 No Content` | DELETE exitoso | Sin body de respuesta |
| `400 Bad Request` | Validación fallida | ModelState inválido o ServiceResult.Fail |
| `401 Unauthorized` | Sin autenticar | Token faltante, expirado, inválido |
| `403 Forbidden` | Sin permisos | Role insuficiente (futuro) |
| `404 Not Found` | Recurso no existe | EntityNotFoundException |
| `500 Internal Server Error` | Error interno | PersistenceException o Exception no controlada |

---

## Formato de Respuesta

### Éxito (200, 201)
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": { ... },
  "errors": null
}
```

### Error (400, 401, 404, 500)
```json
{
  "success": false,
  "message": "Descripción del error",
  "data": null,
  "errors": ["error1", "error2"]
}
```

### Ejemplos por Endpoint

**GET /api/semanas** (200)
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {"id":1,"numeroSemana":1,"anio":2026,"fechaInicio":"2026-01-05T00:00:00","fechaFin":"2026-01-10T00:00:00","estado":"Completada","fechaCreacion":"2026-01-01T10:00:00","insumosCount":5},
    {"id":2,"numeroSemana":2,"anio":2026,"fechaInicio":"2026-01-12T00:00:00","fechaFin":"2026-01-17T00:00:00","estado":"EnProgreso","fechaCreacion":"2026-01-08T10:00:00","insumosCount":4}
  ],
  "errors": null
}
```

**POST /api/semanas** (201)
```
Location: /api/semanas/3
{
  "success": true,
  "message": "Semana creada correctamente.",
  "data": {"id":3,"numeroSemana":3,"anio":2026,"fechaInicio":"2026-01-19T00:00:00","fechaFin":"2026-01-24T00:00:00","estado":"Planificada","fechaCreacion":"2026-01-15T10:00:00","insumosCount":0},
  "errors": null
}
```

**DELETE /api/semanas/1** (204)
```
No Content
```

**Error 400** (Validation)
```json
{
  "success": false,
  "message": "Errores de validación",
  "data": null,
  "errors": ["Número de semana debe ser entre 1 y 53", "Fecha fin debe ser posterior a fecha inicio"]
}
```

**Error 401**
```json
{
  "success": false,
  "message": "Token expirado o inválido",
  "data": null,
  "errors": null
}
```

**Error 404**
```json
{
  "success": false,
  "message": "Semana con Id 999 no fue encontrada.",
  "data": null,
  "errors": null
}
```

**Error 500**
```json
{
  "success": false,
  "message": "Ocurrió un error interno del servidor.",
  "data": null,
  "errors": null
}
```

---

## Versionado

**Estrategia**: URL path versioning
- Actual: `/api/v1/semanas` (implícito v1)
- Futuro: `/api/v2/semanas`

**Headers**: No se usa header `Accept-Version`.

---

## Filtros, Paginación, Ordenamiento (Futuro)

### Query Parameters Estándar
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | int | 1 | Número de página (1-based) |
| `pageSize` | int | 20 | Elementos por página (max 100) |
| `sortBy` | string | `id` | Campo para ordenar |
| `sortDir` | string | `asc` | `asc` o `desc` |
| `filter.*` | string | - | Filtros dinámicos |

### Ejemplo
```
GET /api/semanas?page=2&pageSize=10&sortBy=fechaInicio&sortDir=desc&filter.anio=2026
```

### Response Paginado
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "items": [...],
    "totalCount": 52,
    "page": 2,
    "pageSize": 10,
    "totalPages": 6
  }
}
```

---

## Seguridad

### Autenticación
- **JWT Bearer configurado** (validación de issuer/audience/lifetime/firma, `ClockSkew` 1 min, header `Token-Expired`).
- **Modo demo actual**: `RequireAuthorization` comentado en todos los grupos (acceso libre); el grupo `/api/auth` es `AllowAnonymous`. Endpoints públicos reales: `/api/auth/*`, `/health`, `/health/ready`, `/health/live`.

### Autorización (Futuro)
- **Roles**: `Admin`, `Supervisor`, `Operador`
- **Policies**: `RequireAdmin`, `RequireSupervisor`, `RequireOperador`

### CORS
```csharp
// Política "VueCors" (Program.cs): orígenes desde Cors:AllowedOrigins
// con fallback a http://localhost:5173, :5174 y :4173
policy.WithOrigins(origins)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

---

## Rate Limiting (Futuro)
- **Login**: 5 req/min por IP
- **API General**: 100 req/min por usuario
- **Dashboard**: 30 req/min por usuario

---

## Logging y Observabilidad

### Structured Logging (Serilog - Futuro)
```csharp
Log.Information("Semana creada: {SemanaId} - {NumeroSemana}/{Anio}", semana.Id, semana.NumeroSemana, semana.Anio);
Log.Warning("Intento login fallido: {Email}", email);
Log.Error(ex, "Error al crear semana: {Message}", ex.Message);
```

### Health Checks (públicos, mapeados inline en Program.cs)
```csharp
// GET /health        → { status: "Healthy", timestamp }
// GET /health/ready  → { status: "Ready", database: "Connected" }
// GET /health/live   → { status: "Alive" }
```

---

## OpenAPI / Scalar
```csharp
// Program.cs — OpenAPI nativo de .NET 9 (lo consume Scalar), solo Development
builder.Services.AddOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sistema Industrial API")
               .WithTheme(ScalarTheme.BluePlanet);
    });
}
```