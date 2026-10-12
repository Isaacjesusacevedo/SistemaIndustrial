# Stored Procedures

> Fuente de verdad: `Docs/sql/15_create_all_stored_procedures.sql` (30 SPs: `sp_Semanas_*`, `sp_Insumos_*`, `sp_Stock_*`, `sp_Usuarios_*`), `16_create_productos_catalogo.sql` (7 `sp_ProductosCatalogo_*`), `14_create_tirado_productos.sql` (`sp_TiradoProductos_*`, histórico/desactualizado) y `18_drop_registro_tacho.sql` (DROP pendiente). **No existen** SPs de Objetivos, RegistroTacho, Dashboard, Categorías ni Tiradas en el script 15; los repos nuevos (`CategoriaRepository`, `TiradoProductoRepository`) usan **EF directo** sin SPs. No hay bulk de tirado ni sync en estos scripts.

## Convenciones

| Patrón | Ejemplo |
|--------|---------|
| GetAll | `sp_Usuarios_GetAll` |
| GetById | `sp_Usuarios_GetById @Id` |
| GetByCampo | `sp_Usuarios_GetByEmail @Email` |
| Create | `sp_Usuarios_Create @Email, @PasswordHash, @Nombre, @Role` |
| Update | `sp_Usuarios_Update @Id, @Nombre, @Role, @Activo` |
| Delete | `sp_Usuarios_Delete @Id` |
| Exists | `sp_Usuarios_Exists @Id` |
| ExistsByCampo | `sp_Usuarios_ExistsByEmail @Email` |

---

## Usuarios

```sql
-- GetAll
CREATE PROCEDURE sp_Usuarios_GetAll
AS
BEGIN
    SELECT Id, Email, Nombre, Role, Activo, FechaCreacion
    FROM Usuarios
    WHERE Activo = 1
    ORDER BY FechaCreacion DESC;
END

-- GetById (script 15: NO devuelve PasswordHash ni RefreshToken)
CREATE PROCEDURE sp_Usuarios_GetById @Id INT
AS
BEGIN
    SELECT Id, Email, Nombre, Role, Activo, FechaCreacion
    FROM Usuarios
    WHERE Id = @Id;
END

-- GetByEmail (Login; script 15: SÍ devuelve PasswordHash + RefreshToken)
CREATE PROCEDURE sp_Usuarios_GetByEmail @Email NVARCHAR(256)
AS
BEGIN
    SELECT Id, Email, PasswordHash, Nombre, Role, Activo, FechaCreacion,
           RefreshTokenHash, RefreshTokenExpiry
    FROM Usuarios
    WHERE Email = @Email;
END

-- Create
CREATE PROCEDURE sp_Usuarios_Create
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(255),
    @Nombre NVARCHAR(100),
    @Role NVARCHAR(50) = 'Operador'
AS
BEGIN
    INSERT INTO Usuarios (Email, PasswordHash, Nombre, Role)
    VALUES (@Email, @PasswordHash, @Nombre, @Role);
    SELECT SCOPE_IDENTITY() AS Id;
END

-- Update (script 15: incluye @Email)
CREATE PROCEDURE sp_Usuarios_Update
    @Id INT,
    @Email NVARCHAR(256),
    @Nombre NVARCHAR(100),
    @Role NVARCHAR(50),
    @Activo BIT
AS
BEGIN
    UPDATE Usuarios
    SET Email = @Email, Nombre = @Nombre, Role = @Role, Activo = @Activo
    WHERE Id = @Id;
END

-- UpdateRefreshToken
CREATE PROCEDURE sp_Usuarios_UpdateRefreshToken
    @Id INT,
    @RefreshTokenHash NVARCHAR(255),
    @RefreshTokenExpiry DATETIME
AS
BEGIN
    UPDATE Usuarios
    SET RefreshTokenHash = @RefreshTokenHash, RefreshTokenExpiry = @RefreshTokenExpiry
    WHERE Id = @Id;
END

-- Delete (script 15: DELETE físico, no soft)
CREATE PROCEDURE sp_Usuarios_Delete @Id INT
AS
BEGIN
    DELETE FROM Usuarios WHERE Id = @Id;
END

-- Exists
CREATE PROCEDURE sp_Usuarios_Exists @Id INT
AS
BEGIN
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Usuarios WHERE Id = @Id) THEN 1 ELSE 0 END;
END

-- Nota: `sp_Usuarios_ExistsByEmail` lo usa `UsuarioRepository` pero NO existe en el script 15.
```

