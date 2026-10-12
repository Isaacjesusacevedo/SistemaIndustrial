# Script de Creación de Base de Datos

> **Nota**: este documento mezcla DDL histórico manual con el estado real. Fuentes de verdad: `AppDbContext.cs` (8 DbSets; sin Objetivos/Registro), migración `20261011162134_AddCategoriasYTiradas` y scripts `Docs/sql/14/15/16/18`. Los archivos `.sql` ejecutables se encuentran en `Docs/sql/`.

## 1. Crear Base de Datos

```sql
-- Ejecutar en master
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SistemaIndustrialDB')
BEGIN
    CREATE DATABASE SistemaIndustrialDB
    COLLATE Latin1_General_CI_AS;
END
GO

USE SistemaIndustrialDB;
GO
```

## 2. Tabla Usuarios

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
GO

CREATE NONCLUSTERED INDEX IX_Usuarios_Activo ON Usuarios (Activo);
GO
```

## 3. Tabla Semanas

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
    CONSTRAINT CK_Semanas_Estado CHECK (Estado IN ('Planificada','EnProgreso','Completada','Cancelada')),
    CONSTRAINT CK_Semanas_Fechas CHECK (FechaFin > FechaInicio)
);
GO

CREATE NONCLUSTERED INDEX IX_Semanas_Fechas ON Semanas (FechaInicio, FechaFin);
CREATE NONCLUSTERED INDEX IX_Semanas_Anio ON Semanas (Anio DESC);
GO
```

## 4. Tabla ObjetivosSemana (ELIMINADA del modelo — histórico)

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
GO

CREATE NONCLUSTERED INDEX IX_ObjetivosSemana_SemanaId ON ObjetivosSemana (SemanaId);
GO
```

## 5. Tabla InsumosSemana

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
GO

CREATE NONCLUSTERED INDEX IX_InsumosSemana_SemanaId ON InsumosSemana (SemanaId);
GO
```

## 6. Tabla StockSemana

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
GO

CREATE NONCLUSTERED INDEX IX_StockSemana_SemanaId ON StockSemana (SemanaId);
GO
```

## 7. Tabla RegistroTacho (ELIMINADA del modelo — histórico, DROP pendiente)

> Sin `DbSet`; el script `18_drop_registro_tacho.sql` elimina SPs y tabla `RegistrosTacho` (pendiente, no ejecutar aún).

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
GO

CREATE NONCLUSTERED INDEX IX_RegistroTacho_SemanaId ON RegistroTacho (SemanaId);
CREATE NONCLUSTERED INDEX IX_RegistroTacho_Turno ON RegistroTacho (Turno);
CREATE NONCLUSTERED INDEX IX_RegistroTacho_FechaRegistro ON RegistroTacho (FechaRegistro);
GO
```

## 8. Tablas vigentes no cubiertas arriba (resumen real)

```sql
-- Categorias: migración 20261011162134 (Id, Nombre UNIQUE, Orden DEFAULT 0, Activa DEFAULT 1)
-- ProductosCatalogo: 16_create_productos_catalogo.sql (Id, Nombre UNIQUE, UnidadMedida,
--   PedidoSemanalAproximado DEFAULT 0, PromedioPorTacho DEFAULT 0, Activo DEFAULT 1,
--   Observaciones NULL, FechaCreacion GETDATE(), FechaActualizacion NULL)
-- TiradosProducto: migración 20261011162134 (Id, SemanaId FK CASCADE, CategoriaId FK NULL SET NULL,
--   Nombre, UnidadMedida, PedidoSemanal, StockInicial, PromedioPorTacho,
--   CantidadTirada DEFAULT 0, Stock DEFAULT 0, Observaciones NULL,
--   FechaCreacion GETDATE(), FechaActualizacion NULL, UNIQUE(SemanaId, Nombre))
-- Tiradas: migración 20261011162134 (Id, TiradoProductoId FK CASCADE, Fecha, Cantidad,
--   Observaciones NULL, FechaCreacion GETDATE())
```

