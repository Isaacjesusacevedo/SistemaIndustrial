-- 14_create_tirado_productos.sql
-- Tabla TiradoProductos para control de producción por producto (Tirado/Armado)

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

-- Stored Procedures

-- GetAll
CREATE PROCEDURE sp_TiradoProductos_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    ORDER BY FechaCreacion DESC;
END
GO

-- GetById
CREATE PROCEDURE sp_TiradoProductos_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    WHERE Id = @Id;
END
GO

-- GetBySemanaId
CREATE PROCEDURE sp_TiradoProductos_GetBySemanaId
    @SemanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    WHERE SemanaId = @SemanaId
    ORDER BY Nombre;
END
GO

-- Create
CREATE PROCEDURE sp_TiradoProductos_Create
    @SemanaId INT,
    @Nombre NVARCHAR(100),
    @UnidadMedida NVARCHAR(50),
    @CantidadRequerida DECIMAL(18,2),
    @Observaciones NVARCHAR(500) = ''
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @CantidadRequerida <= 0
        THROW 50001, 'La cantidad requerida debe ser mayor a 0.', 1;
    
    IF NOT EXISTS (SELECT 1 FROM Semanas WHERE Id = @SemanaId)
        THROW 50002, 'La semana no existe.', 1;
    
    IF EXISTS (SELECT 1 FROM TiradoProductos WHERE SemanaId = @SemanaId AND Nombre = @Nombre)
        THROW 50003, 'Ya existe un producto con ese nombre en esta semana.', 1;
    
    INSERT INTO TiradoProductos (SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion)
    VALUES (@SemanaId, @Nombre, @UnidadMedida, @CantidadRequerida, 0, 0, @Observaciones, GETDATE(), GETDATE());
    
    SELECT SCOPE_IDENTITY() AS Id;
END
GO

-- Update
CREATE PROCEDURE sp_TiradoProductos_Update
    @Id INT,
    @Nombre NVARCHAR(100),
    @UnidadMedida NVARCHAR(50),
    @CantidadRequerida DECIMAL(18,2),
    @Observaciones NVARCHAR(500) = ''
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @CantidadRequerida <= 0
        THROW 50001, 'La cantidad requerida debe ser mayor a 0.', 1;
    
    IF EXISTS (SELECT 1 FROM TiradoProductos WHERE Nombre = @Nombre AND SemanaId = (SELECT SemanaId FROM TiradoProductos WHERE Id = @Id) AND Id <> @Id)
        THROW 50003, 'Ya existe un producto con ese nombre en esta semana.', 1;
    
    UPDATE TiradoProductos
    SET Nombre = @Nombre,
        UnidadMedida = @UnidadMedida,
        CantidadRequerida = @CantidadRequerida,
        Observaciones = @Observaciones,
        FechaActualizacion = GETDATE()
    WHERE Id = @Id;
END
GO

-- Delete
CREATE PROCEDURE sp_TiradoProductos_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM TiradoProductos WHERE Id = @Id;
END
GO

-- Exists
CREATE PROCEDURE sp_TiradoProductos_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM TiradoProductos WHERE Id = @Id) THEN 1 ELSE 0 END;
END
GO

-- ExistsByNombreSemana
CREATE PROCEDURE sp_TiradoProductos_ExistsByNombreSemana
    @Nombre NVARCHAR(100),
    @SemanaId INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM TiradoProductos 
        WHERE Nombre = @Nombre AND SemanaId = @SemanaId 
        AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
    ) THEN 1 ELSE 0 END;
END
GO

-- Registrar Tirada (PATCH /{id}/tirada)
CREATE PROCEDURE sp_TiradoProductos_RegistrarTirada
    @Id INT,
    @CantidadTirada DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @CantidadTirada <= 0
        THROW 50001, 'La cantidad tirada debe ser mayor a 0.', 1;
    
    UPDATE TiradoProductos
    SET CantidadTirada = CantidadTirada + @CantidadTirada,
        Stock = CantidadTirada + @CantidadTirada,
        FechaActualizacion = GETDATE()
    WHERE Id = @Id;
    
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    WHERE Id = @Id;
END
GO

-- GetBySemanaId
CREATE PROCEDURE sp_TiradoProductos_GetBySemanaId
    @SemanaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock, Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    WHERE SemanaId = @SemanaId
    ORDER BY Nombre;
END
GO

-- ExistsByNombreSemana
CREATE PROCEDURE sp_TiradoProductos_ExistsByNombreSemana
    @Nombre NVARCHAR(100),
    @SemanaId INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM TiradoProductos 
        WHERE Nombre = @Nombre AND SemanaId = @SemanaId 
        AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
    ) THEN 1 ELSE 0 END;
END
GO

-- GetBySemanaConFiltro
CREATE PROCEDURE sp_TiradoProductos_GetBySemanaConFiltro
    @SemanaId INT,
    @Filtro NVARCHAR(20) = NULL -- 'totales', 'faltantes', 'sobrantes'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, SemanaId, Nombre, UnidadMedida, CantidadRequerida, CantidadTirada, Stock,
        CASE WHEN CantidadTirada < CantidadRequerida THEN CantidadRequerida - CantidadTirada ELSE 0 END AS Faltante,
        CASE WHEN CantidadTirada > CantidadRequerida THEN CantidadTirada - CantidadRequerida ELSE 0 END AS Sobrante,
        CASE WHEN CantidadRequerida > 0 THEN ROUND((CantidadTirada / CantidadRequerida) * 100, 2) ELSE 0 END AS Porcentaje,
        CASE 
            WHEN CantidadRequerida = 0 THEN 'SinPlanificar'
            WHEN CantidadTirada > CantidadRequerida THEN 'Excedido'
            WHEN CantidadTirada >= CantidadRequerida THEN 'Completado'
            ELSE 'Pendiente'
        END AS Estado,
        Observaciones, FechaCreacion, FechaActualizacion
    FROM TiradoProductos
    WHERE SemanaId = @SemanaId
    AND (
        @Filtro IS NULL OR @Filtro = 'totales'
        OR (@Filtro = 'faltantes' AND CantidadTirada < CantidadRequerida)
        OR (@Filtro = 'sobrantes' AND CantidadTirada > CantidadRequerida)
    )
    ORDER BY Nombre;
END
GO