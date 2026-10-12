# Documentación del Proyecto - Sistema Industrial

## Estructura de Documentación

```
Docs/
├── backend/                     # Documentación del Backend (ASP.NET Core 9)
├── frontend/                    # Documentación del Frontend (Vue 3 + TypeScript + Vite)
├── sql/                         # Scripts SQL (Stored Procedures, tablas)
├── DATABASE.md                  # Documentación completa de la BD (snapshot, DDL, datos)
├── INFORMACION_ACADEMICA.md     # Información académica, listado e inscripción
└── README.md                    # Este archivo
```

---

## Backend (ASP.NET Core 9)

**Tecnologías**: .NET 9, SQL Server (Express / LocalDB / SQL Auth), JWT Bearer, BCrypt, ADO.NET (`Microsoft.Data.SqlClient`)

### [Arquitectura](./backend/01-arquitectura/)
- [Visión General](./backend/01-arquitectura/01-vision-general.md) — Diagramas, principios, stack tecnológico
- [Patrones de Diseño](./backend/01-arquitectura/02-patrones-diseno.md) — Repository, Template Method, DI, Strategy, DTO
- [Estructura del Proyecto](./backend/01-arquitectura/03-estructura-proyecto.md) — Árbol de directorios, responsabilidades por capa (AD, BR, API)

### [Base de Datos](./backend/02-base-de-datos/)
- [Esquema BD](./backend/02-base-de-datos/01-esquema-bd.md) — ER diagram, tablas, columnas, índices, FKs
- [Script de Creación](./backend/02-base-de-datos/02-script-creacion.md) — DDL completo
- [Migraciones](./backend/02-base-de-datos/03-migraciones.md) — Estrategia manual, patrones ALTER, versionado

### [Modelos](./backend/03-modelos/)
- [Entidades de Dominio](./backend/03-modelos/01-entidades-dominio.md) — `Semana`, `InsumoSemana`, `StockSemana`, `Categoria`, `TiradoProducto`, `Tirada`, `ProductoCatalogo`, `Usuario` (+ `RegistroTacho`: modelo sin `DbSet` ni endpoint mapeado; sin `ObjetivoSemana`)
- [DTOs y ViewModels](./backend/03-modelos/02-dtos-viewmodels.md) — `LoginRequest`, `SemanaResponse` (con `InsumosCount`), `TiradoProductoBulk*`, `ProductoCatalogoDtos`, `SyncFromTirado`, etc.
- [Validaciones](./backend/03-modelos/03-validaciones.md) — DataAnnotations, `SemanaValidator` (lunes-sábado, 6 días), reglas por entidad

