# Deuda Técnica — Sistema Industrial

> Registro de deuda técnica, bugs conocidos, limitaciones y plan de acción para el proyecto Sistema Industrial.

---

## 🐛 Bugs Críticos

| ID | Severidad | Descripción | Ubicación | Estado |
|----|-----------|-------------|-----------|--------|
| SI-001 | 🔴 Crítico | Autenticación JWT no implementada completamente | `AuthService.cs`, `AuthEndpoints.cs` | 🔄 En progreso |
| SI-002 | 🟢 Resuelto | Módulos Objetivos y Registro Tacho eliminados (flujo cubierto por Tirado); vistas de Insumos y Stock existen | `Frontend/src/views/` | ✅ Hecho |
| SI-003 | 🟡 Parcial | 34 unitarios en verde (BR 24, AD 6, API 4); faltan integración | `*.Test/` projects | 🔄 En progreso |

---

## ⚠️ Limitaciones y Bugs Conocidos

### Backend

| ID | Severidad | Descripción | Archivo/Ubicación |
|----|-----------|-------------|-------------------|
| SI-B001 | 🟡 Media | `AuthController` accede directo a `AppDbContext` (fuera de patrón servicios) | `SistemaIndustrial.API/Endpoints/AuthEndpoints.cs` |
| SI-B002 | 🟡 Media | Sin validación de duplicados en `SemanaCreateDto` a nivel API (solo en Service) | `SemanaEndpoints.cs` |
| SI-B003 | 🟡 Media | `RefreshToken` rotation no implementada completamente | `AuthService.cs` |
| SI-B004 | 🟡 Media | Sin rate limiting en endpoints de autenticación | `Program.cs` |
| SI-B005 | 🟢 Baja | Falta paginación en `GetAll` endpoints | `*Endpoints.cs` |
| SI-B006 | 🟢 Baja | Sin logging estructurado (Serilog) | `Program.cs` |
| SI-B007 | 🟢 Baja | Sin auditoría de cambios (`CreatedBy`, `UpdatedBy`) | Entidades AD |

### Frontend

| ID | Severidad | Descripción | Archivo/Ubicación |
|----|-----------|-------------|-------------------|
| SI-F001 | 🟢 Resuelto | Vistas CRUD existen (semanas, productos, insumos, stock, tirado, armado); Objetivos/RegistroTacho eliminados | `Frontend/src/views/` |
| SI-F002 | 🔴 Crítico | Dashboard sin gráficos interactivos (Chart.js/ECharts) | `DashboardView.vue` |
| SI-F003 | 🟡 Media | Formularios sin validación robusta (VeeValidate + Zod) | `*View.vue` |
| SI-F004 | 🟡 Media | Sin tests E2E (Cypress/Playwright) | `Frontend/` |
| SI-F005 | 🟡 Media | Auto-refresh token interceptor incompleto | `useApiFetch.ts` |
| SI-F006 | 🟢 Baja | Sin internacionalización (i18n) | `Frontend/` |
| SI-F007 | 🟢 Baja | Sin Storybook para component library | `Frontend/` |

### Arquitectura / Infraestructura

| ID | Severidad | Descripción |
|----|-----------|-------------|
| SI-A001 | 🟡 Media | Sin health checks detallados (`/health/ready`, `/health/live`) |
| SI-A002 | 🟡 Media | Sin métricas / observabilidad (Prometheus, OpenTelemetry) |
| SI-A003 | 🟢 Baja | CI/CD pipeline no configurado |
| SI-A004 | 🟢 Baja | Sin contrato de API versionado (`/api/v1/`) |

---

## 📋 Plan de Acción (Roadmap)

### Sprint 1 — Fundación Crítica (Semana 1-2)
- [ ] **SI-001** Implementar JWT Auth completa (Login, Refresh, Logout, ChangePassword)
- [x] **SI-002** Módulos Objetivos/RegistroTacho eliminados; vistas de Insumos y Stock existentes
- [ ] **SI-003** Configurar proyectos de test (`*.Test/`) con MSTest + Moq

