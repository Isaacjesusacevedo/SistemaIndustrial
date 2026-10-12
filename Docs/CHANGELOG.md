# Changelog

Historial de cambios importantes del proyecto.

---

## 2026-10-12 — Fase de deploy en producción

### Infraestructura
- Deploy del frontend en **Vercel** (Hobby plan)
- Deploy del backend en **Render** (Docker + Free tier)
- Base de datos en **MonsterASP.NET** (SQL Server 2022, Free tier)
- Configuración de CORS wildcard para previews de Vercel
- URL: https://sistema-industrial-teal.vercel.app

### Base de datos
- Creadas SPs con nombres correctos para Stock e Insumos
  (`sp_StockSemana_*`, `sp_InsumosSemana_*`)
- Creadas SPs del catálogo de productos (`sp_ProductosCatalogo_*`)
- Migradas las unidades de productos de Galón (4 L) a 600 (600 ml)
- Recalculados estados de semanas según progreso real
- Eliminado el módulo `RegistroTacho` (redundante con `Tirada`)

### Backend
- `Dockerfile` para deploy multi-stage
- `appsettings.Production.json` con connection string de MonsterASP
- CORS adaptado para aceptar cualquier subdominio `.vercel.app` del proyecto

### Frontend
- Variable `VITE_API_URL` configurada para apuntar a Render
- Semanas laborales cambiadas a lunes-sábado (6 días)
- Progreso semanal basado en cobertura real (no en productos cargados)
- Modal de Tirado simplificado: solo 2 campos editables (stock, cantidad tirada)
- Nuevo Producto en Tirado carga desde el catálogo
- Columna "Últ. día" con el último día con actividad
- Columna "Tachos" con tachos netos (siempre ≥ 0)
- Filtro "Sobrantes" más preciso (`StockTotal >= 0`)

---

## 2026-10-11 — Refactor de CSS

- Consolidación de CSS en `src/assets/styles/`
- Eliminación de duplicaciones (kpi-card, modal, data-table)
- Override de tema oscuro para componentes Element Plus

---

## 2026-10-11 — Migración inicial

- Entity Framework Core con SQL Server
- Stored Procedures para queries críticas
- Frontend en Vue 3 + TypeScript + Pinia
- Módulos: Semanas, Insumos, Stock, Tirado, Armado, Productos
