# Troubleshooting

## Errores Comunes de Base de Datos

### 1. Error de Conexión
**Síntoma**: `Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server`

**Causas y Soluciones**:
| Causa | Verificación | Solución |
|-------|--------------|----------|
| SQL Server no corriendo | `systemctl status mssql-server` / Services.msc | Iniciar servicio |
| Puerto bloqueado | `telnet localhost 1433` / `Test-NetConnection` | Firewall: abrir 1433 |
| Connection String incorrecta | Revisar `appsettings.Production.json` | Corregir Server/Database/User/Password |
| SSL/TLS mismatch | `TrustServerCertificate=True/False` | Azure: `Encrypt=True;TrustServerCertificate=False` |
| Usuario sin permisos | `SELECT * FROM sys.database_principals` | Grant db_datareader/writer |

### 2. Login Failed for User
**Síntoma**: `Login failed for user 'industrial_user'`

**Soluciones**:
```sql
-- Verificar usuario existe
SELECT name, type_desc, is_disabled FROM sys.server_principals WHERE name = 'industrial_user';

-- Verificar en BD
USE SistemaIndustrialDB;
SELECT name, type_desc FROM sys.database_principals WHERE name = 'industrial_user';

-- Recrear login/usuario si necesario
DROP USER industrial_user;
DROP LOGIN industrial_user;
CREATE LOGIN industrial_user WITH PASSWORD = 'NuevoPassword123!';
CREATE USER industrial_user FOR LOGIN industrial_user;
ALTER ROLE db_datareader ADD MEMBER industrial_user;
ALTER ROLE db_datawriter ADD MEMBER industrial_user;
```

### 3. Violación de Constraint UNIQUE
**Síntoma**: `Violation of UNIQUE KEY constraint 'UQ_Semanas_NumeroAnio'. Cannot insert duplicate key.`

**Solución**: Validar en BR antes de insertar (ya implementado en `SemanaService`).

### 4. Violación de FK (DELETE)
**Síntoma**: `The DELETE statement conflicted with the REFERENCE constraint "FK_ObjetivosSemana_Semanas"`

**Solución**: Verificar datos dependientes antes de eliminar (implementado en `SemanaService.DeleteAsync`).

### 5. Timeout Expired
**Síntoma**: `Execution Timeout Expired. The timeout period elapsed prior to completion of the operation.`

**Soluciones**:
```json
// Aumentar timeout en Connection String
"ConnectionStrings": {
  "SqlServer": "Server=...;Connection Timeout=60;Command Timeout=120;"
}
```
```csharp
// O en DbContext
options.UseSqlServer(connectionString, sqlOptions => 
{
    sqlOptions.CommandTimeout(120);
});
```

---

## Errores de Autenticación

### 1. 401 Unauthorized - Token Expirado
**Síntoma**: `Token-Expired: true` header o 401 en requests

**Flujo Normal**:
1. Access token expira (8h)
2. Frontend detecta 401
3. Llama `POST /api/auth/refresh` con refresh token
4. Si refresh token válido → nuevos tokens
5. Si refresh token expirado (7d) → logout → redirect login

**Debug**:
```bash
# Verificar expiración JWT
echo "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZXhwIjoxNzYwMTEyMDAwfQ.xxx" | cut -d. -f2 | base64 -d | jq .exp
# Convertir exp a fecha: date -d @1760112000
```

### 2. 401 - Refresh Token Inválido
**Causas**:
- Refresh token ya usado (rotación)
- Refresh token expirado (>7 días)
- Usuario desactivado (`Activo = 0`)
- Hash no coincide (BCrypt)

**Solución**: Forzar logout y nuevo login.

### 3. JWT Validation Failed
**Síntoma**: `IDX10501: Signature validation failed.`

**Causas**:
- `Jwt:Key` diferente entre deploy y validación
- Clock skew (reloj desincronizado)
- Token manipulado

**Solución**: Verificar que `ASPNETCORE_JWT__KEY` sea idéntico en todos los nodos.

### 4. CORS Error
**Síntoma**: `Access to fetch at 'https://api.empresa.com' from origin 'https://industrial.empresa.com' has been blocked by CORS policy`

**Solución**: Verificar `Cors:AllowedOrigins` incluye exactamente `https://industrial.empresa.com` (sin trailing slash, con puerto si no es 443).

---

## Errores de Build / Deploy

### 1. dotnet publish Falla
**Síntoma**: `error NETSDK1045: The current .NET SDK does not support targeting .NET 9.0`

