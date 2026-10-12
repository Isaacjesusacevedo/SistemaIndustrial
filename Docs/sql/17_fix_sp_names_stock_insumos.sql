-- ============================================================
-- 17_fix_sp_names_stock_insumos.sql
-- Corrige los nombres de las SPs de Stock y Insumos para que
-- coincidan con lo que espera el backend.
--
-- El script 15 crea sp_Stock_* y sp_Insumos_*, pero los
-- repositories buscan sp_StockSemana_* y sp_InsumosSemana_*.
-- ============================================================
USE SistemaIndustrialDB;
GO

-- StockSemana
CREATE OR ALTER PROCEDURE sp_StockSemana_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial,
           Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana
    ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial,
           Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_GetBySemanaId
    @SemanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial,
           Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana
    WHERE SemanaId = @SemanaId
    ORDER BY Producto;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_Create
    @SemanaId INT,
    @Producto NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @StockInicial DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @StockInicial < 0
        THROW 50001, 'El stock inicial no puede ser negativo.', 1;

    IF NOT EXISTS (SELECT 1 FROM Semanas WHERE Id = @SemanaId)
        THROW 50002, 'La semana no existe.', 1;

    INSERT INTO StockSemana (SemanaId, Producto, UnidadMedida, StockInicial,
                              Entradas, Salidas, StockFinal, FechaCreacion)
    VALUES (@SemanaId, @Producto, @UnidadMedida, @StockInicial, 0, 0, @StockInicial, GETDATE());

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_Update
    @Id INT,
    @Producto NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @StockInicial DECIMAL(18,2),
    @Entradas DECIMAL(18,2),
    @Salidas DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @StockInicial < 0 OR @Entradas < 0 OR @Salidas < 0
        THROW 50001, 'Las cantidades no pueden ser negativas.', 1;

    DECLARE @StockFinal DECIMAL(18,2) = @StockInicial + @Entradas - @Salidas;

    UPDATE StockSemana
    SET Producto = @Producto,
        UnidadMedida = @UnidadMedida,
        StockInicial = @StockInicial,
        Entradas = @Entradas,
        Salidas = @Salidas,
        StockFinal = @StockFinal
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM StockSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_StockSemana_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM StockSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

-- InsumosSemana
CREATE OR ALTER PROCEDURE sp_InsumosSemana_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida,
           CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana
    ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida,
           CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_GetBySemanaId
    @SemanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida,
           CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana
    WHERE SemanaId = @SemanaId
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_Create
    @SemanaId INT,
    @Nombre NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @CantidadRequerida DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @CantidadRequerida < 0
        THROW 50001, 'La cantidad requerida no puede ser negativa.', 1;

    IF NOT EXISTS (SELECT 1 FROM Semanas WHERE Id = @SemanaId)
        THROW 50002, 'La semana no existe.', 1;

    INSERT INTO InsumosSemana (SemanaId, Nombre, UnidadMedida, CantidadRequerida,
                                CantidadConsumida, StockDisponible, FechaCreacion)
    VALUES (@SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida, 0, 0, GETDATE());

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_Update
    @Id INT,
    @Nombre NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @CantidadRequerida DECIMAL(18,2),
    @CantidadConsumida DECIMAL(18,2),
    @StockDisponible DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF @CantidadRequerida < 0 OR @CantidadConsumida < 0 OR @StockDisponible < 0
        THROW 50001, 'Las cantidades no pueden ser negativas.', 1;

    UPDATE InsumosSemana
    SET Nombre = @Nombre,
        UnidadMedida = @UnidadMedida,
        CantidadRequerida = @CantidadRequerida,
        CantidadConsumida = @CantidadConsumida,
        StockDisponible = @StockDisponible
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM InsumosSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsumosSemana_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM InsumosSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

PRINT ' SPs de StockSemana e InsumosSemana creadas';
GO
