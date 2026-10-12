# Sistema Industrial

Aplicación web para la gestión integral de producción semanal de una fábrica
de productos de limpieza.

> Reemplaza planillas Excel manuales con un sistema centralizado que controla
> stock, tirado y armado en tiempo real.

---

## Impacto real

Este sistema está **en producción desde 2023** y reemplazó un proceso que
durante 5 años se hizo primero a mano y luego en Excel.

| Métrica | Resultado | Cómo se midió |
|---|---|---|
| **Productividad** de sectores afectados | **+40%** | Unidades producidas / hora antes vs. después |
| **Reducción de errores** técnicos | **60%** | Reclamos por errores de cálculo en 6 meses |
| **Automatización** del control de stock | **80%** | % de tareas que ya no requieren carga manual |
| **Tiempo del control de stock semanal** | de **3 horas a 1 hora** | Cronometrado durante 4 semanas |
| **Errores de cálculo** en tirado | **eliminados** | Sin reclamos por fórmulas desde el cambio |

### Antes vs. después

| Tarea | Antes (Excel) | Ahora (Sistema) |
|---|---|---|
| Control de stock semanal | 3 horas manuales | **1 hora automatizada** |
| Cálculo de tirado por producto | Fórmulas que se rompían | Cálculo automático |
| Sincronización entre sectores | Copiar/pegar entre planillas | Tiempo real |
| Historial por semana | 1 archivo Excel por semana | Base de datos centralizada |
| Errores de tipeo | Frecuentes | Validación en formularios |

### Contexto

- **2018-2020**: Control de producción a mano en papel
- **2020-2022**: Migré el proceso a Excel (fórmulas, tablas dinámicas)
- **2023-hoy**: Desarrollo este sistema web que reemplazó el Excel
- **Actualidad**: 20+ semanas registradas, uso diario en producción

---

## Funcionalidades

### Gestión de producción
- **Catálogo de productos** con pedidos semanales aproximados
- **Semanas laborales** (lunes a sábado) con validación automática
- **Carga inicial masiva** de productos por semana
- **Sector Tirado** con registro diario de tachos
- **Sector Armado** con vista consolidada
- **Control de stock** con sincronización automática desde tirado

### Cálculos automáticos
- Stock total por producto: `StockInicial + Tirado − Pedido`
- Tachos estimados según promedio por producto
- Progreso semanal basado en cobertura real
- Estados de semana (Planificada / EnProgreso / Completada / Cancelada)
- Alertas de faltantes y sobrantes

### Visualización
- **Dashboard** con progreso general y checklist
- **Detalle semanal** con tabs por módulo
- **Tachos diarios** con historial por producto
- **Filtros** por estado (Faltantes / Sobrantes)

---

## Arquitectura

```text
SistemaIndustrial/
├── Frontend/                        # Vue 3 + TypeScript + Vite
│   ├── src/
│   │   ├── components/              # Componentes reutilizables
│   │   ├── composables/             # Lógica compartida
│   │   ├── stores/                  # Pinia stores
│   │   ├── views/                   # Vistas por módulo
│   │   ├── types/                   # Tipos TypeScript
│   │   ├── services/api/            # Cliente HTTP
│   │   └── assets/styles/           # CSS organizado por capas
│   └── public/_redirects            # Configuración deploy (SPA)
├── SistemaIndustrial.AD/            # Capa de Acceso a Datos
│   ├── Models/                      # Entidades EF Core
│   ├── Data/                        # AppDbContext
│   ├── Interfaces/                  # Repositorios (contratos)
│   └── Repositories/                # Repositorios (SpInvoker / EF)
├── SistemaIndustrial.BR/            # Capa de Reglas de Negocio
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Services/                    # Lógica de negocio
│   ├── Validators/                  # Validaciones
│   └── Interfaces/                  # Contratos
├── SistemaIndustrial.API/           # Capa de Presentación
│   ├── Endpoints/                   # Minimal APIs
│   └── Program.cs
└── Docs/
    └── sql/                         # Stored Procedures
        ├── 14_create_tirado_productos.sql
        ├── 15_create_all_stored_procedures.sql
        ├── 16_create_productos_catalogo.sql
        └── 18_drop_registro_tacho.sql
```

### Stack técnico

**Frontend**
- Vue 3 (Composition API + `<script setup>`)
- TypeScript
- Pinia (state management)
- Element Plus (UI)
- Vue Router
- Vite (build tool)

**Backend**
- .NET 9 (ASP.NET Core Minimal APIs)
- Entity Framework Core
- SQL Server
- JWT (autenticación)
- Scalar (documentación de API)
- Stored Procedures para operaciones críticas

**Infraestructura**
- Netlify (frontend hosting)
- SQL Server (base de datos)

---

## Instalación local

### Requisitos

- Node.js 20+
- .NET 9 SDK
- SQL Server (local o remoto)

### 1. Clonar el repositorio

```bash
git clone https://github.com/Isaacjesusacevedo/SistemaIndustrial.git
cd SistemaIndustrial
```

