-- ============================================================
-- 19_update_week_states.sql
-- Actualiza el estado de las semanas según el progreso real
--
-- Reglas:
--   - Sin productos cargados                Planificada
--   - Con productos pero con faltantes      EnProgreso
--   - Todos los productos cubiertos         Completada
--     (StockTotal >= 0 en todos)
--
-- NOTA: usa la tabla TiradosProducto (nombre que genera EF Core).
-- Si tu BD se creó con el script 14 (tabla TiradoProductos),
-- reemplazá TiradosProducto por TiradoProductos en este script.
-- ============================================================
USE SistemaIndustrialDB;
GO

UPDATE s
SET s.Estado =
    CASE
        -- Sin productos = Planificada
        WHEN (SELECT COUNT(*) FROM TiradosProducto WHERE SemanaId = s.Id) = 0
            THEN 'Planificada'

        -- Todos con StockTotal >= 0 = Completada
        WHEN NOT EXISTS (
            SELECT 1 FROM TiradosProducto
            WHERE SemanaId = s.Id
            AND (StockInicial + CantidadTirada - PedidoSemanal) < 0
        )
            THEN 'Completada'

        -- Hay faltantes = EnProgreso
        ELSE 'EnProgreso'
    END
FROM Semanas s;

PRINT ' Estados actualizados';

-- Verificar
SELECT
    NumeroSemana,
    Anio,
    Estado,
    (SELECT COUNT(*) FROM TiradosProducto WHERE SemanaId = s.Id) AS Productos,
    (SELECT COUNT(*) FROM TiradosProducto
     WHERE SemanaId = s.Id
       AND (StockInicial + CantidadTirada - PedidoSemanal) < 0) AS Faltantes
FROM Semanas s
ORDER BY Anio DESC, NumeroSemana DESC;
GO
