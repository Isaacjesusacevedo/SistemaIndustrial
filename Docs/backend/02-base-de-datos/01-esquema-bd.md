# Esquema de Base de Datos

> Modelo vigente (`AppDbContext.cs`): 8 DbSets — `Usuarios`, `Semanas`, `InsumosSemana`, `StockSemana`, `Categorias`, `ProductosCatalogo`, `TiradosProducto`, `Tiradas`. `ObjetivosSemana` y `RegistroTacho` están eliminados del modelo (sin `DbSet`); el DROP de `RegistrosTacho` está pendiente en `18_drop_registro_tacho.sql`.

## Diagrama Entidad-Relación

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

    OBJETIVOS_SEMANA {
        int Id PK
        int SemanaId FK
        string Descripcion
        decimal Meta
        decimal Logrado
        decimal Porcentaje
        datetime FechaCreacion
    }

    %% HISTÓRICO — ObjetivosSemana eliminada del DbContext (sin DbSet, sin Models/ObjetivoSemana.cs)

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

    REGISTRO_TACHO {
        int Id PK
        int SemanaId FK
        int Turno
        string Operador
        decimal CantidadProducida
        decimal CantidadRechazada
        string EstadoCalidad
        datetime FechaRegistro
        datetime FechaCreacion
    }

    %% HISTÓRICO — RegistroTacho eliminado del DbContext (DROP pendiente script 18)

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
        datetime FechaCreacion
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
        datetime FechaCreacion
    }

    TIRADAS {
        int Id PK
        int TiradoProductoId FK
        datetime Fecha
        decimal Cantidad
        datetime FechaCreacion
    }
