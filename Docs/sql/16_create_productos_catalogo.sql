-- ============================================================
-- 16_create_productos_catalogo.sql
-- Catálogo general de productos (sin SemanaId).
-- Crea la tabla ProductosCatalogo y sus stored procedures.
-- Ejecutar UNA VEZ en SQL Server.
-- ============================================================
USE SistemaIndustrialDB;
GO

-- ============================================================
-- TABLA
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductosCatalogo')
BEGIN
    CREATE TABLE ProductosCatalogo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(200) NOT NULL,
        UnidadMedida NVARCHAR(50) NOT NULL,
        PedidoSemanalAproximado DECIMAL(18,2) NOT NULL DEFAULT 0,
        PromedioPorTacho DECIMAL(18,2) NOT NULL DEFAULT 0,
        Activo BIT NOT NULL DEFAULT 1,
        Observaciones NVARCHAR(500) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        CONSTRAINT UQ_ProductosCatalogo_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_ProductosCatalogo_Valores CHECK (PedidoSemanalAproximado >= 0 AND PromedioPorTacho >= 0)
    );
END
GO

-- ============================================================
-- SPs
-- ============================================================
-- NOTA: GetAll/GetById NO seleccionan FechaActualizacion (DateTime?)
-- para mantener el mapeo 1:1 con la entidad.

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id, Nombre, UnidadMedida, PedidoSemanalAproximado,
        PromedioPorTacho, Activo, Observaciones, FechaCreacion
    FROM ProductosCatalogo
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id, Nombre, UnidadMedida, PedidoSemanalAproximado,
        PromedioPorTacho, Activo, Observaciones, FechaCreacion
    FROM ProductosCatalogo
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_Create
    @Nombre NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @PedidoSemanalAproximado DECIMAL(18,2),
    @PromedioPorTacho DECIMAL(18,2),
    @Observaciones NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM ProductosCatalogo WHERE Nombre = @Nombre)
        THROW 50001, 'Ya existe un producto con ese nombre.', 1;

    INSERT INTO ProductosCatalogo (Nombre, UnidadMedida, PedidoSemanalAproximado, PromedioPorTacho, Activo, Observaciones, FechaCreacion)
    VALUES (@Nombre, @UnidadMedida, @PedidoSemanalAproximado, @PromedioPorTacho, 1, @Observaciones, GETDATE());

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_Update
    @Id INT,
    @Nombre NVARCHAR(200),
    @UnidadMedida NVARCHAR(50),
    @PedidoSemanalAproximado DECIMAL(18,2),
    @PromedioPorTacho DECIMAL(18,2),
    @Activo BIT,
    @Observaciones NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM ProductosCatalogo WHERE Nombre = @Nombre AND Id <> @Id)
        THROW 50001, 'Ya existe otro producto con ese nombre.', 1;

    UPDATE ProductosCatalogo
    SET Nombre = @Nombre,
        UnidadMedida = @UnidadMedida,
        PedidoSemanalAproximado = @PedidoSemanalAproximado,
        PromedioPorTacho = @PromedioPorTacho,
        Activo = @Activo,
        Observaciones = @Observaciones,
        FechaActualizacion = GETDATE()
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM ProductosCatalogo WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM ProductosCatalogo WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE sp_ProductosCatalogo_ExistsByNombre
    @Nombre NVARCHAR(200),
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM ProductosCatalogo
        WHERE Nombre = @Nombre
        AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
    ) THEN 1 ELSE 0 END;
END
GO