---

## Semanas (nombres exactos del script 15; con validación lunes–sábado)

> El script 15 crea 8 SPs: `sp_Semanas_GetAll/GetById/Create/Update/Delete/Exists/ExistsByNumeroAnio/HasDependentData`. **No existen** en el script 15: `GetByNumeroAnio`, `GetByAnio` ni `UpdateEstado` (documentados antes por error).

```sql
-- GetAll
CREATE PROCEDURE sp_Semanas_GetAll
AS
BEGIN
    SELECT Id, NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion
    FROM Semanas
    ORDER BY Anio DESC, NumeroSemana DESC;
END

-- GetById
CREATE PROCEDURE sp_Semanas_GetById @Id INT
AS
BEGIN
    SELECT Id, NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion
    FROM Semanas
    WHERE Id = @Id;
END

-- GetByNumeroAnio
CREATE PROCEDURE sp_Semanas_GetByNumeroAnio @NumeroSemana INT, @Anio INT
AS
BEGIN
    SELECT Id, NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion
    FROM Semanas
    WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio;
END

-- GetByAnio
CREATE PROCEDURE sp_Semanas_GetByAnio @Anio INT
AS
BEGIN
    SELECT Id, NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion
    FROM Semanas
    WHERE Anio = @Anio
    ORDER BY NumeroSemana;
END

-- Create (script 15: valida lunes–sábado, 6 días)
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

-- Update (script 15: valida lunes–sábado, 6 días)
CREATE PROCEDURE sp_Semanas_Update
    @Id INT,
    @NumeroSemana INT,
    @Anio INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @Estado NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET DATEFIRST 7;

    IF @FechaFin <= @FechaInicio
        THROW 50002, 'La fecha fin debe ser posterior a la fecha inicio.', 1;

    IF DATEPART(WEEKDAY, @FechaInicio) <> 2 -- lunes
        THROW 50002, 'La fecha de inicio debe ser un lunes.', 1;

    IF DATEPART(WEEKDAY, @FechaFin) <> 7 -- sábado
        THROW 50003, 'La fecha de fin debe ser un sábado.', 1;

    IF DATEDIFF(DAY, @FechaInicio, @FechaFin) <> 5
        THROW 50004, 'La semana debe durar exactamente 6 días (lunes a sábado).', 1;

    IF EXISTS (SELECT 1 FROM Semanas WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio AND Id <> @Id)
        THROW 50001, 'Ya existe otra semana con ese número y año.', 1;

    UPDATE Semanas
    SET NumeroSemana = @NumeroSemana, Anio = @Anio, FechaInicio = @FechaInicio, FechaFin = @FechaFin, Estado = @Estado
    WHERE Id = @Id;
END

-- Delete (validar dependencias en BR)
CREATE PROCEDURE sp_Semanas_Delete @Id INT
AS
BEGIN
    DELETE FROM Semanas WHERE Id = @Id;
END

-- Exists
CREATE PROCEDURE sp_Semanas_Exists @Id INT
AS
BEGIN
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Semanas WHERE Id = @Id) THEN 1 ELSE 0 END;
END

-- ExistsByNumeroAnio
CREATE PROCEDURE sp_Semanas_ExistsByNumeroAnio @NumeroSemana INT, @Anio INT, @ExcludeId INT = NULL
AS
BEGIN
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM Semanas 
        WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio 
        AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
    ) THEN 1 ELSE 0 END;
END

-- HasDependentData (script 15: InsumosSemana, StockSemana y TiradoProductos; sin Objetivos/Registro)
CREATE PROCEDURE sp_Semanas_HasDependentData @Id INT
AS
BEGIN
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM InsumosSemana   WHERE SemanaId = @Id
        UNION ALL SELECT 1 FROM StockSemana WHERE SemanaId = @Id
        UNION ALL SELECT 1 FROM TiradoProductos WHERE SemanaId = @Id
    ) THEN 1 ELSE 0 END;
END
```

