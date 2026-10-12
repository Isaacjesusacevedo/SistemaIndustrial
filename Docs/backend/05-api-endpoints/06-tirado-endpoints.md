# Tirado Endpoints (módulo Tirado / Armado)

## Base Path: `/api/tirado`

**Grupo `MapTiradoProductoEndpoints`. Modo demo**: sin `RequireAuthorization` (acceso libre).

Entidad: `TiradoProducto` (producto pedido para una semana) + `Tirada` (cada tacho registrado). Acceso a datos con EF Core directo.

---

### GET /api/tirado/semana/{semanaId}

**Lista productos de Tirado de la semana**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "semanaId": 1,
      "categoriaId": 2,
      "categoriaNombre": "Quesos",
      "nombre": "Queso cremoso",
      "unidadMedida": "kg",
      "pedidoSemanal": 500.00,
      "stockInicial": 50.00,
      "promedioPorTacho": 25.00,
      "cantidadTirada": 100.00,
      "cantidadTachos": 4,
      "stockTotal": 150.00,
      "tachosEstimados": 20.00,
      "estado": "EnProceso",
      "observaciones": null,
      "fechaCreacion": "2026-01-05T08:00:00",
      "fechaActualizacion": null
    }
  ]
}
```

---

### GET /api/tirado/semana/{semanaId}/filtro?filtro={texto}

**Búsqueda por nombre dentro de la semana (case-insensitive, `filtro` opcional)**

---

### GET /api/tirado/{id}

**Producto por ID (404 si no existe)**

---

### POST /api/tirado

**Crea producto de Tirado**

#### Request
```json
{
  "semanaId": 1,
  "nombre": "Queso cremoso",
  "unidadMedida": "kg",
  "pedidoSemanal": 500.00,
  "stockInicial": 50.00,
  "promedioPorTacho": 25.00,
  "categoriaId": 2,
  "observaciones": null
}
```

#### Response 201 Created
```
Location: /api/tirado/{id}
```

#### Validaciones
- `SemanaId` debe existir; `CategoriaId` (opcional) debe existir
- `Nombre` único por semana (índice `SemanaId + Nombre`)
- `PedidoSemanal > 0`, `PromedioPorTacho > 0`, `StockInicial >= 0`
- `Observaciones` máx. 500

---

### PUT /api/tirado/{id}

**Actualiza producto (incluye `cantidadTirada` opcional, actualiza `FechaActualizacion`)**

#### Request
```json
{
  "nombre": "Queso cremoso",
  "unidadMedida": "kg",
  "pedidoSemanal": 500.00,
  "stockInicial": 50.00,
  "promedioPorTacho": 25.00,
  "categoriaId": 2,
  "observaciones": null,
  "cantidadTirada": 100.00
}
```

---

### PATCH /api/tirado/{id}/tirar

**Registra una tirada (tacho) y recalcula `CantidadTirada`/`Stock` del producto**

#### Request
```json
{
  "cantidad": 25.00,
  "fecha": "2026-01-06T10:00:00",
  "observaciones": "Turno mañana"
}
```

- `Fecha` opcional (default: ahora). `Cantidad` no negativa.

---

### GET /api/tirado/semana/{semanaId}/tachos

**Lista las tiradas de la semana (todas las de sus productos, orden descendente por fecha)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "tiradoProductoId": 1,
      "productoNombre": "Queso cremoso",
      "fecha": "2026-01-06T10:00:00",
      "cantidad": 25.00,
      "observaciones": null,
      "fechaCreacion": "2026-01-06T10:05:00"
    }
  ]
}
```

---

### POST /api/tirado/semana/{semanaId}/bulk

**Creación en lote (los items inválidos/duplicados se ignoran, no falla todo)**

#### Request
```json
{
  "items": [
    {
      "nombre": "Queso cremoso",
      "unidadMedida": "kg",
      "promedioPorTacho": 25.00,
      "stockInicial": 50.00,
      "pedidoSemanal": 500.00,
      "categoriaId": 2,
      "observaciones": null
    }
  ]
}
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Bulk completado: 1 creados, 0 ignorados.",
  "data": {
    "creados": 1,
    "ignorados": 0,
    "errores": [],
    "productosCreados": [ ... ]
  }
}
```

#### Reglas por item (`TiradoProductoService.CreateBulkAsync`)
- Lista no vacía; la semana debe existir
- Nombre obligatorio, `PromedioPorTacho > 0`, stock/pedido no negativos
- Categoría (si se envía) debe existir; nombre no duplicado en la semana

---

### DELETE /api/tirado/{id}

**Elimina producto (204 No Content; las Tiradas caen en cascada por FK)**