## 9. Usuario Inicial (Solo Desarrollo)

```sql
-- Password: Industrial2026 (BCrypt workFactor 12)
-- Hash generado con: BCryptNet.HashPassword("Industrial2026", 12)
INSERT INTO Usuarios (Email, PasswordHash, Nombre, Role, Activo)
VALUES (
    'admin@industrial.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', -- Industrial2026
    'Administrador',
    'Admin',
    1
);
GO
```

## 10. Stored Procedures (Resumen real)

Ver scripts reales: `15_create_all_stored_procedures.sql` (30 SPs: `sp_Semanas_*`, `sp_Insumos_*`, `sp_Stock_*`, `sp_Usuarios_*`), `16_create_productos_catalogo.sql` (7 `sp_ProductosCatalogo_*`), `14_create_tirado_productos.sql` (`sp_TiradoProductos_*`, histórico/desactualizado) y `18_drop_registro_tacho.sql` (DROP pendiente). Sin SPs de Objetivos/Registro/Dashboard/Categorías/Tiradas en el script 15; sin bulk ni sync. Los repos nuevos (`CategoriaRepository`, `TiradoProductoRepository`) usan EF directo.

### Patrones de Nombrado SPs
- `sp_{Tabla}_GetAll` — Lista completa
- `sp_{Tabla}_GetById @Id` — Por ID
- `sp_{Tabla}_GetBy{Campo} @Campo` — Por campo específico
- `sp_{Tabla}_Create @Params...` — Inserción, retorna SCOPE_IDENTITY()
- `sp_{Tabla}_Update @Id, @Params...` — Actualización
- `sp_{Tabla}_Delete @Id` — Eliminación
- `sp_{Tabla}_Exists @Id` — Retorna 1/0
- `sp_{Tabla}_ExistsBy{Campo} @Campo` — Verifica duplicados

### Ejemplo: sp_Semanas_Create (script 15 real: valida lunes–sábado)
```sql
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

### Ejemplo: sp_Usuarios_GetByEmail (Login; script 15 real)
```sql
CREATE PROCEDURE sp_Usuarios_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Email, PasswordHash, Nombre, Role, Activo, FechaCreacion,
           RefreshTokenHash, RefreshTokenExpiry
    FROM Usuarios
    WHERE Email = @Email;
END
GO
```

---

## Orden de Ejecución (real)

1. `dotnet ef database update` — Migración `20261011162134_AddCategoriasYTiradas` (Categorias, Semanas, Usuarios, InsumosSemana, ObjetivosSemana*, RegistrosTacho*, StockSemana, TiradosProducto, Tiradas; *histórico, eliminados del DbContext; sin ProductosCatalogo)
2. `Docs/sql/16_create_productos_catalogo.sql` — Tabla `ProductosCatalogo` + 7 SPs
3. `Docs/sql/15_create_all_stored_procedures.sql` — 30 SPs (`sp_Semanas_*`, `sp_Insumos_*`, `sp_Stock_*`, `sp_Usuarios_*`)
4. `Docs/sql/14_create_tirado_productos.sql` — Histórico (tabla `TiradoProductos` + SPs desactualizados; el modelo usa `TiradosProducto`/`Tiradas` por EF)
5. `Docs/sql/18_drop_registro_tacho.sql` — PENDIENTE (no ejecutar aún; elimina SPs RegistroTacho y tabla `RegistrosTacho`)
6. `10_seed_data.sql` — Datos iniciales (usuario admin; verificar nombre real del archivo antes de ejecutar)

---

## Migraciones EF Core (estrategia vigente)

El proyecto usa **EF Core Migrations** (ver `03-migraciones.md`): última migración `20261011162134_AddCategoriasYTiradas`. Los scripts 14/15/16/18 son complementos manuales (SPs, catálogo y DROP pendiente), no la estrategia principal.