**Solución**: Instalar .NET 9 SDK: `https://dotnet.microsoft.com/download/dotnet/9.0`

### 2. Referencias de Proyecto Rotas
**Síntoma**: `error CS0246: The type or namespace name 'SistemaIndustrial.AD' could not be found`

**Solución**:
```bash
# Verificar .sln incluye todos los proyectos
dotnet sln list

# Restaurar desde raíz
dotnet restore SistemaIndustrial.sln
```

### 3. EF Core Tools No Encontrado
**Síntoma**: `Could not execute because the specified command or file was not found. dotnet-ef`

**Solución**: `dotnet tool install --global dotnet-ef --version 9.0.*`

### 4. Frontend Build Falla
**Síntoma**: `error:0308010C:digital envelope routines::unsupported` (Node.js 17+ con OpenSSL 3)

**Solución**:
```bash
# Opción 1: Node.js 20 LTS (recomendado)
nvm install 20
nvm use 20

# Opción 2: Legacy OpenSSL provider
export NODE_OPTIONS=--openssl-legacy-provider
npm run build
```

---

## Errores en Runtime

### 1. 500 Internal Server Error (Genérico)
**Debug**:
```bash
# Ver logs detallados
journalctl -u sistema-industrial-api -f

# O en Development
ASPNETCORE_ENVIRONMENT=Development dotnet run
# Ver stack trace en browser / Swagger
```

### 2. Memory Leak / Alto Consumo
**Síntoma**: API consume > 1GB RAM progresivamente

**Causas Comunes**:
- `DbContext` no disposed (Scoped OK, pero verificar no Singleton)
- Listas grandes sin paginación (`GetAll()` retorna todo)
- Logs excesivos en Debug level

**Solución**: Implementar paginación, revisar DI lifetimes, ajustar log level.

### 3. Deadlocks en BD
**Síntoma**: `Transaction (Process ID XX) was deadlocked on lock resources`

**Soluciones**:
- Índices apropiados en FKs
- Transacciones cortas
- `READ COMMITTED SNAPSHOT` isolation level
- Reintento con backoff en código

---

## Diagnósticos Avanzados

### Health Check Detallado
```bash
# Endpoint personalizado con más info
curl https://industrial.empresa.com/health/ready | jq .

# Verificar DB específicamente
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U industrial_user -P "***" -Q "SELECT 1 AS Connected;"
```

### Profiling API
```bash
# dotnet-trace (incluido en .NET SDK)
dotnet-trace collect --process-id $(pidof dotnet) --providers Microsoft-DotNETCore-SampleProfiler:0:1 --duration 00:01:00 --output trace.nettrace

# Analizar con PerfView / speedscope.app
```

### SQL Query Analysis
```sql
-- Queries lentas (Query Store habilitado en Azure SQL / SQL Server 2016+)
SELECT TOP 10 
    q.query_id,
    qt.query_sql_text,
    rs.avg_duration,
    rs.execution_count
FROM sys.query_store_query q
JOIN sys.query_store_query_text qt ON q.query_text_id = qt.query_text_id
JOIN sys.query_store_plan p ON q.query_id = p.query_id
JOIN sys.query_store_runtime_stats rs ON p.plan_id = rs.plan_id
ORDER BY rs.avg_duration DESC;

-- Índices faltantes
SELECT * FROM sys.dm_db_missing_index_details;
```

### Logs Estructurados (Seq)
```bash
# En Seq: buscar por
@Level = 'Error' AND @Exception IS NOT NULL

# O filtrar por request
RequestPath = '/api/semanas' AND Elapsed > 1000
```

---

## Contacto y Escalación

| Nivel | Contacto | SLA |
|-------|----------|-----|
| **Nivel 1** (Infra) | DevOps / SysAdmin | 1 hora |
| **Nivel 2** (App) | Backend Team | 4 horas |
| **Nivel 3** (Crítico) | Tech Lead + On-call | 30 min |

### Información para Reporte
```markdown
**Incidente**: [Breve descripción]
**Entorno**: Production / Staging / Development
**Hora**: YYYY-MM-DD HH:MM UTC
**Endpoint**: GET /api/semanas/1
**Error**: 500 / Timeout / 401
**Logs**: [Link a Seq / Journalctl lines]
**Pasos para reproducir**: 
1. ...
2. ...
**Impacto**: Usuarios afectados / Funcionalidad bloqueada
```