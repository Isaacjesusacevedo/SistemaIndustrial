# Estrategia de Migraciones

## Enfoque: EF Core Migrations + scripts SQL complementarios

El proyecto usa **EF Core Migrations** como estrategia principal. Última migración: `20261011162134_AddCategoriasYTiradas` (ver `SistemaIndustrial.AD/Migrations/`). Los scripts numerados en `Docs/sql/` son complementos manuales: `14_create_tirado_productos.sql` (histórico/desactualizado), `15_create_all_stored_procedures.sql` (30 SPs reales), `16_create_productos_catalogo.sql` (catálogo + SPs) y `18_drop_registro_tacho.sql` (DROP pendiente, no ejecutar aún).

### Estado real de la última migración
- **Cubre**: `Categorias`, `Semanas`, `Usuarios`, `InsumosSemana`, `StockSemana`, `TiradosProducto` (con `CategoriaId` nullable `SET NULL`, `PedidoSemanal`, `StockInicial`, `PromedioPorTacho`, UK `SemanaId+Nombre`) y `Tiradas` (FK CASCADE). Índices: `IX_Categorias_Nombre` (UK), `IX_Semanas_NumeroSemana_Anio` (UK), `IX_TiradosProducto_SemanaId_Nombre` (UK), `IX_TiradosProducto_CategoriaId`, `IX_Tiradas_TiradoProductoId`, más `IX_Usuarios_Email` (UK) y FKs de Insumos/Stock.
- **Histórico pendiente de limpiar**: aún crea `ObjetivosSemana` y `RegistrosTacho`, eliminados del `AppDbContext` vigente (sin `DbSet`; no existe `Models/ObjetivoSemana.cs`; `Semana` solo expone `Insumos`, `Stocks`, `TiradosProducto`).
- **Faltante**: no crea `ProductosCatalogo` (existe `DbSet ProductosCatalogo` + script 16, pero sin migración).
- El snapshot (`AppDbContextModelSnapshot.cs`) coincide con la migración (incluye `ObjetivoSemana`/`RegistroTacho` históricos; sin `ProductoCatalogo`).

### Ventajas (EF + scripts)

### Ventajas
- Control total sobre el SQL generado
- Facilita revisiones de código (code review) de cambios de BD
- Compatible con cualquier herramienta de despliegue
- Permite optimizaciones específicas de SQL Server
- Historial auditable en Git

### Desventajas
- Requiere disciplina del equipo
- Posibles discrepancias entre modelo C# y BD
- Rollback manual

---

## Estructura de Scripts

```
Docs/sql/
├── 14_create_tirado_productos.sql       -- Histórico: tabla TiradoProductos + sp_TiradoProductos_* (desactualizado vs EF)
├── 15_create_all_stored_procedures.sql  -- 30 SPs reales: sp_Semanas_*, sp_Insumos_*, sp_Stock_*, sp_Usuarios_*
├── 16_create_productos_catalogo.sql     -- Tabla ProductosCatalogo + 7 sp_ProductosCatalogo_*
├── 18_drop_registro_tacho.sql           -- PENDIENTE (no ejecutar aún): DROP SPs RegistroTacho + tabla RegistrosTacho
└── SistemaIndustrial.AD/Migrations/
    └── 20261011162134_AddCategoriasYTiradas.cs  -- Migración EF vigente (ver cobertura arriba)
```

### Convención de Nombrado Migraciones
- Formato: `YYYYMMDD_descripcion_corta.sql`
- Prefijo de fecha para orden cronológico
- Descripción en minúsculas con underscore

---

## Patrones de Migración

### 1. ALTER TABLE (Add Column)
```sql
-- 20260220_add_refresh_token.sql
ALTER TABLE Usuarios
ADD RefreshTokenHash NVARCHAR(255) NULL,
    RefreshTokenExpiry DATETIME NULL;
GO
```

### 2. ALTER TABLE (Modify Column)
```sql
-- 20260301_increase_email_length.sql
ALTER TABLE Usuarios
ALTER COLUMN Email NVARCHAR(320) NOT NULL; -- RFC 5321 max
GO
```

### 3. ADD Constraint
```sql
-- 20260215_add_unique_semana.sql
ALTER TABLE Semanas
ADD CONSTRAINT UQ_Semanas_NumeroAnio UNIQUE (NumeroSemana, Anio);
GO
```

### 4. DROP Constraint
```sql
-- 20260401_remove_old_check.sql
ALTER TABLE ObjetivosSemana
DROP CONSTRAINT CK_Objetivos_Porcentaje;
GO
```

### 5. CREATE INDEX
```sql
-- 20260315_index_registro_tacho_fecha.sql
CREATE NONCLUSTERED INDEX IX_RegistroTacho_FechaRegistro 
ON RegistroTacho (FechaRegistro);
GO
```

