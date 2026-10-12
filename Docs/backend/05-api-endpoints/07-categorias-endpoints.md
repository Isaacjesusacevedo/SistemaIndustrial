# Categorías Endpoints

## Base Path: `/api/categorias`

**Grupo `MapCategoriaEndpoints`. Modo demo**: sin `RequireAuthorization` (acceso libre).

Entidad: `Categoria` (clasifica productos de Tirado; `CategoriaId` nullable con `SetNull`). Acceso a datos con EF Core directo. `GET /` retorna solo activas ordenadas por `Orden`.

---

### GET /api/categorias

**Lista categorías activas ordenadas por `Orden`**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    { "id": 1, "nombre": "Quesos", "orden": 1, "activa": true, "productosCount": 3 }
  ]
}
```

---

### GET /api/categorias/{id}

**Categoría por ID (404 si no existe)**

---

### POST /api/categorias

**Crea categoría**

#### Request
```json
{ "nombre": "Quesos", "orden": 1 }
```

#### Response 201 Created
```
Location: /api/categorias/{id}
```

#### Validaciones
- `Nombre` obligatorio (máx. 100), `Orden >= 0`

---

### PUT /api/categorias/{id}

**Actualiza categoría (incluye `activa`)**

#### Request
```json
{ "nombre": "Quesos", "orden": 1, "activa": true }
```

---

### DELETE /api/categorias/{id}

**Elimina categoría (204 No Content)**
