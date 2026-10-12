-- ============================================================
-- 18_drop_registro_tacho.sql
-- Elimina el módulo Registro Tacho (SPs + tabla).
-- NO ejecutarlo todavía: correr DESPUÉS de que el backend esté
-- actualizado (sin referencias a RegistrosTacho).
-- ============================================================
USE SistemaIndustrialDB;
GO

-- Eliminar SPs de RegistroTacho (si existen)
DROP PROCEDURE IF EXISTS sp_RegistroTacho_GetAll;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_GetById;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_GetBySemanaId;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_GetBySemanaTurno;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_Create;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_Update;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_Delete;
DROP PROCEDURE IF EXISTS sp_RegistroTacho_Exists;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_GetAll;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_GetById;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_GetBySemanaId;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_GetBySemanaTurno;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_Create;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_Update;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_Delete;
DROP PROCEDURE IF EXISTS sp_RegistrosTacho_Exists;
GO

-- Eliminar la tabla
IF OBJECT_ID('RegistrosTacho', 'U') IS NOT NULL
    DROP TABLE RegistrosTacho;
GO

PRINT ' Módulo Registro Tacho eliminado';
GO
