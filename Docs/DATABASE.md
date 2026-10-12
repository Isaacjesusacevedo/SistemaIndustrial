# Documentación de Base de Datos - Sistema Industrial

> Documento consolidado con esquema, scripts, datos de referencia y mantenimiento.
> Generado: 2026-10-12 | Versión: 1.1.0
> Fuente de verdad: `SistemaIndustrial.AD/Data/AppDbContext.cs`, `SistemaIndustrial.AD/Models/*.cs`, `Docs/sql/14/15/16/18` y `SistemaIndustrial.AD/Migrations/20261011162134_AddCategoriasYTiradas`.

---

## Índice

1. [Esquema General](#1-esquema-general)
2. [Tablas](#2-tablas)
3. [Relaciones y Constraints](#3-relaciones-y-constraints)
4. [Índices](#4-índices)
5. [Stored Procedures](#5-stored-procedures)
6. [Datos de Referencia (Seed)](#6-datos-de-referencia-seed)
7. [Scripts de Creación](#7-scripts-de-creación)
8. [Mantenimiento](#8-mantenimiento)
9. [Estadísticas](#9-estadísticas)
10. [Lecciones Aprendidas](#10-lecciones-aprendidas)

---

## 1. Esquema General

### Diagrama Entidad-Relación

```mermaid
erDiagram
    SEMANAS ||--o{ INSUMOS_SEMANA : requiere
    SEMANAS ||--o{ STOCK_SEMANA : gestiona
    SEMANAS ||--o{ TIRADOSPRODUCTO : controla
    CATEGORIAS ||--o{ TIRADOSPRODUCTO : clasifica
    TIRADOSPRODUCTO ||--o{ TIRADAS : detalle

    USUARIOS {
        int Id PK
        string Email UK
        string PasswordHash
        string Nombre
        string Role
        string RefreshTokenHash
        datetime RefreshTokenExpiry
        bit Activo
        datetime FechaCreacion
    }

    SEMANAS {
        int Id PK
        int NumeroSemana
        int Anio
        datetime FechaInicio
        datetime FechaFin
        string Estado
        datetime FechaCreacion
    }

    INSUMOS_SEMANA {
        int Id PK
        int SemanaId FK
        string Nombre
        string UnidadMedida
        decimal CantidadRequerida
        decimal CantidadConsumida
        decimal StockDisponible
        datetime FechaCreacion
    }

    STOCK_SEMANA {
        int Id PK
        int SemanaId FK
        string Producto
        string UnidadMedida
        decimal StockInicial
        decimal Entradas
        decimal Salidas
        decimal StockFinal
        datetime FechaCreacion
    }

    CATEGORIAS {
        int Id PK
        string Nombre UK
        int Orden
        bit Activa
    }

    PRODUCTOSCATALOGO {
        int Id PK
        string Nombre UK
        string UnidadMedida
        decimal PedidoSemanalAproximado
        decimal PromedioPorTacho
        bit Activo
        string Observaciones
        datetime FechaCreacion
        datetime FechaActualizacion
    }

    TIRADOSPRODUCTO {
        int Id PK
        int SemanaId FK
        int CategoriaId FK_NULL
        string Nombre
        string UnidadMedida
        decimal PedidoSemanal
        decimal StockInicial
        decimal PromedioPorTacho
        decimal CantidadTirada
        decimal Stock
        string Observaciones
        datetime FechaCreacion
        datetime FechaActualizacion
    }

    TIRADAS {
        int Id PK
        int TiradoProductoId FK
        datetime Fecha
        decimal Cantidad
        string Observaciones
        datetime FechaCreacion
    }
```

> **Modelo vigente (`AppDbContext.cs`): 8 DbSets** — `Usuarios`, `Semanas`, `InsumosSemana`, `StockSemana`, `Categorias`, `ProductosCatalogo`, `TiradosProducto`, `Tiradas`. `ObjetivosSemana` y `RegistroTacho` están **eliminados del modelo** (sin `DbSet`; `Semana` solo expone `Insumos`, `Stocks` y `TiradosProducto`). `RegistroTacho.cs` aún existe como clase sin mapear; no existe `Models/ObjetivoSemana.cs`. El script `18_drop_registro_tacho.sql` está pendiente de ejecución (su cabecera indica no correrlo hasta actualizar el backend).

### Resumen de Objetos

| Tipo | Cantidad | Descripción |
|------|----------|-------------|
| Tablas (modelo vigente) | 8 | Usuarios, Semanas, InsumosSemana, StockSemana, Categorias, ProductosCatalogo, TiradosProducto, Tiradas |
| Tablas eliminadas del modelo | 2 | ObjetivosSemana, RegistroTacho/RegistrosTacho (histórico; ver script 18 pendiente) |
| Vistas | 0 | No hay vistas definidas |
| Stored Procedures (script 15 real) | 30 | 8 Semanas (`sp_Semanas_*`) + 7 Insumos (`sp_Insumos_*`) + 7 Stock (`sp_Stock_*`) + 8 Usuarios (`sp_Usuarios_*`); sin Objetivos/Registro/Dashboard |
| Stored Procedures (otros scripts) | 7 + 9 | 7 `sp_ProductosCatalogo_*` (script 16) + SPs `sp_TiradoProductos_*` (script 14, desactualizado; repos nuevos usan EF directo) |
| Funciones | 0 | No hay funciones UDF |
| Triggers | 0 | No hay triggers |
| Índices | ~20 | PK, FK, UNIQUE, filtros |

---

## 2. Tablas

### 2.1 Usuarios

```sql
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Operador',
    RefreshTokenHash NVARCHAR(255) NULL,
    RefreshTokenExpiry DATETIME NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Usuarios_Email UNIQUE NONCLUSTERED (Email ASC)
);

CREATE NONCLUSTERED INDEX IX_Usuarios_Activo ON Usuarios (Activo);
CREATE NONCLUSTERED INDEX IX_Usuarios_RefreshToken ON Usuarios (RefreshTokenHash) WHERE RefreshTokenHash IS NOT NULL;
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| Email | nvarchar(255) | NO | | UK, login |
| PasswordHash | nvarchar(255) | NO | | BCrypt hash |
| Nombre | nvarchar(100) | NO | | Nombre completo |
| Role | nvarchar(50) | NO | 'Operador' | Admin, Supervisor, Operador |
| RefreshTokenHash | nvarchar(255) | YES | NULL | Hash BCrypt del refresh token |
| RefreshTokenExpiry | datetime | YES | NULL | Expiración refresh token |
| Activo | bit | NO | 1 | Soft delete |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Datos Iniciales:**
```sql
-- Password: Industrial2026 (BCrypt workFactor 12)
INSERT INTO Usuarios (Email, PasswordHash, Nombre, Role, Activo)
VALUES ('admin@industrial.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Administrador', 'Admin', 1);
```

---

### 2.2 Semanas

```sql
CREATE TABLE Semanas (
    Id INT IDENTITY(1,1) NOT NULL,
    NumeroSemana INT NOT NULL,
    Anio INT NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Planificada',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_Semanas PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Semanas_NumeroAnio UNIQUE NONCLUSTERED (NumeroSemana ASC, Anio ASC),
    CONSTRAINT CK_Semanas_Numero CHECK (NumeroSemana BETWEEN 1 AND 53),
    CONSTRAINT CK_Semanas_Anio CHECK (Anio BETWEEN 2020 AND 2030),
    CONSTRAINT CK_Semanas_Estado CHECK (Estado IN ('Planificada','EnProgreso','Completada','Cancelada')),
    CONSTRAINT CK_Semanas_Fechas CHECK (FechaFin > FechaInicio)
);

CREATE NONCLUSTERED INDEX IX_Semanas_Fechas ON Semanas (FechaInicio, FechaFin);
CREATE NONCLUSTERED INDEX IX_Semanas_Anio ON Semanas (Anio DESC);
CREATE NONCLUSTERED INDEX IX_Semanas_Estado ON Semanas (Estado);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| NumeroSemana | int | NO | | 1-53 (ISO 8601) |
| Anio | int | NO | | Año calendario |
| FechaInicio | datetime | NO | | Lunes semana |
| FechaFin | datetime | NO | | Domingo semana |
| Estado | nvarchar(20) | NO | 'Planificada' | Planificada/EnProgreso/Completada/Cancelada |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Estados Válidos:**
- `Planificada` - Semana creada, sin actividad
- `EnProgreso` - Semana activa, registros en curso
- `Completada` - Semana cerrada, métricas finales
- `Cancelada` - Semana anulada

---

### 2.3 ObjetivosSemana (ELIMINADA del modelo — histórico)

> **Eliminada del `AppDbContext` vigente** (sin `DbSet`; no existe `Models/ObjetivoSemana.cs`). Solo queda en la migración `20261011162134` y el snapshot. El DDL/SPs abajo se conservan como referencia histórica.

```sql
CREATE TABLE ObjetivosSemana (
    Id INT IDENTITY(1,1) NOT NULL,
    SemanaId INT NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    Meta DECIMAL(18,2) NOT NULL,
    Logrado DECIMAL(18,2) NOT NULL DEFAULT 0,
    Porcentaje DECIMAL(5,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_ObjetivosSemana PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_ObjetivosSemana_Semanas FOREIGN KEY (SemanaId) 
        REFERENCES Semanas (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Objetivos_Meta CHECK (Meta > 0),
    CONSTRAINT CK_Objetivos_Logrado CHECK (Logrado >= 0),
    CONSTRAINT CK_Objetivos_Porcentaje CHECK (Porcentaje >= 0 AND Porcentaje <= 100)
);

CREATE NONCLUSTERED INDEX IX_ObjetivosSemana_SemanaId ON ObjetivosSemana (SemanaId);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| SemanaId | int | NO | | FK → Semanas |
| Descripcion | nvarchar(500) | NO | | Texto objetivo |
| Meta | decimal(18,2) | NO | | Valor objetivo |
| Logrado | decimal(18,2) | NO | 0 | Valor actual |
| Porcentaje | decimal(5,2) | NO | 0 | (Logrado/Meta)*100 |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Regla de Negocio:** `Porcentaje` se recalcula automáticamente en BR al actualizar `Logrado`.

---

### 2.4 InsumosSemana

```sql
CREATE TABLE InsumosSemana (
    Id INT IDENTITY(1,1) NOT NULL,
    SemanaId INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    UnidadMedida NVARCHAR(50) NOT NULL,
    CantidadRequerida DECIMAL(18,2) NOT NULL,
    CantidadConsumida DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockDisponible DECIMAL(18,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_InsumosSemana PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_InsumosSemana_Semanas FOREIGN KEY (SemanaId) 
        REFERENCES Semanas (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Insumos_Cantidades CHECK (CantidadRequerida >= 0 AND CantidadConsumida >= 0 AND StockDisponible >= 0)
);

CREATE NONCLUSTERED INDEX IX_InsumosSemana_SemanaId ON InsumosSemana (SemanaId);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| SemanaId | int | NO | | FK → Semanas |
| Nombre | nvarchar(200) | NO | | Nombre insumo |
| UnidadMedida | nvarchar(50) | NO | | kg, lt, un, m, m2, m3 |
| CantidadRequerida | decimal(18,2) | NO | | Planificado |
| CantidadConsumida | decimal(18,2) | NO | 0 | Real consumido |
| StockDisponible | decimal(18,2) | NO | 0 | Stock actual |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

---

### 2.5 StockSemana

```sql
CREATE TABLE StockSemana (
    Id INT IDENTITY(1,1) NOT NULL,
    SemanaId INT NOT NULL,
    Producto NVARCHAR(200) NOT NULL,
    UnidadMedida NVARCHAR(50) NOT NULL,
    StockInicial DECIMAL(18,2) NOT NULL,
    Entradas DECIMAL(18,2) NOT NULL DEFAULT 0,
    Salidas DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockFinal DECIMAL(18,2) NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_StockSemana PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_StockSemana_Semanas FOREIGN KEY (SemanaId) 
        REFERENCES Semanas (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Stock_Valores CHECK (StockInicial >= 0 AND Entradas >= 0 AND Salidas >= 0),
    CONSTRAINT CK_Stock_Final CHECK (StockFinal = StockInicial + Entradas - Salidas)
);

CREATE NONCLUSTERED INDEX IX_StockSemana_SemanaId ON StockSemana (SemanaId);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| SemanaId | int | NO | | FK → Semanas |
| Producto | nvarchar(200) | NO | | Producto terminado |
| UnidadMedida | nvarchar(50) | NO | | kg, lt, un, m |
| StockInicial | decimal(18,2) | NO | | Inicio semana |
| Entradas | decimal(18,2) | NO | 0 | Producción + ingresos |
| Salidas | decimal(18,2) | NO | 0 | Ventas + consumos |
| StockFinal | decimal(18,2) | NO | | Calculado: Inicial + Entradas - Salidas |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Regla de Negocio:** `StockFinal` se recalcula automáticamente en BR: `StockInicial + Entradas - Salidas`.

---

### 2.6 RegistroTacho (ELIMINADO del modelo — histórico, DROP pendiente)

> **Eliminado del `AppDbContext` vigente** (sin `DbSet`; `Semana` ya no expone `RegistrosTacho`). La clase `Models/RegistroTacho.cs` aún existe sin mapear y la migración/snapshot todavía crean `RegistrosTacho`. El script `18_drop_registro_tacho.sql` elimina SPs (`sp_RegistroTacho_*` y `sp_RegistrosTacho_*`) y la tabla `RegistrosTacho`, pero su cabecera indica **no ejecutarlo todavía** (backend aún referencia `RegistroTachoRepository`). DDL/SPs abajo solo como referencia histórica.

```sql
CREATE TABLE RegistroTacho (
    Id INT IDENTITY(1,1) NOT NULL,
    SemanaId INT NOT NULL,
    Turno INT NOT NULL,
    Operador NVARCHAR(100) NOT NULL,
    CantidadProducida DECIMAL(18,2) NOT NULL,
    CantidadRechazada DECIMAL(18,2) NOT NULL DEFAULT 0,
    EstadoCalidad NVARCHAR(20) NOT NULL DEFAULT 'Aprobado',
    FechaRegistro DATETIME NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_RegistroTacho PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_RegistroTacho_Semanas FOREIGN KEY (SemanaId) 
        REFERENCES Semanas (Id) ON DELETE CASCADE,
    CONSTRAINT CK_RegistroTacho_Turno CHECK (Turno IN (1,2,3)),
    CONSTRAINT CK_RegistroTacho_Cantidades CHECK (CantidadProducida >= 0 AND CantidadRechazada >= 0),
    CONSTRAINT CK_RegistroTacho_EstadoCalidad CHECK (EstadoCalidad IN ('Aprobado','Revision','Rechazado'))
);

CREATE NONCLUSTERED INDEX IX_RegistroTacho_SemanaId ON RegistroTacho (SemanaId);
CREATE NONCLUSTERED INDEX IX_RegistroTacho_Turno ON RegistroTacho (Turno);
CREATE NONCLUSTERED INDEX IX_RegistroTacho_FechaRegistro ON RegistroTacho (FechaRegistro);
CREATE NONCLUSTERED INDEX IX_RegistroTacho_Operador ON RegistroTacho (Operador);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| SemanaId | int | NO | | FK → Semanas |
| Turno | int | NO | | 1=Mañana, 2=Tarde, 3=Noche |
| Operador | nvarchar(100) | NO | | Nombre operador |
| CantidadProducida | decimal(18,2) | NO | | Unidades buenas |
| CantidadRechazada | decimal(18,2) | NO | 0 | Unidades defecto |
| EstadoCalidad | nvarchar(20) | NO | 'Aprobado' | Aprobado/Revision/Rechazado |
| FechaRegistro | datetime | NO | | Timestamp real |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Estados Calidad:**
- `Aprobado` - Cumple especificaciones
- `Revision` - Pendiente inspección
- `Rechazado` - No cumple, descartado

---
### 2.7 TiradosProducto (entidad `TiradoProducto`)

> **Modelo vigente (`Models/TiradoProducto.cs` + `OnModelCreating`):** tabla `TiradosProducto` (la migración crea `TiradosProducto`; el script histórico `14_create_tirado_productos.sql` crea `TiradoProductos` con `CantidadRequerida` y sin `CategoriaId`/`StockInicial`/`PromedioPorTacho`/`PedidoSemanal`, por lo que está desactualizado). Campos exactos: `Id`, `SemanaId` (FK CASCADE), `CategoriaId` (FK nullable → `Categorias`, `SetNull`), `Nombre`, `UnidadMedida`, `PedidoSemanal` (renombrado desde `CantidadRequerida`), `StockInicial`, `PromedioPorTacho`, `CantidadTirada` (default 0), `Stock` (default 0), `Observaciones` (null), `FechaCreacion` (GETDATE()), `FechaActualizacion` (null). UK `(SemanaId, Nombre)`.

```sql
-- Modelo EF vigente (no el script 14 histórico):
-- TiradosProducto (Id, SemanaId FK CASCADE, CategoriaId FK NULL SET NULL,
-- Nombre, UnidadMedida, PedidoSemanal, StockInicial, PromedioPorTacho,
-- CantidadTirada DEFAULT 0, Stock DEFAULT 0, Observaciones NULL,
-- FechaCreacion DEFAULT GETDATE(), FechaActualizacion NULL,
-- UNIQUE (SemanaId, Nombre))

```sql
CREATE TABLE TiradoProductos (
    Id INT IDENTITY(1,1) NOT NULL,
    SemanaId INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    UnidadMedida NVARCHAR(50) NOT NULL,
    CantidadRequerida DECIMAL(18,2) NOT NULL,
    CantidadTirada DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stock DECIMAL(18,2) NOT NULL DEFAULT 0,
    Observaciones NVARCHAR(500) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_TiradoProductos PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_TiradoProductos_Semanas FOREIGN KEY (SemanaId) 
        REFERENCES Semanas (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TiradoProductos_Semana_Nombre UNIQUE NONCLUSTERED (SemanaId, Nombre),
    CONSTRAINT CK_TiradoProductos_Cantidades CHECK (CantidadRequerida > 0 AND CantidadTirada >= 0 AND Stock >= 0)
);

CREATE NONCLUSTERED INDEX IX_TiradoProductos_SemanaId ON TiradoProductos (SemanaId);
CREATE NONCLUSTERED INDEX IX_TiradoProductos_Nombre ON TiradoProductos (Nombre);
```

**Columnas:**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| SemanaId | int | NO | | FK → Semanas |
| Nombre | nvarchar(100) | NO | | Nombre del producto |
| UnidadMedida | nvarchar(50) | NO | | kg, lt, un, m, m2, m3, t, pza |
| CantidadRequerida | decimal(18,2) | NO | | Cantidad objetivo a tirar |
| CantidadTirada | decimal(18,2) | NO | 0 | Cantidad ya tirada (acumulativa) |
| Stock | decimal(18,2) | NO | 0 | Stock actual (= CantidadTirada) |
| Observaciones | nvarchar(500) | YES | NULL | Observaciones |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |
| FechaActualizacion | datetime | NO | GETDATE() | Última actualización |

**Estados (calculados en BR, no en BD):**
- `Faltante = MAX(0, PedidoSemanal - CantidadTirada)`
- `Sobrante = MAX(0, CantidadTirada - PedidoSemanal)`
- `Porcentaje = (CantidadTirada / PedidoSemanal) * 100`
- `Estado`: `Pendiente` (tirada < pedido), `Completado` (=), `Excedido` (>), `SinPlanificar` (pedido = 0)

> Nota: el DDL histórico del script 14 usa `CantidadRequerida`; el modelo vigente usa `PedidoSemanal` más `CategoriaId`, `StockInicial` y `PromedioPorTacho`.

---

### 2.8 Categorias

```sql
-- Migración 20261011162134_AddCategoriasYTiradas (modelo vigente)
-- Categorias (Id IDENTITY PK, Nombre NVARCHAR(450) UNIQUE, Orden DEFAULT 0, Activa DEFAULT 1)
```

**Columnas (`Models/Categoria.cs`):**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| Nombre | nvarchar(450) | NO | | UK, nombre categoría |
| Orden | int | NO | 0 | Orden de presentación |
| Activa | bit | NO | 1 | Soft delete lógico |

**Relación:** `Categoria 1-* TiradosProducto` (`CategoriaId` nullable, `ON DELETE SET NULL`).

---

### 2.9 ProductosCatalogo

```sql
-- Script 16_create_productos_catalogo.sql (catálogo general, sin SemanaId)
-- ProductosCatalogo (Id, Nombre UNIQUE, UnidadMedida, PedidoSemanalAproximado DEFAULT 0,
-- PromedioPorTacho DEFAULT 0, Activo DEFAULT 1, Observaciones NULL,
-- FechaCreacion DEFAULT GETDATE(), FechaActualizacion NULL,
-- CHECK (PedidoSemanalAproximado >= 0 AND PromedioPorTacho >= 0))
```

**Columnas (`Models/ProductoCatalogo.cs`):**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| Nombre | nvarchar(200) | NO | | UK |
| UnidadMedida | nvarchar(50) | NO | | kg, lt, un, m, etc. |
| PedidoSemanalAproximado | decimal(18,2) | NO | 0 | Estimación semanal |
| PromedioPorTacho | decimal(18,2) | NO | 0 | Rendimiento por tacho |
| Activo | bit | NO | 1 | Soft delete lógico |
| Observaciones | nvarchar(500) | YES | NULL | Notas |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |
| FechaActualizacion | datetime | YES | NULL | Última actualización |

> Sin `SemanaId` ni FK: es catálogo general. **No cubierto por la última migración EF** (solo existe por script 16 + `DbSet ProductosCatalogo`).

---

### 2.10 Tiradas

```sql
-- Migración 20261011162134_AddCategoriasYTiradas (modelo vigente)
-- Tiradas (Id IDENTITY PK, TiradoProductoId FK CASCADE, Fecha, Cantidad DECIMAL(18,2),
-- Observaciones NULL, FechaCreacion DEFAULT GETDATE())
```

**Columnas (`Models/Tirada.cs`):**
| Columna | Tipo | Null | Default | Descripción |
|---------|------|------|---------|-------------|
| Id | int | NO | IDENTITY | PK |
| TiradoProductoId | int | NO | | FK → TiradosProducto |
| Fecha | datetime | NO | | Fecha de la tirada |
| Cantidad | decimal(18,2) | NO | | Cantidad tirada |
| Observaciones | nvarchar(max) | YES | NULL | Notas |
| FechaCreacion | datetime | NO | GETDATE() | Auditoría |

**Relación:** `TiradosProducto 1-* Tiradas` (`ON DELETE CASCADE`). Los repos nuevos (`TiradoProductoRepository`) usan EF directo (`_context.Tiradas.Add` + `SaveChanges`), sin SPs.

---

## 3. Relaciones y Constraints

### Foreign Keys

| Tabla Hija | Columna | Tabla Padre | Columna | On Delete | On Update |
|------------|---------|-------------|---------|-----------|-----------|
| InsumosSemana | SemanaId | Semanas | Id | CASCADE | NO ACTION |
| StockSemana | SemanaId | Semanas | Id | CASCADE | NO ACTION |
| TiradosProducto | SemanaId | Semanas | Id | CASCADE | NO ACTION |
| TiradosProducto | CategoriaId (nullable) | Categorias | Id | SET NULL | NO ACTION |
| Tiradas | TiradoProductoId | TiradosProducto | Id | CASCADE | NO ACTION |
| ~~ObjetivosSemana~~ | ~~SemanaId~~ | ~~Semanas~~ | ~~Id~~ | ~~CASCADE~~ | Eliminada del modelo (solo migración/snapshot histórico) |
| ~~RegistroTacho/RegistrosTacho~~ | ~~SemanaId~~ | ~~Semanas~~ | ~~Id~~ | ~~CASCADE~~ | Eliminado del modelo (DROP pendiente script 18) |

**Nota:** `ON DELETE CASCADE` asegura integridad: al eliminar semana, se eliminan sus datos dependientes.

### Unique Constraints

| Tabla | Constraint | Columnas | Propósito |
|-------|------------|----------|-----------|
| Usuarios | UQ_Usuarios_Email | Email | Email único por usuario |
| Semanas | UQ_Semanas_NumeroAnio | NumeroSemana, Anio | Una semana por número/año |
| Categorias | UQ (IX_Categorias_Nombre) | Nombre | Categoría única |
| ProductosCatalogo | UQ_ProductosCatalogo_Nombre | Nombre | Producto catálogo único |
| TiradosProducto | UQ (IX_TiradosProducto_SemanaId_Nombre) | SemanaId, Nombre | Un nombre por semana |

### Check Constraints

| Tabla | Constraint | Regla |
|-------|------------|-------|
| Semanas | CK_Semanas_Numero | NumeroSemana BETWEEN 1 AND 53 |
| Semanas | CK_Semanas_Anio | Anio BETWEEN 2020 AND 2030 |
| Semanas | CK_Semanas_Estado | IN ('Planificada','EnProgreso','Completada','Cancelada') |
| Semanas | CK_Semanas_Fechas | FechaFin > FechaInicio |
| ObjetivosSemana | CK_Objetivos_Meta | Meta > 0 |
| ObjetivosSemana | CK_Objetivos_Logrado | Logrado >= 0 |
| ObjetivosSemana | CK_Objetivos_Porcentaje | Porcentaje BETWEEN 0 AND 100 |
| InsumosSemana | CK_Insumos_Cantidades | Todas las cantidades >= 0 |
| StockSemana | CK_Stock_Valores | Inicial, Entradas, Salidas >= 0 |
| StockSemana | CK_Stock_Final | StockFinal = Inicial + Entradas - Salidas |
| ProductosCatalogo | CK_ProductosCatalogo_Valores | PedidoSemanalAproximado >= 0 AND PromedioPorTacho >= 0 |
| ~~ObjetivosSemana~~ | ~~CK_Objetivos_*~~ | Histórico (tabla eliminada del modelo) |
| ~~RegistroTacho~~ | ~~CK_RegistroTacho_*~~ | Histórico (tabla eliminada del modelo; DROP pendiente script 18) |

> Validación lunes–sábado: no es CHECK, es lógica en `sp_Semanas_Create/Update` (script 15): `FechaInicio` debe ser lunes (`DATEPART(WEEKDAY)=2` con `DATEFIRST 7`), `FechaFin` sábado (`=7`) y `DATEDIFF(DAY)=5` (6 días lunes a sábado).

---

## 4. Índices

### Índices por Tabla

| Tabla | Índice | Columnas | Tipo | Propósito |
|-------|--------|----------|------|-----------|
| Usuarios | PK_Usuarios | Id | CLUSTERED | PK |
| Usuarios | UQ_Usuarios_Email | Email | NONCLUSTERED | UK + login |
| Usuarios | IX_Usuarios_Activo | Activo | NONCLUSTERED | Filtro activos |
| Usuarios | IX_Usuarios_RefreshToken | RefreshTokenHash | NONCLUSTERED (FILTERED) | Refresh token lookup |
| Semanas | PK_Semanas | Id | CLUSTERED | PK |
| Semanas | UQ_Semanas_NumeroAnio | NumeroSemana, Anio | NONCLUSTERED | UK + duplicados |
| Semanas | IX_Semanas_Fechas | FechaInicio, FechaFin | NONCLUSTERED | Rangos fecha |
| Semanas | IX_Semanas_Anio | Anio DESC | NONCLUSTERED | Orden por año |
| Semanas | IX_Semanas_Estado | Estado | NONCLUSTERED | Filtro estado |
| ObjetivosSemana | PK_ObjetivosSemana | Id | CLUSTERED | PK |
| ObjetivosSemana | FK_ObjetivosSemana_Semanas | SemanaId | NONCLUSTERED | FK join |
| InsumosSemana | PK_InsumosSemana | Id | CLUSTERED | PK |
| InsumosSemana | FK_InsumosSemana_Semanas | SemanaId | NONCLUSTERED | FK join |
| StockSemana | PK_StockSemana | Id | CLUSTERED | PK |
| StockSemana | FK_StockSemana_Semanas | SemanaId | NONCLUSTERED | FK join |
| Categorias | PK_Categorias | Id | CLUSTERED | PK |
| Categorias | IX_Categorias_Nombre | Nombre | NONCLUSTERED UNIQUE | UK |
| ProductosCatalogo | PK_ProductosCatalogo | Id | CLUSTERED | PK |
| ProductosCatalogo | UQ_ProductosCatalogo_Nombre | Nombre | NONCLUSTERED UNIQUE | UK |
| TiradosProducto | PK_TiradosProducto | Id | CLUSTERED | PK |
| TiradosProducto | IX_TiradosProducto_SemanaId_Nombre | SemanaId, Nombre | NONCLUSTERED UNIQUE | UK + join |
| TiradosProducto | IX_TiradosProducto_CategoriaId | CategoriaId | NONCLUSTERED | FK join |
| Tiradas | PK_Tiradas | Id | CLUSTERED | PK |
| Tiradas | IX_Tiradas_TiradoProductoId | TiradoProductoId | NONCLUSTERED | FK join |
| ~~ObjetivosSemana~~ | ~~PK/IX~~ | — | — | Histórico (eliminada del modelo) |
| ~~RegistroTacho/RegistrosTacho~~ | ~~PK/IX~~ | — | — | Histórico (DROP pendiente script 18) |

### Índices Faltantes (Futuro)
```sql
-- Para queries frecuentes del módulo tirado
CREATE NONCLUSTERED INDEX IX_Tiradas_Fecha ON Tiradas (Fecha);
```

---

## 5. Stored Procedures

### Patrones de Nombrado
- `sp_{Tabla}_GetAll` — Lista completa
- `sp_{Tabla}_GetById @Id` — Por ID
- `sp_{Tabla}_GetBy{Campo} @Campo` — Por campo específico
- `sp_{Tabla}_Create @Params...` — Inserción, retorna SCOPE_IDENTITY()
- `sp_{Tabla}_Update @Id, @Params...` — Actualización
- `sp_{Tabla}_Delete @Id` — Eliminación
- `sp_{Tabla}_Exists @Id` — Retorna 1/0
- `sp_{Tabla}_ExistsBy{Campo} @Campo` — Verifica duplicados

### Lista de SPs reales (script `15_create_all_stored_procedures.sql`)

> El script 15 crea **30 SPs exactos**: `sp_Semanas_*` (8), `sp_Insumos_*` (7, sin sufijo `Semana`), `sp_Stock_*` (7, sin sufijo `Semana`) y `sp_Usuarios_*` (8). **No crea** SPs de Objetivos, RegistroTacho, Tirado, Categorías, ProductosCatalogo ni Dashboard. Los SPs de semanas validan **lunes–sábado** (`FechaInicio` lunes, `FechaFin` sábado, 6 días). **Desactualización conocida:** los repos `InsumoRepository`/`StockRepository` llaman `sp_InsumosSemana_*` / `sp_StockSemana_*`, que no existen en el script 15; y los repos nuevos (`CategoriaRepository`, `TiradoProductoRepository`) usan **EF directo** sin SPs. No hay bulk de tirado ni sync en los scripts 14/15/16/18.

#### Usuarios (script 15, nombres exactos)
```sql
sp_Usuarios_GetAll
sp_Usuarios_GetById @Id
sp_Usuarios_GetByEmail @Email
sp_Usuarios_Create @Email, @PasswordHash, @Nombre, @Role
sp_Usuarios_Update @Id, @Email, @Nombre, @Role, @Activo
sp_Usuarios_Delete @Id  -- DELETE físico en script 15 (no soft)
sp_Usuarios_Exists @Id
sp_Usuarios_UpdateRefreshToken @Id, @RefreshTokenHash, @RefreshTokenExpiry
```

#### Semanas (script 15, nombres exactos; con validación lunes–sábado)
```sql
sp_Semanas_GetAll
sp_Semanas_GetById @Id
sp_Semanas_Create @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado
sp_Semanas_Update @Id, @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado
sp_Semanas_Delete @Id
sp_Semanas_Exists @Id
sp_Semanas_ExistsByNumeroAnio @NumeroSemana, @Anio, @ExcludeId
sp_Semanas_HasDependentData @Id  -- Revisa InsumosSemana, StockSemana y TiradoProductos (ya sin Objetivos/Registro)
```

#### Insumos (script 15: `sp_Insumos_*`, sin `Semana`)
```sql
sp_Insumos_GetAll
sp_Insumos_GetById @Id
sp_Insumos_GetBySemanaId @SemanaId
sp_Insumos_Create @SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida
sp_Insumos_Update @Id, @Nombre, @UnidadMedida, @CantidadRequerida, @CantidadConsumida, @StockDisponible
sp_Insumos_Delete @Id
sp_Insumos_Exists @Id
```

#### Stock (script 15: `sp_Stock_*`, sin `Semana`)
```sql
sp_Stock_GetAll
sp_Stock_GetById @Id
sp_Stock_GetBySemanaId @SemanaId
sp_Stock_Create @SemanaId, @Producto, @UnidadMedida, @StockInicial
sp_Stock_Update @Id, @Producto, @UnidadMedida, @StockInicial, @Entradas, @Salidas  -- Recalcula StockFinal
sp_Stock_Delete @Id
sp_Stock_Exists @Id
```

#### ProductosCatalogo (script 16, nombres exactos)
```sql
sp_ProductosCatalogo_GetAll
sp_ProductosCatalogo_GetById @Id
sp_ProductosCatalogo_Create @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Observaciones
sp_ProductosCatalogo_Update @Id, @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Activo, @Observaciones
sp_ProductosCatalogo_Delete @Id
sp_ProductosCatalogo_Exists @Id
sp_ProductosCatalogo_ExistsByNombre @Nombre, @ExcludeId
```

#### TiradoProductos (script 14 histórico — desactualizado; repos usan EF directo)
```sql
sp_TiradoProductos_GetAll
sp_TiradoProductos_GetById @Id
sp_TiradoProductos_GetBySemanaId @SemanaId  -- duplicado en el script
sp_TiradoProductos_Create @SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida, @Observaciones
sp_TiradoProductos_Update @Id, @Nombre, @UnidadMedida, @CantidadRequerida, @Observaciones
sp_TiradoProductos_Delete @Id
sp_TiradoProductos_Exists @Id
sp_TiradoProductos_ExistsByNombreSemana @Nombre, @SemanaId, @ExcludeId  -- duplicado en el script
sp_TiradoProductos_RegistrarTirada @Id, @CantidadTirada
sp_TiradoProductos_GetBySemanaConFiltro @SemanaId, @Filtro  -- 'totales' | 'faltantes' | 'sobrantes'
```

#### ObjetivosSemana / RegistroTacho / Dashboard (histórico — no existen en script 15)
```sql
-- Sin SPs en el script 15. Referencia histórica en Docs/DATABASE/STORED_PROCEDURES.md.
-- El script 18 elimina sp_RegistroTacho_* y sp_RegistrosTacho_* (DROP pendiente).
```

### Ejemplo SP Completo

```sql
-- sp_Semanas_Create (script 15 real: valida lunes-sábado, 6 días)
CREATE PROCEDURE sp_Semanas_Create
    @NumeroSemana INT,
    @Anio INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @Estado NVARCHAR(50) = 'Planificada'
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 7;

    IF EXISTS (SELECT 1 FROM Semanas WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio)
        THROW 50001, 'Ya existe una semana con ese número y año.', 1;

    IF @FechaFin <= @FechaInicio
        THROW 50002, 'La fecha fin debe ser posterior a la fecha inicio.', 1;

    IF DATEPART(WEEKDAY, @FechaInicio) <> 2 -- lunes
        THROW 50002, 'La fecha de inicio debe ser un lunes.', 1;

    IF DATEPART(WEEKDAY, @FechaFin) <> 7 -- sábado
        THROW 50003, 'La fecha de fin debe ser un sábado.', 1;

    IF DATEDIFF(DAY, @FechaInicio, @FechaFin) <> 5 -- 6 días lunes a sábado
        THROW 50004, 'La semana debe durar exactamente 6 días (lunes a sábado).', 1;

    INSERT INTO Semanas (NumeroSemana, Anio, FechaInicio, FechaFin, Estado)
    VALUES (@NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado);

    SELECT SCOPE_IDENTITY() AS Id;
END
GO
```

---

## 6. Datos de Referencia (Seed Data)

### Unidades de Medida
```sql
-- Tabla de referencia (futuro: catálogo separado)
-- Valores válidos para UnidadMedida:
-- kg, lt, un, m, m2, m3, t, glb, pza, rollo, bolsa, caja, pallet
```

### Estados Calidad
```sql
-- Valores válidos para EstadoCalidad en RegistroTacho:
-- 'Aprobado', 'Revision', 'Rechazado'
```

### Roles Usuario
```sql
-- Valores válidos para Role en Usuarios:
-- 'Admin', 'Supervisor', 'Operador'
```

### Estados Semana
```sql
-- Valores válidos para Estado en Semanas:
-- 'Planificada', 'EnProgreso', 'Completada', 'Cancelada'
```

---

## 7. Scripts de Creación

### Orden de Ejecución (scripts reales en `Docs/sql/`)
```bash
# 1. Migraciones EF Core (estrategia vigente para el modelo)
dotnet ef database update  # aplica 20261011162134_AddCategoriasYTiradas

# 2. Scripts SQL manuales
14_create_tirado_productos.sql       # histórico: tabla TiradoProductos + sp_TiradoProductos_* (desactualizado vs EF)
15_create_all_stored_procedures.sql  # 30 SPs: sp_Semanas_*, sp_Insumos_*, sp_Stock_*, sp_Usuarios_*
16_create_productos_catalogo.sql     # tabla ProductosCatalogo + 7 sp_ProductosCatalogo_*
18_drop_registro_tacho.sql           # PENDIENTE: elimina SPs RegistroTacho y tabla RegistrosTacho (no ejecutar aún)
```

### Scripts Disponibles en `Docs/sql/`
- `14_create_tirado_productos.sql` — Tabla `TiradoProductos` + SPs `sp_TiradoProductos_*` (histórico/desactualizado; el modelo EF usa `TiradosProducto` con `PedidoSemanal`, `CategoriaId`, `StockInicial`, `PromedioPorTacho`)
- `15_create_all_stored_procedures.sql` — 30 SPs reales (`sp_Semanas_*`, `sp_Insumos_*`, `sp_Stock_*`, `sp_Usuarios_*`); sin bulk ni sync
- `16_create_productos_catalogo.sql` — Tabla `ProductosCatalogo` + 7 SPs `sp_ProductosCatalogo_*`
- `18_drop_registro_tacho.sql` — DROP de SPs `sp_RegistroTacho_*`/`sp_RegistrosTacho_*` y tabla `RegistrosTacho` (pendiente)

---

## 8. Mantenimiento

### Backup Diario
```bash
# Script: backup-db.sh
BACKUP_DIR="/opt/sistema-industrial/backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="SistemaIndustrialDB"

/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "***" \
  -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_DIR/${DB_NAME}_$DATE.bak' WITH NOFORMAT, NOINIT, NAME = N'$DB_NAME-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Retención 30 días
find "$BACKUP_DIR" -name "${DB_NAME}_*.bak" -mtime +30 -delete
```

### Restaurar Backup
```bash
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "***" \
  -Q "RESTORE DATABASE [SistemaIndustrialDB] FROM DISK = N'/opt/sistema-industrial/backups/SistemaIndustrialDB_20261010_020000.bak' WITH REPLACE, RECOVERY"
```

### Estadísticas y Mantenimiento
```sql
-- Actualizar estadísticas (semanal)
EXEC sp_updatestats;

-- Rebuild índices fragmentados (mensual)
ALTER INDEX ALL ON Semanas REBUILD;
ALTER INDEX ALL ON InsumosSemana REBUILD;
ALTER INDEX ALL ON StockSemana REBUILD;
ALTER INDEX ALL ON Categorias REBUILD;
ALTER INDEX ALL ON ProductosCatalogo REBUILD;
ALTER INDEX ALL ON TiradosProducto REBUILD;
ALTER INDEX ALL ON Tiradas REBUILD;
ALTER INDEX ALL ON Usuarios REBUILD;
-- Histórico (eliminadas del modelo): ObjetivosSemana, RegistrosTacho (DROP pendiente script 18).

-- Verificar fragmentación
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 30
ORDER BY ips.avg_fragmentation_in_percent DESC;
```

### Limpieza de Datos Antiguos (Futuro)
```sql
-- Archivar semanas > 2 años (particionar o mover a tabla historial)
-- Soft delete usuarios inactivos > 1 año
UPDATE Usuarios SET Activo = 0 WHERE Activo = 1 AND FechaCreacion < DATEADD(YEAR, -1, GETDATE()) AND Id NOT IN (SELECT DISTINCT Id FROM ...);
```

---

## 9. Estadísticas

### Tamaño Estimado (Producción)

| Tabla | Registros Estimados/año | Tamaño/Registro | Total/año |
|-------|------------------------|-----------------|-----------|
| Usuarios | 50 | ~1 KB | 50 KB |
| Semanas | 52 | ~500 bytes | 26 KB |
| InsumosSemana | 500 | ~300 bytes | 150 KB |
| StockSemana | 300 | ~300 bytes | 90 KB |
| Categorias | 20 | ~200 bytes | 4 KB |
| ProductosCatalogo | 200 | ~300 bytes | 60 KB |
| TiradosProducto | 2,600 | ~300 bytes | 780 KB |
| Tiradas | 15,000 | ~300 bytes | 4.5 MB |
| ~~ObjetivosSemana / RegistroTacho~~ | — | — | Histórico (eliminadas del modelo) |
| **Total** | | | **~5.7 MB/año** |

### Crecimiento Proyectado
- **Año 1**: ~10 MB (con índices, logs)
- **Año 3**: ~30 MB
- **Año 5**: ~50 MB

**Recomendación**: SQL Server Express (10GB) suficiente para 10+ años.

---

## 10. Lecciones Aprendidas

### 1. SpInvoker + ToListAsync()
**Problema**: `FirstOrDefaultAsync()` sobre `FromSqlRaw` con `EXEC` falla en EF Core 9.
**Solución**: Siempre usar `ToListAsync()` + `.FirstOrDefault()` en memoria.

```csharp
// ❌ MAL - Falla en runtime
var semana = await _context.Semanas.FromSqlRaw("EXEC sp_Semanas_GetById @Id", id).FirstOrDefaultAsync();

// ✅ BIEN - Funciona correctamente
var list = await _context.Semanas.FromSqlRaw("EXEC sp_Semanas_GetById @Id", id).ToListAsync();
var semana = list.FirstOrDefault();
```

### 2. Decimal Precision en EF Core
**Problema**: Valores decimales truncados al guardar.
**Solución**: Configurar precision/scale global en `OnModelCreating`:

```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
```

### 3. Cascade Delete + Soft Delete Conflict
**Problema**: `ON DELETE CASCADE` en FKs vs Soft Delete en Usuarios.
**Solución**: Usuarios NO tiene FKs hacia otras tablas (solo referenciado en auditoría futura). Semanas usa CASCADE para limpieza automática de datos dependientes.

### 4. Refresh Token Storage
**Problema**: Buscar usuario por refresh token requiere escanear tabla o índice en hash.
**Solución Actual**: Índice filtrado en `RefreshTokenHash WHERE NOT NULL`.
**Futuro**: Tabla separada `RefreshTokens (Id, UserId, TokenHash, Expiry, Revoked)` para mejor performance y revocación granular.

### 5. Check Constraint StockFinal
**Problema**: `CK_Stock_Final` con fórmula calculada falla en INSERT si no se proporciona StockFinal.
**Solución**: SP de CREATE calcula `StockFinal = StockInicial + Entradas - Salidas` antes de INSERT. En UPDATE, BR recalcula automáticamente.

### 6. Script 15 desactualizado + repos nuevos con EF directo
**Estado**: el script 15 solo cubre Semanas/Insumos/Stock/Usuarios y usa `sp_Insumos_*` / `sp_Stock_*`, mientras `InsumoRepository`/`StockRepository` llaman `sp_InsumosSemana_*` / `sp_StockSemana_*`. Los repos nuevos (`CategoriaRepository`, `TiradoProductoRepository` con `Tiradas`) usan EF directo (`DbContext` + `Include` + `SaveChangesAsync`) sin SPs. Documentar tal cual: no inventar SPs de categorías/tiradas y mantener el script 14 como histórico.

---

## Referencias Cruzadas

| Documento | Ubicación |
|-----------|-----------|
| Esquema BD Detallado | `Docs/backend/02-base-de-datos/01-esquema-bd.md` |
| Scripts DDL/SPs reales | `Docs/sql/14_create_tirado_productos.sql`, `15_create_all_stored_procedures.sql`, `16_create_productos_catalogo.sql`, `18_drop_registro_tacho.sql` |
| Migraciones | `Docs/backend/02-base-de-datos/03-migraciones.md` |
| Modelos C# | `Docs/backend/03-modelos/01-entidades-dominio.md` |
| Repositorios | `Docs/backend/04-servicios/01-capa-servicios.md` |