### Sprint 2 — Funcionalidad Core (Semana 3-4)
- [ ] Completar vistas CRUD con DataTable, FormField, Modal
- [ ] Implementar Dashboard con Chart.js/ECharts (KPIs, tendencias, alertas)
- [ ] Agregar validación de formularios con VeeValidate + Zod
- [ ] Implementar auto-refresh token robusto en `useApiFetch.ts`

### Sprint 3 — Calidad y Robustez (Semana 5-6)
- [ ] Tests unitarios: Repositories, Services, AuthService
- [ ] Tests integración: Endpoints API con `WebApplicationFactory`
- [ ] Tests E2E: Flujos críticos (Login, CRUD Semanas, Dashboard)
- [ ] Rate limiting en Auth endpoints
- [ ] Paginación en GetAll endpoints

### Sprint 4 — Observabilidad y Producción (Semana 7-8)
- [ ] Health checks completos (`/health`, `/health/ready`, `/health/live`)
- [ ] Logging estructurado con Serilog (Console, File, Seq)
- [ ] Métricas Prometheus (`/metrics`)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Docker multi-stage optimizado
- [ ] Documentación OpenAPI/Swagger completa

---

## 📊 Métricas de Deuda Técnica

| Métrica | Actual | Objetivo |
|---------|--------|----------|
| **Cobertura Tests Backend** | 0% | ≥ 80% |
| **Cobertura Tests Frontend** | 0% | ≥ 70% |
| **Bugs Críticos Abiertos** | 3 | 0 |
| **Bugs Medios Abiertos** | 11 | ≤ 3 |
| **Deuda Técnica (horas estimadas)** | ~120h | ≤ 20h |
| **Complejidad Ciclomática Promedio** | N/A | ≤ 10 |
| **Duplicación Código** | N/A | < 3% |

---

## 🏷️ Clasificación de Severidad

| Nivel | Descripción | SLA Resolución |
|-------|-------------|----------------|
| 🔴 **Crítico** | Bloquea funcionalidad principal, seguridad, pérdida de datos | Inmediato / Sprint actual |
| 🟡 **Media** | Degrada experiencia, deuda técnica acumulable, riesgo medio | Próximo sprint |
| 🟢 **Baja** | Mejora opcional, nice-to-have, refactor cosmético | Backlog / Sprint de mantenimiento |

---

## 📝 Cómo Registrar Nueva Deuda Técnica

### Template de Issue

```markdown
## Descripción
Breve descripción del problema.

## Severidad
🔴 Crítica / 🟡 Media / 🟢 Baja

## Ubicación
- Archivo(s): `ruta/al/archivo.cs`
- Línea(s): 42-58

## Impacto
Qué funcionalidad se ve afectada.

## Causa Raíz (si conocida)
Por qué ocurre.

## Solución Propuesta
Cómo se planea resolver.

## Estimación
Horas/días estimados.

## Dependencias
Otros issues o PRs relacionados.
```

### Etiquetas GitHub
- `deuda-tecnica` — Todas las entradas
- `critico` / `media` / `baja` — Severidad
- `backend` / `frontend` / `arquitectura` — Área
- `sprint-ready` — Listo para planificar
- `blocked` — Bloqueado por dependencia

---

## 📈 Historial de Resolución

| Fecha | ID | Descripción | Resuelto Por | Commit/PR |
|-------|----|-------------|--------------|-----------|
| 2026-10-10 | — | Estructura N-Tier creada (AD, BR, API) | Isa | `init: n-tier structure` |
| 2026-10-10 | — | Documentación completa generada | Isa | `docs: complete documentation` |
| 2026-10-10 | — | Frontend restructurado (stores, composables, components) | Isa | `feat: frontend restructure` |

---

## 🔗 Referencias

- **README Principal:** [../README.md](../README.md)
- **Documentación Técnica:** [../Docs/README.md](../Docs/README.md)
- **Arquitectura:** [../Docs/backend/01-arquitectura/01-vision-general.md](../Docs/backend/01-arquitectura/01-vision-general.md)
- **Frontend Docs:** [../Docs/frontend/10-frontend/01-vision-general.md](../Docs/frontend/10-frontend/01-vision-general.md)