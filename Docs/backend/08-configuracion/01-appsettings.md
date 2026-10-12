# AppSettings

## Estructura JSON

### appsettings.json (Base - LocalDB / Windows Auth)
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=SistemaIndustrialDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SistemaIndustrial@JWT#2026!ClaveSecretaMuyLarga$ParaFirmarTokens*32chars",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json (Desarrollo - SQL Auth + JWT)
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "SistemaIndustrial@JWT#2026!ClaveSecretaMuyLarga$ParaFirmarTokens*32chars",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Production.json (Producción)
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=prod-sql.empresa.com;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=${DB_PASSWORD};TrustServerCertificate=False;Encrypt=True;"
  },
  "Jwt": {
    "Key": "${JWT_KEY}",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  },
  "Cors": {
    "AllowedOrigins": ["https://industrial.empresa.com"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "/var/log/sistema-industrial/api-.log", "rollingInterval": "Day" } },
      { "Name": "Seq", "Args": { "serverUrl": "https://seq.empresa.com", "apiKey": "${SEQ_API_KEY}" } }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "AllowedHosts": "industrial.empresa.com"
}
```

---

## Connection Strings

### LocalDB (Desarrollo Windows)
```json
"Server=(localdb)\\MSSQLLocalDB;Database=SistemaIndustrialDB;Trusted_Connection=True;TrustServerCertificate=True;"
```
- Windows Authentication
- Base de datos local por usuario
- Ideal para desarrollo individual

### SQL Server Express / Developer (Desarrollo Equipo)
```json
"Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
```
- SQL Authentication
- Compartido entre desarrolladores
- `TrustServerCertificate=True` para certificados auto-firmados

### Producción (Azure SQL / SQL Server)
```json
"Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```
- `Encrypt=True` obligatorio en Azure
- `TrustServerCertificate=False` valida certificado CA
- Timeout explícito
- Password desde Key Vault / Variables de entorno

---

## JWT Configuration

### Clave de Firma (Key)
- **Mínimo**: 32 caracteres (256 bits para HS256)
- **Recomendado**: 64+ caracteres aleatorios
- **Generación**: `openssl rand -base64 64`
- **Rotación**: Cada 90 días (requiere invalidar tokens activos)

```json
"Jwt": {
  "Key": "ClaveMuyLargaYAleatoriaDeAlMenos32CaracteresParaHS256!!",
  "Issuer": "SistemaIndustrial",
  "Audience": "SistemaIndustrialClient"
}
```

### Parámetros
| Parámetro | Descripción | Valor Típico |
|-----------|-------------|--------------|
| `Key` | Clave secreta firma HS256 | 32-64 chars aleatorios |
| `Issuer` | Emisor del token | `SistemaIndustrial` |
| `Audience` | Destinatario esperado | `SistemaIndustrialClient` |
| `AccessTokenExpiry` | Expiración access token | 8 horas (hardcoded) |
| `RefreshTokenExpiry` | Expiración refresh token | 7 días (hardcoded) |

---

## CORS Configuration

### Desarrollo
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
}
```

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? ["http://localhost:5173"];
        
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Necesario para cookies futuras
    });
});
```

### Producción
```json
"Cors": {
  "AllowedOrigins": ["https://industrial.empresa.com"]
}
```

**Reglas**:
- **Nunca** `AllowAnyOrigin()` con `AllowCredentials()` en producción
- Orígenes explícitos, sin wildcards
- HTTPS obligatorio en producción

---

## Logging

### Niveles por Entorno
| Entorno | Default | Microsoft | EF Core |
|---------|---------|-----------|---------|
| Development | Debug | Information | Information |
| Staging | Information | Warning | Warning |
| Production | Information | Warning | Warning |

### Serilog (Producción - Futuro)
```json
"Serilog": {
  "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "/var/log/sistema-industrial/api-.log", "rollingInterval": "Day", "retainedFileCountLimit": 30 } },
    { "Name": "Seq", "Args": { "serverUrl": "https://seq.empresa.com", "apiKey": "${SEQ_API_KEY}" } }
  ],
  "Enrich": ["FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId"],
  "Properties": {
    "Application": "SistemaIndustrial.API",
    "Environment": "Production"
  }
}
```

---

## Variables de Entorno (Override)

### Prioridad de Configuración
1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (Development/Staging/Production)
3. **Variables de entorno** (prefijo `ASPNETCORE_` o directo)
4. **User Secrets** (desarrollo)
5. **Azure Key Vault / AWS Secrets Manager** (producción)

### Variables Críticas
```bash
# Base de datos
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=...;Database=...;User Id=...;Password=..."

# JWT
ASPNETCORE_JWT__KEY="ClaveSuperSecretaDe64CaracteresMinimoParaProduccion!!"
ASPNETCORE_JWT__ISSUER="SistemaIndustrial"
ASPNETCORE_JWT__AUDIENCE="SistemaIndustrialClient"

# CORS
ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://industrial.empresa.com"

# Logging (Serilog/Seq)
SEQ_API_KEY="..."
```

### Docker Compose (Ejemplo)
```yaml
services:
  api:
    image: sistema-industrial-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER=Server=sql;Database=SistemaIndustrialDB;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=False;Encrypt=True
      - ASPNETCORE_JWT__KEY=${JWT_KEY}
      - ASPNETCORE_CORS__ALLOWEDORIGINS__0=https://industrial.empresa.com
    secrets:
      - db_password
      - jwt_key
    ports:
      - "8080:8080"

secrets:
  db_password:
    external: true
  jwt_key:
    external: true
```

### Kubernetes (Ejemplo)
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: sistema-industrial-secrets
type: Opaque
stringData:
  DB_PASSWORD: "..."
  JWT_KEY: "..."
---
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER
          valueFrom:
            secretKeyRef:
              name: sistema-industrial-secrets
              key: DB_PASSWORD
        - name: ASPNETCORE_JWT__KEY
          valueFrom:
            secretKeyRef:
              name: sistema-industrial-secrets
              key: JWT_KEY
```

---

## Feature Flags (Futuro)
```json
"Features": {
  "EnableSwagger": true,
  "EnableRateLimiting": false,
  "EnableMfa": false,
  "EnableAuditLog": true,
  "MaintenanceMode": false
}
```