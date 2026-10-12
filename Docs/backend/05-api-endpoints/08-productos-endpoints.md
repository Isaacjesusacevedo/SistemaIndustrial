# Productos Endpoints (catálogo general)

## Base Path: `/api/productos`

**Grupo `MapProductoCatalogoEndpoints`. Modo demo**: `RequireAuthorization` comentado (acceso libre).

Entidad: `ProductoCatalogo` — catálogo general de productos, independiente de la semana (sin `SemanaId`), con `Nombre` único. Acceso a datos vía Stored Procedures (`sp_ProductosCatalogo_*` con `SpInvoker`).

---

### GET /api/productos

**Lista todo el catálogo**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "nombre": "Queso cremoso",
      "unidadMedida": "kg",
      "pedidoSemanalAproximado": 500.00,
      "promedioPorTacho": 25.00,
      "activo": true,
      "observaciones": null,
      "fechaCreacion": "2026-01-01T10:00:00"
    }
  ]
}
```

---

### GET /api/productos/{id}

**Producto por ID (404 si no existe)**

---

### POST /api/productos

**Crea producto del catálogo**

#### Request
```json
{
  "nombre": "Queso cremoso",
  "unidadMedida": "kg",
  "pedidoSemanalAproximado": 500.00,
  "promedioPorTacho": 25.00,
  "observaciones": null
}
```

#### Response 201 Created
```
Location: /api/productos/{id}
```

#### Validaciones (`ProductoCatalogoValidator`)
- `Nombre` obligatorio (máx. 200) y único
- `UnidadMedida` obligatoria (máx. 50)
- `PedidoSemanalAproximado >= 0`, `PromedioPorTacho >= 0`
- `Observaciones` máx. 500

---

### PUT /api/productos/{id}

**Actualiza producto (incluye `activo`)**

#### Request
```json
{
  "nombre": "Queso cremoso",
  "unidadMedida": "kg",
  "pedidoSemanalAproximado": 500.00,
  "promedioPorTacho": 25.00,
  "activo": true,
  "observaciones": null
}
```

---

### DELETE /api/productos/{id}

**Elimina producto (204 No Content)**
