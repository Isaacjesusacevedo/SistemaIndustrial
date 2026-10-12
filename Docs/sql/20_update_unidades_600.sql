-- ============================================================
-- 20_update_unidades_600.sql
-- Migra los productos de "Galón (4 L)" a "600 (600 ml)"
--
-- IMPORTANTE: hay que actualizar 2 tablas:
--   - ProductosCatalogo (catálogo global)
--   - TiradosProducto (instancias por semana: snapshot histórico)
--
-- NOTA: usa la tabla TiradosProducto (nombre que genera EF Core).
-- Si tu BD se creó con el script 14 (tabla TiradoProductos),
-- reemplazá TiradosProducto por TiradoProductos en este script.
-- ============================================================
USE SistemaIndustrialDB;
GO

-- 1. Actualizar el catálogo
UPDATE ProductosCatalogo
SET UnidadMedida = '600 (600 ml)'
WHERE UnidadMedida = 'Galón (4 L)';

PRINT ' Catálogo actualizado';

-- 2. Actualizar los productos ya cargados en semanas
UPDATE TiradosProducto
SET UnidadMedida = '600 (600 ml)'
WHERE UnidadMedida = 'Galón (4 L)';

PRINT ' Productos de semanas actualizados';

-- 3. Verificación
SELECT 'Catalogo' AS Tabla, UnidadMedida, COUNT(*) AS Cantidad
FROM ProductosCatalogo GROUP BY UnidadMedida
UNION ALL
SELECT 'Tirados', UnidadMedida, COUNT(*) AS Cantidad
FROM TiradosProducto GROUP BY UnidadMedida;
GO
