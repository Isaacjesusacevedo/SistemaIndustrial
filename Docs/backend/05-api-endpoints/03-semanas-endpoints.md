# Semanas Endpoints

## Base Path: `/api/semanas`

**Modo demo**: `RequireAuthorization` comentado (acceso libre).

---

### GET /api/semanas

**Lista todas las semanas (o filtra por año)**

#### Query Parameters
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `anio` | int | - | Si se envía, retorna solo ese año (`GetByAnioAsync`, filtro en memoria) |
| `estado` | string | - | Declarado en la firma pero actualmente no filtra |

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "numeroSemana": 1,
      "anio": 2026,
      "fechaInicio": "2026-01-05T00:00:00",
      "fechaFin": "2026-01-10T00:00:00",
      "estado": "Completada",
      "fechaCreacion": "2026-01-01T10:00:00",
      "insumosCount": 5
    }
  ]
}
```

---

### GET /api/semanas/{id}

**Obtiene semana por ID**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "id": 1,
    "numeroSemana": 1,
    "anio": 2026,
    "fechaInicio": "2026-01-05T00:00:00",
    "fechaFin": "2026-01-10T00:00:00",
    "estado": "Completada",
    "fechaCreacion": "2026-01-01T10:00:00",
    "insumosCount": 5
  }
}
```

#### Response 404 Not Found
```json
{
  "success": false,
  "message": "Semana con Id 999 no fue encontrada.",
  "data": null
}
```

---

### POST /api/semanas

**Crea nueva semana**

#### Request
```json
{
  "numeroSemana": 3,
  "anio": 2026,
  "fechaInicio": "2026-01-19T00:00:00",
  "fechaFin": "2026-01-24T00:00:00"
}
```

#### Response 201 Created
```
Location: /api/semanas/3
{
  "success": true,
  "message": "Semana creada correctamente.",
  "data": {
    "id": 3,
    "numeroSemana": 3,
    "anio": 2026,
    "fechaInicio": "2026-01-19T00:00:00",
    "fechaFin": "2026-01-24T00:00:00",
    "estado": "Planificada",
    "fechaCreacion": "2026-01-15T10:00:00",
    "insumosCount": 0
  }
}
```

#### Validaciones
- `NumeroSemana`: 1-53
- `Anio`: 2020-2030
- `FechaInicio` debe ser lunes, `FechaFin` debe ser sábado, duración exacta 6 días (`SemanaValidator`)
- No duplicado (NumeroSemana + Anio)
- `Estado` inicial siempre `Planificada`

---

### PUT /api/semanas/{id}

**Actualiza semana**

#### Request
```json
{
  "numeroSemana": 3,
  "anio": 2026,
  "fechaInicio": "2026-01-19T00:00:00",
  "fechaFin": "2026-01-24T00:00:00",
  "estado": "EnProgreso"
}
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Semana actualizada correctamente.",
  "data": { ... }
}
```

#### Validaciones
- Mismo que POST
- `Estado`: Planificada | EnProgreso | Completada | Cancelada
- Id en ruta debe coincidir con body (si se envía)

---

### DELETE /api/semanas/{id}

**Elimina semana (si no tiene datos dependientes)**

#### Response 204 No Content

#### Response 400 Bad Request
```json
{
  "success": false,
  "message": "No se puede eliminar la semana porque tiene datos asociados (insumos, stock, tirado).",
  "data": null
}
```

#### Response 404 Not Found
```json
{
  "success": false,
  "message": "Semana con Id 999 no fue encontrada.",
  "data": null
}
```

---

## Otros recursos

Los demás recursos antes documentados en este archivo ahora tienen archivo propio:

- Insumos → `04-insumos-endpoints.md`
- Stock (+ `sync-from-tirado`) → `05-stock-endpoints.md`
- Tirado (+ `bulk`, `tirar`, `tachos`, `filtro`) → `06-tirado-endpoints.md`
- Categorías → `07-categorias-endpoints.md`
- Catálogo de productos → `08-productos-endpoints.md`
- Dashboard → `09-dashboard-endpoints.md`
- Health → `10-health-endpoints.md`

> No existe `/api/objetivos` (`ObjetivoSemana` fue eliminado del modelo). `RegistroTachoEndpoints.cs` (`/api/registrotacho`) existe en código pero no está mapeado en `Program.cs`; el flujo vigente de producción es TiradoProducto + Tirada.