---

## ObjetivosSemana (ELIMINADA del modelo — sin SPs en script 15)

> Tabla eliminada del `AppDbContext` (sin `DbSet`; no existe `Models/ObjetivoSemana.cs`). El script 15 **no crea** `sp_ObjetivosSemana_*`. El bloque anterior se conserva en el historial Git; no se documentan SPs inexistentes.

---

## Insumos (nombres exactos del script 15: `sp_Insumos_*`)

> El script 15 crea `sp_Insumos_*` (sin sufijo `Semana`). **Desactualización conocida:** `InsumoRepository` llama `sp_InsumosSemana_*`, que no existen en el script 15. Se documentan los nombres reales del script.

```sql
CREATE PROCEDURE sp_Insumos_GetAll AS
BEGIN
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana ORDER BY FechaCreacion DESC;
END

CREATE PROCEDURE sp_Insumos_GetById @Id INT AS
BEGIN
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana WHERE Id = @Id;
END

CREATE PROCEDURE sp_Insumos_GetBySemanaId @SemanaId INT AS
BEGIN
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana WHERE SemanaId = @SemanaId ORDER BY Nombre;
END

CREATE PROCEDURE sp_Insumos_Create
    @SemanaId INT, @Nombre NVARCHAR(200), @UnidadMedida NVARCHAR(50), @CantidadRequerida DECIMAL(18,2)
AS
BEGIN
    IF @CantidadRequerida < 0 THROW 50001, 'La cantidad requerida no puede ser negativa.', 1;
    IF NOT EXISTS(SELECT 1 FROM Semanas WHERE Id = @SemanaId) THROW 50002, 'La semana no existe.', 1;
    INSERT INTO InsumosSemana (SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadConsumida, StockDisponible, FechaCreacion)
    VALUES (@SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida, 0, 0, GETDATE());
    SELECT SCOPE_IDENTITY() AS Id;
END

CREATE PROCEDURE sp_Insumos_Update
    @Id INT, @Nombre NVARCHAR(200), @UnidadMedida NVARCHAR(50), 
    @CantidadRequerida DECIMAL(18,2), @CantidadConsumida DECIMAL(18,2), @StockDisponible DECIMAL(18,2)
AS
BEGIN
    IF @CantidadRequerida < 0 OR @CantidadConsumida < 0 OR @StockDisponible < 0
        THROW 50001, 'Las cantidades no pueden ser negativas.', 1;
    UPDATE InsumosSemana
    SET Nombre = @Nombre, UnidadMedida = @UnidadMedida, 
        CantidadRequerida = @CantidadRequerida, CantidadConsumida = @CantidadConsumida, StockDisponible = @StockDisponible
    WHERE Id = @Id;
END

CREATE PROCEDURE sp_Insumos_Delete @Id INT AS
BEGIN
    DELETE FROM InsumosSemana WHERE Id = @Id;
END

CREATE PROCEDURE sp_Insumos_Exists @Id INT AS
BEGIN
    SELECT CASE WHEN EXISTS(SELECT 1 FROM InsumosSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
```

---

## Stock (nombres exactos del script 15: `sp_Stock_*`)

> El script 15 crea `sp_Stock_*` (sin sufijo `Semana`). **Desactualización conocida:** `StockRepository` llama `sp_Stock_*`, que no existen en el script 15. Se documentan los nombres reales del script.

