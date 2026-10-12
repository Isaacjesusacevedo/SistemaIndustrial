# Tablas de Base de Datos

> Modelo vigente (`AppDbContext.cs`): 8 tablas — `Usuarios`, `Semanas`, `InsumosSemana`, `StockSemana`, `Categorias`, `ProductosCatalogo`, `TiradosProducto`, `Tiradas`. `ObjetivosSemana` y `RegistroTacho` están eliminados del modelo (histórico; DROP de `RegistrosTacho` pendiente en `18_drop_registro_tacho.sql`).

## Resumen

| Tabla | Registros/año | Tamaño estimado | Propósito |
|-------|---------------|-----------------|-----------|
| Usuarios | 50 | 50 KB | Autenticación y autorización |
| Semanas | 52 | 26 KB | Entidad principal temporal |
| InsumosSemana | 500 | 150 KB | Materiales requeridos/consumidos |
| StockSemana | 300 | 90 KB | Inventario productos terminados |
| Categorias | 20 | 4 KB | Catálogo de categorías (clasifica TiradosProducto) |
| ProductosCatalogo | 200 | 60 KB | Catálogo general sin SemanaId (script 16; sin migración EF) |
| TiradosProducto | 2,600 | 780 KB | Control por producto/semana (`PedidoSemanal`, `CategoriaId`, `StockInicial`, `PromedioPorTacho`) |
| Tiradas | 15,000 | 4.5 MB | Detalle de tiradas por producto (EF directo, sin SPs) |
| ~~ObjetivosSemana~~ | — | — | ELIMINADA del modelo (solo migración/snapshot histórico) |
| ~~RegistroTacho/RegistrosTacho~~ | — | — | ELIMINADO del modelo (DROP pendiente script 18) |

---

## Usuarios

```sql
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Operador',
    RefreshTokenHash NVARCHAR(255) NULL,
    RefreshTokenExpiry DATETIME NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE INDEX IX_Usuarios_Activo ON Usuarios(Activo);
CREATE INDEX IX_Usuarios_RefreshToken ON Usuarios(RefreshTokenHash) WHERE RefreshTokenHash IS NOT NULL;
```

**Roles válidos:** `Admin`, `Supervisor`, `Operador`

---

## Semanas

```sql
CREATE TABLE Semanas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroSemana INT NOT NULL,
    Anio INT NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL,
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Planificada',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT UQ_Semanas_NumeroAnio UNIQUE (NumeroSemana, Anio),
    CONSTRAINT CK_Semanas_Numero CHECK (NumeroSemana BETWEEN 1 AND 53),
    CONSTRAINT CK_Semanas_Anio CHECK (Anio BETWEEN 2020 AND 2030),
    CONSTRAINT CK_Semanas_Estado CHECK (Estado IN ('Planificada','EnProgreso','Completada','Cancelada')),
    CONSTRAINT CK_Semanas_Fechas CHECK (FechaFin > FechaInicio)
);

CREATE INDEX IX_Semanas_Fechas ON Semanas(FechaInicio, FechaFin);
CREATE INDEX IX_Semanas_Anio ON Semanas(Anio DESC);
CREATE INDEX IX_Semanas_Estado ON Semanas(Estado);
```

**Estados:** `Planificada` | `EnProgreso` | `Completada` | `Cancelada`

---

## ObjetivosSemana (ELIMINADA del modelo — histórico)

> Sin `DbSet` en `AppDbContext`; no existe `Models/ObjetivoSemana.cs`. Solo queda en la migración `20261011162134` y el snapshot. DDL abajo como referencia histórica.

```sql
CREATE TABLE ObjetivosSemana (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SemanaId INT NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    Meta DECIMAL(18,2) NOT NULL,
    Logrado DECIMAL(18,2) NOT NULL DEFAULT 0,
    Porcentaje DECIMAL(5,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_ObjetivosSemana_Semanas FOREIGN KEY (SemanaId) REFERENCES Semanas(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Objetivos_Meta CHECK (Meta > 0),
    CONSTRAINT CK_Objetivos_Logrado CHECK (Logrado >= 0),
    CONSTRAINT CK_Objetivos_Porcentaje CHECK (Porcentaje BETWEEN 0 AND 100)
);

CREATE INDEX IX_ObjetivosSemana_SemanaId ON ObjetivosSemana(SemanaId);
```

**Regla:** `Porcentaje = (Logrado / Meta) * 100` (calculado en BR)

---

## InsumosSemana

```sql
CREATE TABLE InsumosSemana (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SemanaId INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    UnidadMedida NVARCHAR(50) NOT NULL,
    CantidadRequerida DECIMAL(18,2) NOT NULL,
    CantidadConsumida DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockDisponible DECIMAL(18,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_InsumosSemana_Semanas FOREIGN KEY (SemanaId) REFERENCES Semanas(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Insumos_Cantidades CHECK (CantidadRequerida >= 0 AND CantidadConsumida >= 0 AND StockDisponible >= 0)
);

CREATE INDEX IX_InsumosSemana_SemanaId ON InsumosSemana(SemanaId);
```

**Unidades típicas:** `kg`, `lt`, `un`, `m`, `m2`, `m3`, `t`, `pza`

---

## StockSemana

