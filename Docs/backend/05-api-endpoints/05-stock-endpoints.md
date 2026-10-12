# Stock Endpoints

## Base Path: `/api/stock`

**Modo demo**: `RequireAuthorization` comentado (acceso libre).

---

### GET /api/stock?semanaId={id}

**Lista stock por semana (`semanaId` requerido como query string)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "id": 1,
      "semanaId": 1,
      "producto": "Viga H-200",
      "unidadMedida": "m",
      "stockInicial": 1000.00,
      "entradas": 500.00,
      "salidas": 300.00,
      "stockFinal": 1200.00,
      "fechaCreacion": "2026-01-02T08:00:00"
    }
  ]
}
```

---

### GET /api/stock/{id}

**Stock por ID (404 si no existe)**

---

### POST /api/stock

**Crea registro de stock**

#### Request
```json
{
  "semanaId": 1,
  "producto": "Perfil UPN-100",
  "unidadMedida": "m",
  "stockInicial": 500.00
}
```

#### Response 201 Created
```
Location: /api/stock/{id}
```

#### Validaciones
- `SemanaId` debe existir
- `Producto` obligatorio (máx. 100), `UnidadMedida` obligatoria (máx. 50)
- `StockInicial >= 0`

---

### PUT /api/stock/{id}

**Actualiza stock (recalcula StockFinal)**

#### Request
```json
{
  "producto": "Perfil UPN-100",
  "unidadMedida": "m",
  "stockInicial": 500.00,
  "entradas": 200.00,
  "salidas": 150.00
}
```

#### Lógica
- `StockFinal = StockInicial + Entradas - Salidas` (calculado en `StockService`)
- `Entradas`, `Salidas >= 0`

---

### DELETE /api/stock/{id}

**Elimina registro (204 No Content)**

---

### POST /api/stock/sync-from-tirado/{semanaId}

**Crea el stock faltante desde los productos de Tirado de la semana**

#### Lógica (`StockService.SyncFromTiradoAsync`)
1. La semana debe existir.
2. Lee los productos de Tirado (`ITiradoProductoService.GetBySemanaIdAsync`).
3. Crea `StockSemana` solo para nombres que aún no existen en stock (comparación case-insensitive), con `StockInicial` del Tirado y `Entradas`/`Salidas` en 0.
4. Retorna el stock final de la semana.

#### Response 200 OK
```json
{
  "success": true,
  "message": "2 productos sincronizados desde Tirado.",
  "data": [ ... ]
}
```

Si ya estaba todo sincronizado: `"Todos los productos ya estaban sincronizados."`.
