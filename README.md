<div align="center">

# Sistema Industrial

**Gestión de producción semanal para fábrica de productos de limpieza**

[![Live Demo](https://img.shields.io/badge/_Live_Demo-sistema--industrial.vercel.app-blue?style=for-the-badge)](https://sistema-industrial-teal.vercel.app/semanas)
[![Backend](https://img.shields.io/badge/API-Render-46E3B7?style=for-the-badge)](https://sistema-industrial-api.onrender.com/scalar/v1)
[![Vue 3](https://img.shields.io/badge/Vue_3-4FC08D?style=for-the-badge&logo=vue.js&logoColor=white)](https://vuejs.org)
[![.NET 9](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)

**Reemplaza 5 años de planillas Excel manuales con un sistema web en tiempo real.**

[ Ver Demo en Vivo](https://sistema-industrial-teal.vercel.app/semanas) ·
[ API Docs (Scalar)](https://sistema-industrial-api.onrender.com/scalar/v1) ·
[ Reportar Bug](https://github.com/Isaacjesusacevedo/SistemaIndustrial/issues)

</div>

---

## Probar la demo (sin registro)

La demo está abierta — no requiere login.

### [**Abrir la demo**](https://sistema-industrial-teal.vercel.app/semanas)

> **Nota sobre la demo**: el login está deshabilitado a propósito para
> facilitar la navegación. En producción real usa JWT + guard de rutas.

### Tour sugerido (3 minutos)

| # | Paso | Qué mirar |
|---|---|---|
| 1 | **[Dashboard](https://sistema-industrial-teal.vercel.app/dashboard)** | KPIs + progreso general + checklist de módulos |
| 2 | **[Semanas](https://sistema-industrial-teal.vercel.app/semanas)** | Historial de 20 semanas · badge "ACTIVA" · estado por color |
| 3 | **[Detalle de semana](https://sistema-industrial-teal.vercel.app/semanas/1)** | Tabs por módulo · progreso de cobertura · KPIs |
| 4 | **[Sector Tirado](https://sistema-industrial-teal.vercel.app/semanas/1/tirado)** | Tabla con input inline para cargar tachos · filtros Faltantes/Sobrantes |
| 5 | **[Tachos de la semana](https://sistema-industrial-teal.vercel.app/semanas/1/tirado/tachos)** | Resumen por día · tabla de tiradas individuales |
| 6 | **[Catálogo de Productos](https://sistema-industrial-teal.vercel.app/productos)** | Productos reutilizables entre semanas |
| 7 | **[Sector Armado](https://sistema-industrial-teal.vercel.app/semanas/1/armado)** | Vista consolidada del sector · ajuste de stock |

---

## Impacto real (en producción desde 2023)

Este sistema **reemplazó un proceso que se hacía a mano (2018-2020) y luego
en Excel (2020-2022)**.

| Métrica | Antes | Ahora | Mejora |
|---|---|---|---|
| **Control de stock semanal** | 3 horas manuales | **1 hora automatizada** | **66% tiempo** |
| **Productividad de sectores afectados** |  |  | **+40%** |
| **Errores técnicos** | Frecuentes | Validación automática | **60%** |
| **Automatización del control** | 20% | 80% | **80% automatizado** |
| **Errores de cálculo** | Fórmulas rotas | Cero | **100%** |

### Evolución

- **2018-2020** · Proceso a mano en papel
- **2020-2022** · Migré todo a Excel (fórmulas, tablas dinámicas)
- **2023-hoy** · Desarrollo y mantengo este sistema web
- **Actualidad** · 20+ semanas registradas, uso diario en planta

---

## Funcionalidades

### Gestión de producción
- Catálogo de productos reutilizable entre semanas
- Semanas laborales (lunes a sábado) con validación
- Carga inicial masiva de productos desde el catálogo
- Registro diario de tachos con input inline
- Cálculos automáticos de stock, tachos y progreso
- Vista consolidada para sector Armado

### Cálculos y automatizaciones
- `StockTotal = StockInicial + Tirado − PedidoSemanal`
- `TachosEstimados = StockTotal / PromedioPorTacho`
- **Progreso semanal** basado en cobertura real (no en productos cargados)
- **Estados automáticos** (Planificada → EnProgreso → Completada)
- **Alertas** de faltantes y sobrantes

### UX/Diseño
- **Dark mode** por defecto (reduce fatiga visual en planta)
- **Responsive** (mobile, tablet, desktop)
- Componentes reutilizables (KpiCard, DataTable, FormField)
- **Feedback inmediato** (toasts, validación en tiempo real)

---

## Stack técnico

### Frontend
| Tecnología | Uso |
|---|---|
| **Vue 3** (Composition API) | Framework principal |
| **TypeScript** | Tipado estricto |
| **Vite** | Build tool |
| **Pinia** | State management |
| **Element Plus** | Componentes UI |
| **Vue Router** | Navegación |
| **Vercel** | Hosting |

### Backend
| Tecnología | Uso |
|---|---|
| **.NET 9** | Runtime |
| **ASP.NET Core** (Minimal APIs) | Web framework |
| **Entity Framework Core** | ORM |
| **SQL Server** | Base de datos |
| **JWT** | Autenticación |
| **Scalar** | Documentación de API |
| **Render** | Hosting |

### Arquitectura por capas

```text
SistemaIndustrial/
├── Frontend/                    # Vue 3 + Vite
├── SistemaIndustrial.AD/        # Acceso a Datos (repositorios + EF)
├── SistemaIndustrial.BR/        # Reglas de Negocio (servicios + validadores)
├── SistemaIndustrial.API/       # Endpoints (minimal APIs)
└── Docs/sql/                    # Stored Procedures
```

- **AD** · Entidades EF Core + Repositorios (SpInvoker + LINQ)
- **BR** · DTOs + Servicios + Validadores
- **API** · Minimal APIs + JWT + Middleware de errores

---

## Capturas

> TODO: subir las capturas a `Docs/screenshots/`.

### Dashboard
![Dashboard](./Docs/screenshots/dashboard.png)

### Detalle de semana con progreso
![Detalle](./Docs/screenshots/detalle-semana.png)

### Sector Tirado con carga inline
![Tirado](./Docs/screenshots/tirado.png)

### Catálogo de productos
![Productos](./Docs/screenshots/productos.png)

## Sistema anterior (Excel)

Este sistema web reemplaza un proceso que se hacía en Excel desde 2020.
El archivo original y su análisis están en [`Docs/legacy/`](./Docs/legacy/).

---

## Ejecutar localmente

### Requisitos

- **Node.js 20+**
- **.NET 9 SDK**
- **SQL Server** (Express, LocalDB o Docker)
- **Git**

### 1. Clonar el repo

```bash
git clone https://github.com/Isaacjesusacevedo/SistemaIndustrial.git
cd SistemaIndustrial
```

### 2. Levantar la base de datos

```sql
CREATE DATABASE SistemaIndustrialDB;
GO

CREATE LOGIN industrial_user WITH PASSWORD = 'Industrial2026';
ALTER SERVER ROLE dbcreator ADD MEMBER industrial_user;
GO
```

### 3. Configurar el backend

Editar `SistemaIndustrial.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "TuClaveSecretaDeAlMenos32Caracteres",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  }
}
```

### 4. Aplicar migraciones

```bash
dotnet ef database update \
  --project SistemaIndustrial.AD \
  --startup-project SistemaIndustrial.API
```

### 5. Ejecutar SPs

Abrir `Docs/sql/15_create_all_stored_procedures.sql` en SSMS y ejecutar.

### 6. Levantar el backend

```bash
dotnet run --project SistemaIndustrial.API
```

Backend en `http://localhost:5127`
Docs en `http://localhost:5127/scalar/v1`

### 7. Levantar el frontend

```bash
cd Frontend
npm install
npm run dev
```

Frontend en `http://localhost:5173`

---

## Modelo de datos

```text
Semana (1) <── (N) TiradoProducto (1) <── (N) Tirada
Semana (1) <── (N) StockSemana
Semana (1) <── (N) InsumoSemana
TiradoProducto (N) ── (1) Categoria
ProductoCatalogo ── (plantilla para) TiradoProducto
Usuario (auth)
```

### Entidades principales

| Entidad | Descripción |
|---|---|
| `Semana` | Período laboral (lunes a sábado) |
| `ProductoCatalogo` | Catálogo general reutilizable |
| `TiradoProducto` | Productos cargados en una semana específica |
| `Tirada` | Cada tacho individual que se tira |
| `StockSemana` | Control de stock semanal |
| `Usuario` | Autenticación y roles |

---

## Decisiones de diseño

### ¿Por qué Vue 3 + Composition API?
Mejor tipado con TypeScript, lógica reutilizable vía composables, y mejor
performance que Options API en apps medianas.

### ¿Por qué Minimal APIs en lugar de Controllers?
Menos boilerplate, más simple de mantener, endpoints agrupados por módulo.

### ¿Por qué SPs + LINQ mixto?
- **SPs** para queries críticas (semana + joins con conteos)
- **LINQ** para CRUDs simples (tirado, categorías)
- **Decisión pragmática**: migrar 100% a LINQ sería ideal pero el costo/beneficio
  no lo justificaba.

### ¿Por qué dark mode por defecto?
La app se usa en planta con poca luz. El dark mode reduce fatiga visual y
mejora la legibilidad en pantallas grandes.

### ¿Por qué el progreso se basa en "cobertura"?
Inicialmente medía `tirado / pedido`. El problema: si había stock inicial,
el progreso mostraba valores bajos aunque el pedido ya estuviera cubierto.
Cambiamos a `(stockInicial + tirado) / pedido` que refleja la realidad.

---

## Testing

```bash
# Backend (34 unitarios en verde: validators + services + API models)
dotnet test
```

- [x] Tests unitarios backend (MSTest + Moq + EF InMemory)
- [ ] Tests de componentes (Vitest)
- [ ] Tests E2E (Playwright)

---

## Roadmap

- [x] Módulo de semanas laborales
- [x] Catálogo de productos
- [x] Sector tirado con registro de tachos
- [x] Sector armado con vista consolidada
- [x] Control de stock
- [x] Dashboard con progreso
- [x] Tests unitarios (backend)
- [x] Deploy en Vercel + Render
- [ ] Tests de componentes (frontend)
- [ ] PWA (offline-first)
- [ ] Reportes exportables a PDF
- [ ] Gráficos en dashboard
- [ ] Integración con impresoras térmicas

---

## Sobre mí

**Isaac Jesús Acevedo**
Desarrollador Full Stack

- [GitHub](https://github.com/Isaacjesusacevedo)
- [LinkedIn](https://linkedin.com/in/tu-perfil) <!-- TODO: perfil real -->
- isaacacevedor1610@gmail.com

---

## Licencia

Uso privado. Todos los derechos reservados.

---

<div align="center">

**Si te sirvió, dejame una estrella en GitHub**

Hecho con esfuerzo en Argentina

</div>