### 2. Configurar la base de datos

Ejecutá en SSMS (como admin):

```sql
-- 1. Crear la base de datos
CREATE DATABASE SistemaIndustrialDB;
GO

-- 2. Crear el usuario (opcional)
CREATE LOGIN industrial_user WITH PASSWORD = 'Industrial2026';
ALTER SERVER ROLE dbcreator ADD MEMBER industrial_user;
GO
```

Después corré las migraciones de EF Core:

```bash
dotnet ef database update --project SistemaIndustrial.AD --startup-project SistemaIndustrial.API
```

Y ejecutá los scripts de SPs (en orden):

```text
Docs/sql/15_create_all_stored_procedures.sql
Docs/sql/16_create_productos_catalogo.sql
```

### 3. Configurar el backend

Editá `SistemaIndustrial.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "tu-clave-secreta-de-al-menos-32-caracteres",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:5174"
    ]
  }
}
```

### 4. Correr el backend

```bash
dotnet run --project SistemaIndustrial.API
```

Escucha en `http://localhost:5127`. Documentación disponible en
`http://localhost:5127/scalar/v1`.

### 5. Correr el frontend

```bash
cd Frontend
npm install
npm run dev
```

Disponible en `http://localhost:5173`.

## Capturas

> TODO: subir capturas a `Docs/screenshots/` con estos nombres.

- Dashboard de producción — `Docs/screenshots/dashboard.png`
- Detalle de semana — `Docs/screenshots/detalle-semana.png`
- Sector tirado — `Docs/screenshots/tirado.png`
- Catálogo de productos — `Docs/screenshots/productos.png`

## Scripts útiles

### Backend

```bash
# Compilar
dotnet build

# Correr en modo desarrollo
dotnet run --project SistemaIndustrial.API

# Tests unitarios
dotnet test

# Crear migración EF Core
dotnet ef migrations add NombreMigracion \
  --project SistemaIndustrial.AD \
  --startup-project SistemaIndustrial.API

# Aplicar migraciones
dotnet ef database update \
  --project SistemaIndustrial.AD \
  --startup-project SistemaIndustrial.API
```

### Frontend

```bash
# Instalar dependencias
npm install

# Correr en desarrollo
npm run dev

# Build de producción
npm run build

# Preview del build
npm run preview

# Type checking
npm run typecheck
```

## Modelo de datos

### Entidades principales

| Entidad | Descripción |
|---|---|
| Semana | Período laboral (lunes a sábado) |
| ProductoCatalogo | Catálogo general de productos |
| TiradoProducto | Productos cargados por semana |
| Tirada | Cada tacho individual tirado |
| StockSemana | Control de stock semanal |
| InsumoSemana | Insumos consumidos por semana |
| Usuario | Usuarios del sistema |

### Relaciones

```text
Semana (1) <── (N) TiradoProducto
TiradoProducto (1) <── (N) Tirada
Semana (1) <── (N) StockSemana
Semana (1) <── (N) InsumoSemana
ProductoCatalogo ── (plantilla para) TiradoProducto
```

## Diseño y UX

Principios:

- Dark mode por defecto (menor fatiga visual en planta)
- Tipografía monoespaciada para números (mejor lectura)
- Validación en tiempo real de formularios
- Feedback visual inmediato (toasts, colores)
- Navegación por semana activa persistida
- Responsive (mobile / tablet / desktop)

Sistema de diseño:

- Paleta oscura con acentos azules
- Variables CSS para consistencia
- Componentes reutilizables (KpiCard, DataTable, FormField)
- CSS organizado por capas (base/, layouts/, components/, views/)

## Testing

```bash
# Backend (34 unitarios: validators + services + API models)
dotnet test
```

## Roadmap

- [x] Módulo de semanas laborales
- [x] Catálogo de productos
- [x] Sector tirado con registro de tachos
- [x] Sector armado
- [x] Control de stock
- [x] Dashboard con progreso
- [x] Tests unitarios (backend)
- [ ] Tests de componentes (frontend)
- [ ] Reportes exportables a PDF
- [ ] Gráficos en dashboard
- [ ] Modo offline con sincronización
- [ ] App móvil (PWA)
- [ ] Integración con impresoras térmicas

## Contribuir

Este es un proyecto personal en producción. Si encontrás un bug o tenés una
sugerencia, abrí un issue.

## Licencia

Uso privado. Todos los derechos reservados.

## Autor

**Isaac Jesús Acevedo** — Desarrollador full-stack. Este sistema está en
producción desde 2023 y es utilizado diariamente en producción industrial.

- GitHub: [@Isaacjesusacevedo](https://github.com/Isaacjesusacevedo)
- LinkedIn: <!-- TODO: agregar perfil -->

## Agradecimientos

A todo el equipo de producción que probó y validó cada funcionalidad durante
los primeros meses de implementación.

<div align="center"><sub>Hecho en Argentina</sub></div>
