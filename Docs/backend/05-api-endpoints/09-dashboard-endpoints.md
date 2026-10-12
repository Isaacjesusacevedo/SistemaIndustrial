# Dashboard Endpoints

## Base Path: `/api/dashboard`

**Modo demo**: `RequireAuthorization` comentado (acceso libre).

Fuente de producción: **TiradoProducto + Tiradas** (ya no hay objetivos ni registro de tachos por turnos). `TotalRechazado` y `OperadoresUnicos` están reservados en 0; `TurnosRegistrados` cuenta días distintos con tiradas.

---

### GET /api/dashboard/metricas/{semanaId}

**KPIs de una semana (404 si la semana no existe)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "semanaId": 1,
    "numeroSemana": 1,
    "anio": 2026,
    "totalProducido": 1250.00,
    "totalRechazado": 0.00,
    "porcentajeCalidad": 100.00,
    "insumosConsumidos": 4200.00,
    "insumosRequeridos": 5000.00,
    "stockActual": 1200.00,
    "turnosRegistrados": 3,
    "operadoresUnicos": 0
  }
}
```

---

### GET /api/dashboard/tendencias?semanasAtras=12

**Tendencias de las últimas N semanas (`Periodo: "AAAA-WNN"`)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "periodo": "2026-W01",
      "produccion": 1250.00,
      "rechazos": 0.00,
      "calidad": 100.00
    }
  ]
}
```

---

### GET /api/dashboard/alertas/{semanaId}

**Alertas automáticas desde las métricas (ej. `StockBajo` / `Critical`, `SinRegistros` / `Info`)**

#### Response 200 OK
```json
{
  "success": true,
  "message": "OK",
  "data": [
    {
      "tipo": "SinRegistros",
      "mensaje": "No hay registros de producción esta semana",
      "severidad": "Info",
      "semanaId": 1,
      "detalle": "Registrar tiradas en el módulo Tirado"
    }
  ]
}
```