```sql
CREATE PROCEDURE sp_Stock_GetAll AS
BEGIN
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial, Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana ORDER BY FechaCreacion;
END

CREATE PROCEDURE sp_Stock_GetById @Id INT AS
BEGIN
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial, Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana WHERE Id = @Id;
END

CREATE PROCEDURE sp_Stock_GetBySemanaId @SemanaId INT AS
BEGIN
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial, Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana WHERE SemanaId = @SemanaId ORDER BY FechaCreacion;
END

CREATE PROCEDURE sp_Stock_Create
    @SemanaId INT, @Producto NVARCHAR(200), @UnidadMedida NVARCHAR(50), @StockInicial DECIMAL(18,2)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Semanas WHERE Id = @SemanaId) THROW 50001, 'Semana no existe', 1;
    DECLARE @StockFinal DECIMAL(18,2) = @StockInicial; -- Entradas=0, Salidas=0
    INSERT INTO StockSemana (SemanaId, Producto, UnidadMedida, StockInicial, Entradas, Salidas, StockFinal)
    VALUES (@SemanaId, @Producto, @UnidadMedida, @StockInicial, 0, 0, @StockFinal);
    SELECT SCOPE_IDENTITY() AS Id;
END

CREATE PROCEDURE sp_Stock_Update
    @Id INT, @Producto NVARCHAR(200), @UnidadMedida NVARCHAR(50),
    @StockInicial DECIMAL(18,2), @Entradas DECIMAL(18,2), @Salidas DECIMAL(18,2)
AS
BEGIN
    DECLARE @StockFinal DECIMAL(18,2) = @StockInicial + @Entradas - @Salidas;
    UPDATE StockSemana
    SET Producto = @Producto, UnidadMedida = @UnidadMedida,
        StockInicial = @StockInicial, Entradas = @Entradas, Salidas = @Salidas, StockFinal = @StockFinal
    WHERE Id = @Id;
END

CREATE PROCEDURE sp_Stock_Delete @Id INT AS
BEGIN
    DELETE FROM StockSemana WHERE Id = @Id;
END

CREATE PROCEDURE sp_Stock_Exists @Id INT AS
BEGIN
    SELECT CASE WHEN EXISTS(SELECT 1 FROM StockSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
```

---

## RegistroTacho (ELIMINADO del modelo — DROP pendiente en script 18)

> Tabla eliminada del `AppDbContext` (sin `DbSet`). El script 15 **no crea** SPs de RegistroTacho. El script `18_drop_registro_tacho.sql` (pendiente, no ejecutar aún) elimina `sp_RegistroTacho_GetAll/GetById/GetBySemanaId/GetBySemanaTurno/Create/Update/Delete/Exists` y sus variantes `sp_RegistrosTacho_*`, más la tabla `RegistrosTacho`. El bloque anterior con cuerpos de SPs se conserva en el historial Git; no se documentan SPs inexistentes en el script 15. `RegistroTachoRepository` aún referencia `sp_RegistroTacho_*` (pendiente de migrar/eliminar con el backend).

---

## Dashboard (sin SPs en script 15 — histórico)

> El script 15 **no crea** `sp_Dashboard_*`. Los bloques anteriores (`GetMetricas`, `GetTendencias`, `GetAlertas`, basados en `ObjetivosSemana`/`RegistroTacho`) se conservan en el historial Git; no corresponden al código/SQL real vigente.

---

## ProductosCatalogo (nombres exactos del script 16)

```sql
sp_ProductosCatalogo_GetAll
sp_ProductosCatalogo_GetById @Id
sp_ProductosCatalogo_Create @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Observaciones
sp_ProductosCatalogo_Update @Id, @Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, @Activo, @Observaciones
sp_ProductosCatalogo_Delete @Id
sp_ProductosCatalogo_Exists @Id
sp_ProductosCatalogo_ExistsByNombre @Nombre, @ExcludeId
```

> Nota del script 16: `GetAll`/`GetById` no seleccionan `FechaActualizacion` para mantener el mapeo 1:1 con la entidad.

---

## TiradoProductos (script 14 histórico — desactualizado; repos usan EF directo)

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

> Desactualizado respecto al modelo EF (`TiradosProducto` con `PedidoSemanal`, `CategoriaId`, `StockInicial`, `PromedioPorTacho` + tabla `Tiradas`). `TiradoProductoRepository` y `CategoriaRepository` usan EF directo (`DbContext`, `Include`, `SaveChangesAsync`) sin SPs. Sin SPs de Categorías ni Tiradas en ningún script; sin bulk ni sync.

---

## Categorías y Tiradas (EF directo, sin SPs)

> No existen SPs `sp_Categoria_*` ni `sp_Tirada_*` en los scripts 14/15/16/18. Acceso vía `CategoriaRepository` y `TiradoProductoRepository` (`GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `ExistsAsync`, `GetBySemanaIdAsync`, `ExistsByNombreSemanaAsync`, `AddTiradaAsync`, `GetTiradasByProductoAsync`).