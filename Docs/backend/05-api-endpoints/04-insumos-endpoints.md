# Insumos Endpoints

## Base Path: `/api/insumos`

**Modo demo**: `RequireAuthorization` comentado (acceso libre).

---

### GET /api/insumos?semanaId={id}

**Lista insumos por semana (`semanaId` requerido como query string)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "semanaId": 1,
      "nombre": "Acero laminado",
      "unidadMedida": "kg",
      "cantidadRequerida": 5000.00,
      "cantidadConsumida": 4200.00,
      "stockDisponible": 800.00,
      "fechaCreacion": "2026-01-02T08:00:00"
    }
  ]
}
```

---

### GET /api/insumos/{id}

**Insumo por ID (404 si no existe)**

---

### POST /api/insumos

**Crea insumo**

#### Request
```json
{
  "semanaId": 1,
  "nombre": "Pintura epoxi",
  "unidadMedida": "lt",
  "cantidadRequerida": 200.00
}
```

#### Response 201 Created
```
Location: /api/insumos/{id}
```

#### Validaciones
- `SemanaId` debe existir
- `Nombre` obligatorio (máx. 100), `UnidadMedida` obligatoria (máx. 50)
- `CantidadRequerida >= 0`

---

### PUT /api/insumos/{id}

**Actualiza insumo (requerido, consumido, stock)**

#### Request
```json
{
  "nombre": "Pintura epoxi",
  "unidadMedida": "lt",
  "cantidadRequerida": 200.00,
  "cantidadConsumida": 150.00,
  "stockDisponible": 50.00
}
```

---

### DELETE /api/insumos/{id}

**Elimina insumo (204 No Content)**