```

## Tablas y Columnas

### Usuarios
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| Email | nvarchar(255) | NO | | UK, índice único |
| PasswordHash | nvarchar(255) | NO | | BCrypt hash |
| Nombre | nvarchar(100) | NO | | |
| Role | nvarchar(50) | NO | 'Operador' | 'Admin', 'Supervisor', 'Operador' |
| RefreshTokenHash | nvarchar(255) | YES | NULL | Hash del refresh token |
| RefreshTokenExpiry | datetime | YES | NULL | Expiración refresh token |
| Activo | bit | NO | 1 | Soft delete |
| FechaCreacion | datetime | NO | GETDATE() | |

### Semanas
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| NumeroSemana | int | NO | | 1-53 |
| Anio | int | NO | | Ej. 2026 |
| FechaInicio | datetime | NO | | Lunes de la semana |
| FechaFin | datetime | NO | | Domingo de la semana |
| Estado | nvarchar(20) | NO | 'Planificada' | 'Planificada', 'EnProgreso', 'Completada', 'Cancelada' |
| FechaCreacion | datetime | NO | GETDATE() | |

**Índices**: UNIQUE (NumeroSemana, Anio)

### ObjetivosSemana (ELIMINADA del modelo — histórico)
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| SemanaId | int | NO | | FK → Semanas(Id) |
| Descripcion | nvarchar(500) | NO | | |
| Meta | decimal(18,2) | NO | | Objetivo numérico |
| Logrado | decimal(18,2) | NO | 0 | Valor actual |
| Porcentaje | decimal(5,2) | NO | 0 | Calculado: (Logrado/Meta)*100 |
| FechaCreacion | datetime | NO | GETDATE() | |

**FK**: SemanaId → Semanas(Id) ON DELETE CASCADE

> Sin `DbSet`; no existe `Models/ObjetivoSemana.cs`. Solo migración/snapshot histórico.

### InsumosSemana
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| SemanaId | int | NO | | FK → Semanas(Id) |
| Nombre | nvarchar(200) | NO | | Nombre del insumo |
| UnidadMedida | nvarchar(50) | NO | | kg, lt, un, m2, etc. |
| CantidadRequerida | decimal(18,2) | NO | | Planificado |
| CantidadConsumida | decimal(18,2) | NO | 0 | Real consumido |
| StockDisponible | decimal(18,2) | NO | 0 | Stock actual |
| FechaCreacion | datetime | NO | GETDATE() | |

**FK**: SemanaId → Semanas(Id) ON DELETE CASCADE

### StockSemana
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| SemanaId | int | NO | | FK → Semanas(Id) |
| Producto | nvarchar(200) | NO | | Producto terminado |
| UnidadMedida | nvarchar(50) | NO | | |
| StockInicial | decimal(18,2) | NO | | Inicio de semana |
| Entradas | decimal(18,2) | NO | 0 | Producción + ingresos |
| Salidas | decimal(18,2) | NO | 0 | Ventas + consumos |
| StockFinal | decimal(18,2) | NO | | Calculado: Inicial + Entradas - Salidas |
| FechaCreacion | datetime | NO | GETDATE() | |

**FK**: SemanaId → Semanas(Id) ON DELETE CASCADE

### RegistroTacho (ELIMINADO del modelo — histórico, DROP pendiente)
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| SemanaId | int | NO | | FK → Semanas(Id) |
| Turno | int | NO | | 1=Mañana, 2=Tarde, 3=Noche |
| Operador | nvarchar(100) | NO | | Nombre operador |
| CantidadProducida | decimal(18,2) | NO | | Unidades buenas |
| CantidadRechazada | decimal(18,2) | NO | 0 | Unidades con defecto |
| EstadoCalidad | nvarchar(20) | NO | 'Aprobado' | 'Aprobado', 'Revisión', 'Rechazado' |
| FechaRegistro | datetime | NO | | Timestamp del registro |
| FechaCreacion | datetime | NO | GETDATE() | |

**FK**: SemanaId → Semanas(Id) ON DELETE CASCADE

> Sin `DbSet` (`Semana` ya no expone `RegistrosTacho`); DROP pendiente en `18_drop_registro_tacho.sql` (no ejecutar aún).

### Categorias
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| Nombre | nvarchar(450) | NO | | UK |
| Orden | int | NO | 0 | Orden de presentación |
| Activa | bit | NO | 1 | Activa/inactiva |

**Relación**: `Categoria 1-* TiradosProducto` (`CategoriaId` nullable, `ON DELETE SET NULL`)

### ProductosCatalogo
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| Nombre | nvarchar(200) | NO | | UK |
| UnidadMedida | nvarchar(50) | NO | | |
| PedidoSemanalAproximado | decimal(18,2) | NO | 0 | Estimación semanal |
| PromedioPorTacho | decimal(18,2) | NO | 0 | Rendimiento por tacho |
| Activo | bit | NO | 1 | |
| Observaciones | nvarchar(500) | YES | NULL | |
| FechaCreacion | datetime | NO | GETDATE() | |
| FechaActualizacion | datetime | YES | NULL | |

**Nota**: catálogo general sin `SemanaId` (script 16; sin migración EF).

### TiradosProducto (entidad `TiradoProducto`)
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| SemanaId | int | NO | | FK → Semanas(Id) CASCADE |
| CategoriaId | int | YES | NULL | FK → Categorias(Id) SET NULL |
| Nombre | nvarchar(450) | NO | | UK con SemanaId |
| UnidadMedida | nvarchar(max) | NO | | |
| PedidoSemanal | decimal(18,2) | NO | | Renombrado desde `CantidadRequerida` |
| StockInicial | decimal(18,2) | NO | | |
| PromedioPorTacho | decimal(18,2) | NO | | |
| CantidadTirada | decimal(18,2) | NO | 0 | Acumulada |
| Stock | decimal(18,2) | NO | 0 | |
| Observaciones | nvarchar(max) | YES | NULL | |
| FechaCreacion | datetime | NO | GETDATE() | |
| FechaActualizacion | datetime | YES | NULL | |

**UK**: (SemanaId, Nombre)

### Tiradas
| Columna | Tipo | Nullable | Default | Descripción |
|---------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY(1,1) | PK |
| TiradoProductoId | int | NO | | FK → TiradosProducto CASCADE |
| Fecha | datetime | NO | | Fecha de la tirada |
| Cantidad | decimal(18,2) | NO | | |
| Observaciones | nvarchar(max) | YES | NULL | |
| FechaCreacion | datetime | NO | GETDATE() | |

**Nota**: repos nuevos con EF directo, sin SPs.

---

## Índices y Constraints

| Tabla | Índice/Constraint | Columnas | Tipo |
|-------|-------------------|----------|------|
| Usuarios | PK_Usuarios | Id | PRIMARY KEY |
| Usuarios | UQ_Usuarios_Email | Email | UNIQUE |
| Usuarios | IX_Usuarios_Activo | Activo | NONCLUSTERED |
| Semanas | PK_Semanas | Id | PRIMARY KEY |
| Semanas | UQ_Semanas_NumeroAnio | NumeroSemana, Anio | UNIQUE |
| Semanas | IX_Semanas_Fechas | FechaInicio, FechaFin | NONCLUSTERED |
| ObjetivosSemana | PK_ObjetivosSemana | Id | PRIMARY KEY |
| ObjetivosSemana | FK_ObjetivosSemana_Semanas | SemanaId | FOREIGN KEY |
| ObjetivosSemana | IX_ObjetivosSemana_SemanaId | SemanaId | NONCLUSTERED |
| ~~Nota~~ | — | — | HISTÓRICO (eliminada del modelo; solo migración/snapshot) |
| InsumosSemana | PK_InsumosSemana | Id | PRIMARY KEY |
| InsumosSemana | FK_InsumosSemana_Semanas | SemanaId | FOREIGN KEY |
| InsumosSemana | IX_InsumosSemana_SemanaId | SemanaId | NONCLUSTERED |
| StockSemana | PK_StockSemana | Id | PRIMARY KEY |
| StockSemana | FK_StockSemana_Semanas | SemanaId | FOREIGN KEY |
| StockSemana | IX_StockSemana_SemanaId | SemanaId | NONCLUSTERED |
| Categorias | PK_Categorias | Id | PRIMARY KEY |
| Categorias | UQ (IX_Categorias_Nombre) | Nombre | UNIQUE |
| ProductosCatalogo | UQ_ProductosCatalogo_Nombre | Nombre | UNIQUE |
| TiradosProducto | PK_TiradosProducto | Id | PRIMARY KEY |
| TiradosProducto | IX_TiradosProducto_SemanaId_Nombre | SemanaId, Nombre | UNIQUE |
| TiradosProducto | IX_TiradosProducto_CategoriaId | CategoriaId | NONCLUSTERED |
| Tiradas | PK_Tiradas | Id | PRIMARY KEY |
| Tiradas | IX_Tiradas_TiradoProductoId | TiradoProductoId | NONCLUSTERED |
| ~~ObjetivosSemana~~ | — | — | HISTÓRICO (eliminada del modelo) |
| ~~RegistroTacho~~ | — | — | HISTÓRICO (DROP pendiente script 18) |

---

## Stored Procedures Principales (nombres exactos del script 15 + 16; 14 histórico)

> Script 15: 30 SPs (`sp_Semanas_*`, `sp_Insumos_*`, `sp_Stock_*`, `sp_Usuarios_*`) con validación lunes–sábado en semanas. Script 16: 7 `sp_ProductosCatalogo_*`. Script 14 (`sp_TiradoProductos_*`) desactualizado; `CategoriaRepository` y `TiradoProductoRepository` usan EF directo. Sin SPs de Objetivos/Registro/Dashboard/Categorías/Tiradas en el script 15; sin bulk ni sync.

### Usuarios (script 15)
- `sp_Usuarios_GetAll` — Lista todos
- `sp_Usuarios_GetById @Id` — Por ID (sin PasswordHash)
- `sp_Usuarios_GetByEmail @Email` — Por email, con PasswordHash + RefreshToken (login)
- `sp_Usuarios_Create @Email, @PasswordHash, @Nombre, @Role` — Inserta, retorna ID
- `sp_Usuarios_Update @Id, @Email, @Nombre, @Role, @Activo` — Actualiza
- `sp_Usuarios_UpdateRefreshToken @Id, @RefreshTokenHash, @RefreshTokenExpiry` — Refresh token
- `sp_Usuarios_Delete @Id` — DELETE físico en script 15
- `sp_Usuarios_Exists @Id` — Verifica existencia (1/0)
- Nota: `sp_Usuarios_ExistsByEmail` lo usa el repo pero no existe en el script 15.

### Semanas (script 15; validación lunes–sábado)
- `sp_Semanas_GetAll` — Lista todas ordenadas por Año desc, Semana desc
- `sp_Semanas_GetById @Id` — Por ID
- `sp_Semanas_Create @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado` — Inserta (valida inicio lunes, fin sábado, 6 días)
- `sp_Semanas_Update @Id, @NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado` — Actualiza (misma validación)
- `sp_Semanas_Delete @Id` — Elimina (validar dependencias en BR)
- `sp_Semanas_Exists @Id` — Verifica existencia
- `sp_Semanas_ExistsByNumeroAnio @NumeroSemana, @Anio, @ExcludeId` — Duplicados
- `sp_Semanas_HasDependentData @Id` — Verifica InsumosSemana, StockSemana y TiradoProductos
- No existen en el script 15: `GetByNumeroAnio`, `GetByAnio`, `UpdateEstado`.

### ObjetivosSemana (ELIMINADA — sin SPs en script 15)
- Sin `sp_ObjetivosSemana_*` en el script 15. Tabla eliminada del modelo.

### Insumos (script 15: `sp_Insumos_*`; el repo llama `sp_InsumosSemana_*`, inexistentes en el script)
- `sp_Insumos_GetAll` — Todos
- `sp_Insumos_GetById @Id` — Por ID
- `sp_Insumos_GetBySemanaId @SemanaId` — Por semana
- `sp_Insumos_Create @SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida` — Inserta
- `sp_Insumos_Update @Id, @Nombre, @UnidadMedida, @CantidadRequerida, @CantidadConsumida, @StockDisponible` — Actualiza
- `sp_Insumos_Delete @Id` — Elimina
- `sp_Insumos_Exists @Id` — Verifica

### Stock (script 15: `sp_Stock_*`; el repo llama `sp_StockSemana_*`, inexistentes en el script)
- `sp_Stock_GetAll` — Todos
- `sp_Stock_GetById @Id` — Por ID
- `sp_Stock_GetBySemanaId @SemanaId` — Por semana
- `sp_Stock_Create @SemanaId, @Producto, @UnidadMedida, @StockInicial` — Inserta
- `sp_Stock_Update @Id, @Producto, @UnidadMedida, @StockInicial, @Entradas, @Salidas` — Actualiza (recalcula StockFinal)
- `sp_Stock_Delete @Id` — Elimina
- `sp_Stock_Exists @Id` — Verifica

### RegistroTacho (ELIMINADO — DROP pendiente script 18)
- Sin SPs en el script 15. El script 18 elimina `sp_RegistroTacho_*` y `sp_RegistrosTacho_*` + tabla `RegistrosTacho` (pendiente, no ejecutar aún).

### ProductosCatalogo (script 16)
- `sp_ProductosCatalogo_GetAll`, `sp_ProductosCatalogo_GetById @Id`, `sp_ProductosCatalogo_Create`, `sp_ProductosCatalogo_Update`, `sp_ProductosCatalogo_Delete @Id`, `sp_ProductosCatalogo_Exists @Id`, `sp_ProductosCatalogo_ExistsByNombre @Nombre, @ExcludeId`

### TiradoProductos (script 14 histórico — EF directo en código)
- `sp_TiradoProductos_GetAll/GetById/GetBySemanaId/Create/Update/Delete/Exists/ExistsByNombreSemana/RegistrarTirada/GetBySemanaConFiltro` (script con duplicados; desactualizado vs `TiradosProducto`/`Tiradas`). Sin SPs de Categorías ni Tiradas; sin bulk ni sync.

### Dashboard (sin SPs en script 15)
- Sin `sp_Dashboard_*` en el script 15 (histórico).