-- ============================================================
-- 15_create_all_stored_procedures.sql
-- Recrea todas las stored procedures del sistema
-- Ejecutar UNA VEZ después de las migraciones de EF Core
-- ============================================================
USE SistemaIndustrialDB;
GO

-- ============================================================
-- SEMANAS
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Semanas_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.Id, s.NumeroSemana, s.Anio, s.FechaInicio, s.FechaFin,
        s.Estado, s.FechaCreacion,
        (SELECT COUNT(*) FROM InsumosSemana   WHERE SemanaId = s.Id) AS InsumosCount
    FROM Semanas s
    ORDER BY s.Anio DESC, s.NumeroSemana DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.Id, s.NumeroSemana, s.Anio, s.FechaInicio, s.FechaFin,
        s.Estado, s.FechaCreacion,
        (SELECT COUNT(*) FROM InsumosSemana   WHERE SemanaId = s.Id) AS InsumosCount
    FROM Semanas s
    WHERE s.Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_Create
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

    DECLARE @DiaInicioCreate INT = DATEPART(WEEKDAY, @FechaInicio);
    DECLARE @DiaFinCreate INT = DATEPART(WEEKDAY, @FechaFin);
    DECLARE @DiffDiasCreate INT = DATEDIFF(DAY, @FechaInicio, @FechaFin);

    IF @DiaInicioCreate <> 2 -- lunes (con DATEFIRST 7: domingo=1, lunes=2)
        THROW 50002, 'La fecha de inicio debe ser un lunes.', 1;

    IF @DiaFinCreate <> 7 -- sábado
        THROW 50003, 'La fecha de fin debe ser un sábado.', 1;

    IF @DiffDiasCreate <> 5
        THROW 50004, 'La semana debe durar exactamente 6 días (lunes a sábado).', 1;

    INSERT INTO Semanas (NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion)
    VALUES (@NumeroSemana, @Anio, @FechaInicio, @FechaFin, @Estado, GETDATE());

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_Update
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

    DECLARE @DiaInicioUpd INT = DATEPART(WEEKDAY, @FechaInicio);
    DECLARE @DiaFinUpd INT = DATEPART(WEEKDAY, @FechaFin);
    DECLARE @DiffDiasUpd INT = DATEDIFF(DAY, @FechaInicio, @FechaFin);

    IF @DiaInicioUpd <> 2 -- lunes (con DATEFIRST 7: domingo=1, lunes=2)
        THROW 50002, 'La fecha de inicio debe ser un lunes.', 1;

    IF @DiaFinUpd <> 7 -- sábado
        THROW 50003, 'La fecha de fin debe ser un sábado.', 1;

    IF @DiffDiasUpd <> 5
        THROW 50004, 'La semana debe durar exactamente 6 días (lunes a sábado).', 1;

    IF EXISTS (SELECT 1 FROM Semanas WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio AND Id <> @Id)
        THROW 50001, 'Ya existe otra semana con ese número y año.', 1;

    UPDATE Semanas
    SET NumeroSemana = @NumeroSemana,
        Anio = @Anio,
        FechaInicio = @FechaInicio,
        FechaFin = @FechaFin,
        Estado = @Estado
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Semanas WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Semanas WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_ExistsByNumeroAnio
    @NumeroSemana INT,
    @Anio INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM Semanas
        WHERE NumeroSemana = @NumeroSemana AND Anio = @Anio
        AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
    ) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE sp_Semanas_HasDependentData
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM InsumosSemana   WHERE SemanaId = @Id
        UNION ALL
        SELECT 1 FROM StockSemana     WHERE SemanaId = @Id
        UNION ALL
        SELECT 1 FROM TiradoProductos WHERE SemanaId = @Id
    ) THEN 1 ELSE 0 END;
END
GO

-- ============================================================
-- INSUMOS
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Insumos_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, 
           CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana
    ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_Insumos_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, 
           CantidadConsumida, StockDisponible, FechaCreacion
    FROM InsumosSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Insumos_GetBySemanaId
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

CREATE OR ALTER PROCEDURE sp_Insumos_Create
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

CREATE OR ALTER PROCEDURE sp_Insumos_Update
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

CREATE OR ALTER PROCEDURE sp_Insumos_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM InsumosSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Insumos_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM InsumosSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

-- ============================================================
-- STOCK
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Stock_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial, 
           Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana
    ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_Stock_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Producto, UnidadMedida, StockInicial, 
           Entradas, Salidas, StockFinal, FechaCreacion
    FROM StockSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Stock_GetBySemanaId
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

CREATE OR ALTER PROCEDURE sp_Stock_Create
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

CREATE OR ALTER PROCEDURE sp_Stock_Update
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

CREATE OR ALTER PROCEDURE sp_Stock_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM StockSemana WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Stock_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM StockSemana WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

-- ============================================================
-- USUARIOS
-- ============================================================
CREATE OR ALTER PROCEDURE sp_Usuarios_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Email, Nombre, Role, Activo, FechaCreacion
    FROM Usuarios
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Email, Nombre, Role, Activo, FechaCreacion
    FROM Usuarios WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Email, PasswordHash, Nombre, Role, Activo, FechaCreacion,
           RefreshTokenHash, RefreshTokenExpiry
    FROM Usuarios WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_Create
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @Nombre NVARCHAR(100),
    @Role NVARCHAR(50) = 'Operador'
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = @Email)
        THROW 50001, 'Ya existe un usuario con ese email.', 1;

    INSERT INTO Usuarios (Email, PasswordHash, Nombre, Role, Activo, FechaCreacion)
    VALUES (@Email, @PasswordHash, @Nombre, @Role, 1, GETDATE());

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_Update
    @Id INT,
    @Email NVARCHAR(256),
    @Nombre NVARCHAR(100),
    @Role NVARCHAR(50),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuarios
    SET Email = @Email,
        Nombre = @Nombre,
        Role = @Role,
        Activo = @Activo
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Usuarios WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Usuarios WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE sp_Usuarios_UpdateRefreshToken
    @Id INT,
    @RefreshTokenHash NVARCHAR(MAX),
    @RefreshTokenExpiry DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuarios
    SET RefreshTokenHash = @RefreshTokenHash,
        RefreshTokenExpiry = @RefreshTokenExpiry
    WHERE Id = @Id;
END
GO

PRINT 'Todas las stored procedures fueron creadas correctamente.';
GO