### 6. DROP INDEX
```sql
-- 20260405_drop_unused_index.sql
DROP INDEX IX_ObjetivosSemana_Descripcion ON ObjetivosSemana;
GO
```

### 7. Stored Procedure Versioning
```sql
-- 20260320_update_sp_semanas_getall.sql
-- DROP y CREATE para versionar SPs
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Semanas_GetAll') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Semanas_GetAll;
GO

CREATE PROCEDURE sp_Semanas_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, NumeroSemana, Anio, FechaInicio, FechaFin, Estado, FechaCreacion
    FROM Semanas
    ORDER BY Anio DESC, NumeroSemana DESC;
END
GO
```

---

## Flujo de Trabajo

### Desarrollo Local
1. Crear migración EF (`dotnet ef migrations add ...`) para cambios del modelo (`Models/` + `AppDbContext.cs`); complementos manuales en `Docs/sql/` solo para SPs/catálogo/DROP (15/16/18).
2. Ejecutar en LocalDB: `dotnet ef database update` y luego los scripts 15/16 (el 18 no ejecutar aún; el 14 es histórico).
3. Actualizar entidades C# en `SistemaIndustrial.AD/Models/` si corresponde (fuente: `AppDbContext.cs` + `Models/*.cs`).
4. Commit ambos cambios juntos.

### Despliegue a Producción
```bash
# 1. Migraciones EF
dotnet ef database update --project SistemaIndustrial.AD --startup-project SistemaIndustrial.API
# 2. Scripts complementarios en orden
sqlcmd -S $PROD_SERVER -d SistemaIndustrialDB -i Docs/sql/16_create_productos_catalogo.sql
sqlcmd -S $PROD_SERVER -d SistemaIndustrialDB -i Docs/sql/15_create_all_stored_procedures.sql
# 18_drop_registro_tacho.sql: PENDIENTE (no ejecutar hasta actualizar backend)
```

### Rollback
- Cada migración debe tener su script de rollback inverso
- Guardar en `Docs/sql/rollbacks/YYYYMMDD_descripcion_rollback.sql`
- Probar rollback en staging antes de producción

---

## Versionado de Base de Datos

> EF Core usa su propia tabla `__EFMigrationsHistory`. El bloque `__MigrationHistory` de abajo es una propuesta histórica no implementada; se conserva como referencia.

### Tabla de Control de Versiones
```sql
CREATE TABLE __MigrationHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MigrationName NVARCHAR(255) NOT NULL,
    AppliedAt DATETIME NOT NULL DEFAULT GETDATE(),
    AppliedBy NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    Description NVARCHAR(MAX) NULL,
    Checksum NVARCHAR(64) NULL -- SHA256 del script
);
```

### Registro de Aplicación
```sql
-- Al final de cada script de migración
INSERT INTO __MigrationHistory (MigrationName, Description, Checksum)
VALUES ('20260310_add_dashboard_sps', 'Agrega SPs para dashboard', 'a1b2c3d4...');
```

---

## Sincronización Modelo C# ↔ BD

### Herramientas Recomendadas
1. **EF Core Reverse Engineering** (solo lectura):
   ```bash
   dotnet ef dbcontext scaffold "Server=...;Database=SistemaIndustrialDB;..." Microsoft.EntityFrameworkCore.SqlServer -o Models -c AppDbContext --no-onconfiguring --context-dir Data
   ```

2. **SQL Server Data Tools (SSDT)** en Visual Studio para comparación esquema

3. **Revisión manual** en code review: cada PR que toque `Models/` debe incluir script SQL correspondiente

### Checklist Code Review
- [ ] ¿Migración EF en `SistemaIndustrial.AD/Migrations/` (o script en `Docs/sql/` si es SP/catálogo/DROP)?
- [ ] ¿Entidades C# actualizadas en `SistemaIndustrial.AD/Models/` y `DbSets` en `AppDbContext.cs`?
- [ ] ¿Repositorios actualizados (`EF directo` para Categorías/Tiradas; `SpInvoker` para el resto)?
- [ ] ¿SPs actualizados/creados en `Docs/sql/` (nombres exactos `sp_*`)?
- [ ] ¿DROP 18 sigue pendiente hasta actualizar backend (`RegistroTachoRepository`)?
- [ ] ¿Rollback documentado?
- [ ] ¿Probado en LocalDB?

---

## Datos de Referencia (Seed Data)

Los datos de referencia (catálogos, configuraciones) se versionan en `Docs/sql/seed/`:
```
Docs/sql/seed/
├── 01_roles.sql           -- Roles del sistema
├── 02_unidades_medida.sql -- kg, lt, un, m, m2, m3
├── 03_estados_calidad.sql -- Aprobado, Revision, Rechazado
├── 04_estados_semana.sql  -- Planificada, EnProgreso, Completada, Cancelada
└── 99_admin_user.sql      -- Usuario admin inicial (solo dev)
```

**Regla**: Los seed data de producción NO incluyen usuario admin por seguridad.