```sql
CREATE TABLE StockSemana (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SemanaId INT NOT NULL,
    Producto NVARCHAR(200) NOT NULL,
    UnidadMedida NVARCHAR(50) NOT NULL,
    StockInicial DECIMAL(18,2) NOT NULL,
    Entradas DECIMAL(18,2) NOT NULL DEFAULT 0,
    Salidas DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockFinal DECIMAL(18,2) NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_StockSemana_Semanas FOREIGN KEY (SemanaId) REFERENCES Semanas(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Stock_Valores CHECK (StockInicial >= 0 AND Entradas >= 0 AND Salidas >= 0),
    CONSTRAINT CK_Stock_Final CHECK (StockFinal = StockInicial + Entradas - Salidas)
);

CREATE INDEX IX_StockSemana_SemanaId ON StockSemana(SemanaId);
```

**Regla:** `StockFinal = StockInicial + Entradas - Salidas` (calculado en BR)

---

## RegistroTacho (ELIMINADO del modelo — histórico, DROP pendiente)

> Sin `DbSet` en `AppDbContext` (`Semana` ya no expone `RegistrosTacho`). `Models/RegistroTacho.cs` aún existe sin mapear; migración/snapshot crean `RegistrosTacho`. El script `18_drop_registro_tacho.sql` (pendiente, no ejecutar aún) elimina SPs y tabla. DDL abajo como referencia histórica.

```sql
CREATE TABLE RegistroTacho (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SemanaId INT NOT NULL,
    Turno INT NOT NULL,
    Operador NVARCHAR(100) NOT NULL,
    CantidadProducida DECIMAL(18,2) NOT NULL,
    CantidadRechazada DECIMAL(18,2) NOT NULL DEFAULT 0,
    EstadoCalidad NVARCHAR(20) NOT NULL DEFAULT 'Aprobado',
    FechaRegistro DATETIME NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_RegistroTacho_Semanas FOREIGN KEY (SemanaId) REFERENCES Semanas(Id) ON DELETE CASCADE,
    CONSTRAINT CK_RegistroTacho_Turno CHECK (Turno IN (1,2,3)),
    CONSTRAINT CK_RegistroTacho_Cantidades CHECK (CantidadProducida >= 0 AND CantidadRechazada >= 0),
    CONSTRAINT CK_RegistroTacho_EstadoCalidad CHECK (EstadoCalidad IN ('Aprobado','Revision','Rechazado'))
);

CREATE INDEX IX_RegistroTacho_SemanaId ON RegistroTacho(SemanaId);
CREATE INDEX IX_RegistroTacho_Turno ON RegistroTacho(Turno);
CREATE INDEX IX_RegistroTacho_FechaRegistro ON RegistroTacho(FechaRegistro);
CREATE INDEX IX_RegistroTacho_Operador ON RegistroTacho(Operador);
```

**Turnos:** 1=Mañana, 2=Tarde, 3=Noche  
**Estados Calidad:** `Aprobado` | `Revision` | `Rechazado`

---

## TiradosProducto (entidad `TiradoProducto` — modelo vigente)

> Tabla EF: `TiradosProducto`. El script histórico `14_create_tirado_productos.sql` crea `TiradoProductos` con `CantidadRequerida` y sin `CategoriaId`/`StockInicial`/`PromedioPorTacho`, por lo que está desactualizado. Campos exactos (`Models/TiradoProducto.cs`): `Id`, `SemanaId` (FK CASCADE), `CategoriaId` (FK nullable → `Categorias`, `SetNull`), `Nombre`, `UnidadMedida`, `PedidoSemanal` (renombrado desde `CantidadRequerida`), `StockInicial`, `PromedioPorTacho`, `CantidadTirada` (default 0), `Stock` (default 0), `Observaciones` (null), `FechaCreacion` (GETDATE()), `FechaActualizacion` (null). UK `(SemanaId, Nombre)`.

**Estados (calculados en BR, no en BD):** `Faltante = MAX(0, PedidoSemanal - CantidadTirada)`, `Sobrante = MAX(0, CantidadTirada - PedidoSemanal)`, `Porcentaje = (CantidadTirada / PedidoSemanal) * 100`.

---

## Categorias

```sql
-- Migración 20261011162134_AddCategoriasYTiradas (modelo vigente)
-- Categorias (Id IDENTITY PK, Nombre NVARCHAR(450) UNIQUE, Orden DEFAULT 0, Activa DEFAULT 1)
-- FK: TiradosProducto.CategoriaId → Categorias(Id) ON DELETE SET NULL
```

**Columnas:** `Id` int PK · `Nombre` nvarchar(450) UK · `Orden` int default 0 · `Activa` bit default 1.

---

## ProductosCatalogo

```sql
-- 16_create_productos_catalogo.sql (catálogo general, sin SemanaId)
-- ProductosCatalogo (Id, Nombre UNIQUE, UnidadMedida, PedidoSemanalAproximado DEFAULT 0,
-- PromedioPorTacho DEFAULT 0, Activo DEFAULT 1, Observaciones NULL,
-- FechaCreacion DEFAULT GETDATE(), FechaActualizacion NULL)
```

**Columnas:** `Id` · `Nombre` UK · `UnidadMedida` · `PedidoSemanalAproximado` · `PromedioPorTacho` · `Activo` · `Observaciones` (null) · `FechaCreacion` · `FechaActualizacion` (null).

---

## Tiradas

```sql
-- Migración 20261011162134_AddCategoriasYTiradas (modelo vigente)
-- Tiradas (Id IDENTITY PK, TiradoProductoId FK CASCADE, Fecha, Cantidad DECIMAL(18,2),
-- Observaciones NULL, FechaCreacion DEFAULT GETDATE())
```

**Columnas:** `Id` · `TiradoProductoId` FK → TiradosProducto (CASCADE) · `Fecha` · `Cantidad` · `Observaciones` (null) · `FechaCreacion`. Repos con EF directo, sin SPs.