### [Servicios](./backend/04-servicios/)
- [Capa de Servicios](./backend/04-servicios/01-capa-servicios.md) — DI directa en `Program.cs`, doble patrón (SPs vía `SpInvoker` + EF Core directo), servicios CRUD, `SyncFromTirado`, bulk de Tirado, `AuthService`
- [SpInvoker](./backend/04-servicios/01-capa-servicios.md#spinvoker-data-access-layer) — Métodos async `QueryEntityAsync/ExistsAsync/ScalarAsync/NonQueryAsync`
- [Servicios CRUD](./backend/04-servicios/01-capa-servicios.md#servicios-crud---detalle) — Semana, Insumo, Stock, TiradoProducto, Categoria, ProductoCatalogo, Dashboard
- [Servicio Auth](./backend/04-servicios/01-capa-servicios.md#authservice) — Login, BCrypt work factor 12, JWT 8h, refresh JWT 7d con rotación, change-password

### [API / Endpoints](./backend/05-api-endpoints/)
- [Convenciones Minimal API](./backend/05-api-endpoints/01-convenciones-minimal-api.md) — REST standards, status codes, formatos, Scalar/OpenAPI, CORS `VueCors`, modo demo
- [Auth Endpoints](./backend/05-api-endpoints/02-auth-endpoints.md) — `POST /api/auth/login`, refresh, logout; `register` deshabilitado (400)
- [Semanas](./backend/05-api-endpoints/03-semanas-endpoints.md) — CRUD `/api/semanas` (filtro `?anio`)
- [Insumos](./backend/05-api-endpoints/04-insumos-endpoints.md) — CRUD `/api/insumos?semanaId=`
- [Stock](./backend/05-api-endpoints/05-stock-endpoints.md) — CRUD `/api/stock` + `POST /api/stock/sync-from-tirado/{semanaId}`
- [Tirado](./backend/05-api-endpoints/06-tirado-endpoints.md) — `/api/tirado` + `bulk`, `tirar`, `tachos`, `filtro`
- [Categorías](./backend/05-api-endpoints/07-categorias-endpoints.md) — CRUD `/api/categorias`
- [Productos (catálogo)](./backend/05-api-endpoints/08-productos-endpoints.md) — CRUD `/api/productos`
- [Dashboard](./backend/05-api-endpoints/09-dashboard-endpoints.md) — Métricas, tendencias, alertas `/api/dashboard`
- [Health](./backend/05-api-endpoints/10-health-endpoints.md) — `GET /health`, `/health/ready`, `/health/live`

### [Autenticación y Autorización](./backend/06-autenticacion/)
- [JWT Authentication](./backend/06-autenticacion/01-jwt-auth.md) — Config, token generation, validation, claims, refresh tokens
- [Roles y Permisos](./backend/06-autenticacion/02-roles-permisos.md) — Matriz permisos, policies
- [Protección Endpoints](./backend/06-autenticacion/03-proteccion-endpoints.md) — Middleware pipeline, atributos, testing

### [Excepciones](./backend/07-excepciones/)
- [Jerarquía](./backend/07-excepciones/01-jerarquia-excepciones.md) — `PersistenceException`, `EntityNotFoundException`, `UnauthorizedAccessException`
- [Manejo Global](./backend/07-excepciones/02-manejo-global-errores.md) — Exception handler, formatos, logging, Problem Details

### [Configuración](./backend/08-configuracion/)
- [AppSettings](./backend/08-configuracion/01-appsettings.md) — JSON structure, ConnectionStrings (SQL Auth / LocalDB), JWT, logging
- [Variables de Entorno](./backend/08-configuracion/02-variables-entorno.md) — Dev/Staging/Prod, Docker, K8s, Azure, AWS
- [CORS y Middleware](./backend/08-configuracion/03-cors-middleware.md) — Pipeline orden, políticas, producción

### [Despliegue](./backend/09-despliegue/)
- [Requisitos Previos](./backend/09-despliegue/01-requisitos-previos.md) — .NET 9, SQL Server Express, puertos, estructura
- [Pasos de Instalación](./backend/09-despliegue/02-pasos-instalacion.md) — BD, config, build, primer admin, publicar
- [Variables Producción](./backend/09-despliegue/03-variables-produccion.md) — Secrets, connection strings, rotación
- [Troubleshooting](./backend/09-despliegue/04-troubleshooting.md) — Errores comunes BD, Auth, Build, Runtime, diagnósticos

### [Testing](./backend/11-testing/)
- [Tests Unitarios](./backend/11-testing/01-tests-unitarios.md) — MSTest + Moq + InMemory: 34 tests (BR 24, AD 6, API 4)

---

## Frontend (Vue 3 + TypeScript + Vite)

**Tecnologías**: Vue 3, TypeScript, Vite, Element Plus, Pinia, Vue Router 4, Fetch API

### [Documentación Frontend](./frontend/10-frontend/)
- [Visión General](./frontend/10-frontend/01-vision-general.md) — Stack, arquitectura, principios
- [Estructura del Proyecto](./frontend/10-frontend/02-estructura-proyecto.md) — Directorios, convenciones, módulos
- [Enrutamiento](./frontend/10-frontend/03-enrutamiento.md) — Vue Router, tabla rutas, guards, lazy loading
- [Autenticación](./frontend/10-frontend/04-autenticacion.md) — `useAuth` composable, login/logout, headers, guards, JWT
- [Componentes y Vistas](./frontend/10-frontend/05-componentes-vistas.md) — Componentes reutilizables, vistas por módulo, patrones
- [Integración API](./frontend/10-frontend/06-integracion-api.md) — Cliente HTTP, endpoints por módulo, tipado, errores
- [Configuración y Build](./frontend/10-frontend/07-configuracion.md) — Vite, TS, ESLint, Prettier, env vars

---

## Inicio Rápido

### Backend
```bash
# 1. Base de Datos (SQL Server)
# Opción A: SQL Server Express / Developer Edition
#   Conectar con SSMS / Azure Data Studio / VS Code
#   Crear BD 'SistemaIndustrialDB' y ejecutar DDL (ver docs/backend/02-base-de-datos/02-script-creacion.md)
#
# Opción B: LocalDB (desarrollo)
#   Se crea automáticamente al ejecutar la API con appsettings.json

# 2. Configurar connection string en appsettings.Development.json (SQL Auth)
# "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"

# 3. Configurar JWT en appsettings.Development.json
# "Jwt": { "Key": "...", "Issuer": "SistemaIndustrial", "Audience": "SistemaIndustrialClient" }

# 4. Compilar y ejecutar
cd SistemaIndustrial.API
dotnet run --environment Development
# → http://localhost:5127 | https://localhost:7244

# 5. Crear primer usuario (una sola vez, solo en Development)
curl -X POST http://localhost:5127/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@industrial.com","password":"Industrial2026","nombre":"Admin","role":"Admin"}'
```

### Frontend
```bash
cd Frontend
npm install
npm run dev  # → http://localhost:5173
```

---

## Integración Backend ↔ Frontend

| Aspecto | Backend | Frontend |
|---------|---------|----------|
| **Base URL** | `http://localhost:5127` | `VITE_API_URL=http://localhost:5127` |
| **Auth** | JWT Bearer 8h + Refresh 7d | `localStorage` + `useAuth` |
| **CORS** | `AllowCredentials()` + origen específico | Fetch directo a `VITE_API_URL` (con proxy Vite en dev) |
| **Endpoints** | Documentados en [05-api-endpoints](./backend/05-api-endpoints/) | Consumidos en [06-integracion-api](./frontend/10-frontend/06-integracion-api.md) |
| **Response Wrapper** | `ServiceResult<T>` / `ApiResponse<T>` | Se desempaqueta en composables/services |

---

## 📋 Notas Importantes (Estado Actual)

| Tema | Estado | Detalle |
|------|--------|---------|
| **Script DDL** | ✅ En `Docs/sql/` | Ver `docs/backend/02-base-de-datos/02-script-creacion.md` |
| **Connection String** | ✅ Configurado | `appsettings.Development.json` (SQL Auth) + `appsettings.json` (LocalDB) |
| **JWT Auth** | 🔄 En desarrollo | Login, refresh, logout implementados |
| **Swagger/OpenAPI** | ✅ OpenAPI nativo + Scalar (solo Development) | `MapOpenApi` + `MapScalarApiReference` en `Program.cs` |
| **Paginación** | ❌ No implementada | `GetAll` retorna todo |
| **Tests Backend** | ✅ 34 unitarios (BR 24, AD 6, API 4) | `dotnet test SistemaIndustrial.sln` |
| **Vistas Frontend completas** | 🔄 Parcial | Solo Semanas y Dashboard implementadas |

---