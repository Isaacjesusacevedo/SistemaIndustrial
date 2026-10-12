# Requisitos Previos

## Software Requerido

### Servidor de Aplicaciones
| Componente | Versión | Notas |
|------------|---------|-------|
| .NET Runtime | 9.0 (LTS) | `dotnet --version` |
| ASP.NET Core Runtime | 9.0 | Incluido en .NET Runtime |
| SQL Server | 2019+ / Express / Azure SQL | Compatibilidad EF Core 9 |

### Herramientas de Desarrollo
| Herramienta | Versión | Uso |
|-------------|---------|-----|
| .NET SDK | 9.0 | Build, publish, EF Core tools |
| Visual Studio 2022 / VS Code | Latest | IDE |
| Git | 2.40+ | Control de versiones |
| Docker | 24+ | Contenedores (opcional) |
| Node.js | 20+ (LTS) | Frontend build |
| npm | 10+ | Package manager |

### Base de Datos
| Opción | Requisitos |
|--------|------------|
| **LocalDB** | Incluido con Visual Studio / .NET SDK |
| **SQL Server Express** | Gratis, hasta 10GB, 1 socket, 1GB RAM |
| **SQL Server Developer** | Gratis para desarrollo, características Enterprise |
| **Azure SQL Database** | PaaS, escalable, backup automático |
| **Docker SQL Server** | `mcr.microsoft.com/mssql/server:2022-latest` |

---

## Puertos Requeridos

| Servicio | Puerto | Protocolo | Descripción |
|----------|--------|-----------|-------------|
| API HTTP | 5127 (dev) / 8080 (prod) | HTTP | Endpoints REST |
| API HTTPS | 7244 (dev) / 8081 (prod) | HTTPS | Endpoints REST seguros |
| SQL Server | 1433 | TCP | Base de datos |
| Frontend Dev | 5173 | HTTP | Vite dev server |
| Seq (Logs) | 5341 / 80 | HTTP | Structured logging (opcional) |

### Firewall Rules
```bash
# Windows (PowerShell Admin)
New-NetFirewallRule -DisplayName "Sistema Industrial API HTTP" -Direction Inbound -LocalPort 5127 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Sistema Industrial API HTTPS" -Direction Inbound -LocalPort 7244 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -LocalPort 1433 -Protocol TCP -Action Allow

# Linux (ufw)
sudo ufw allow 8080/tcp comment 'Sistema Industrial API'
sudo ufw allow 1433/tcp comment 'SQL Server'
```

---

## Estructura de Directorios (Producción)

```
/opt/sistema-industrial/
├── api/                    # Backend publicado
│   ├── SistemaIndustrial.API.dll
│   ├── *.dll
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── logs/               # Logs de aplicación
├── frontend/               # Frontend build estático
│   ├── index.html
│   ├── assets/
│   └── ...
├── scripts/                # Scripts de despliegue/mantenimiento
│   ├── deploy.sh
│   ├── backup-db.sh
│   └── restore-db.sh
├── backups/                # Backups de BD
│   └── YYYYMMDD_HHMMSS.bak
└── ssl/                    # Certificados TLS
    ├── cert.pem
    └── key.pem
```

### Permisos
```bash
# Usuario dedicado (no root)
sudo useradd -r -s /bin/false -d /opt/sistema-industrial sistema-industrial
sudo chown -R sistema-industrial:sistema-industrial /opt/sistema-industrial
sudo chmod 750 /opt/sistema-industrial
sudo chmod 640 /opt/sistema-industrial/api/appsettings.Production.json
```

---

## Certificados TLS (Producción)

### Let's Encrypt (Certbot)
```bash
# Instalar certbot
sudo apt install certbot python3-certbot-nginx

# Obtener certificado
sudo certbot --nginx -d industrial.empresa.com -d admin.industrial.empresa.com

# Renovación automática (systemd timer incluido)
sudo systemctl enable certbot.timer
```

### Certificados Manuales
```bash
# Estructura
/opt/sistema-industrial/ssl/
├── cert.pem          # Certificado público + cadena intermedia
├── key.pem           # Clave privada (solo lectura owner)
└── ca-bundle.pem     # CA bundle (opcional)

# Permisos
chmod 640 cert.pem key.pem
chown sistema-industrial:sistema-industrial cert.pem key.pem
```

### Configuración ASP.NET Core (Kestrel)
```json
// appsettings.Production.json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://+:8080"
    },
    "Https": {
      "Url": "https://+:8081",
      "Certificate": {
        "Path": "/opt/sistema-industrial/ssl/cert.pem",
        "KeyPath": "/opt/sistema-industrial/ssl/key.pem",
        "Password": ""  // Si la key tiene password, usar variable de entorno
      }
    }
  }
}
```

---

## Base de Datos - Preparación

### Crear Login y Usuario (SQL Server)
```sql
-- En master
CREATE LOGIN industrial_user WITH PASSWORD = 'PasswordSeguro123!';
GO

-- En SistemaIndustrialDB
CREATE USER industrial_user FOR LOGIN industrial_user;
GO

-- Permisos mínimos necesarios
ALTER ROLE db_datareader ADD MEMBER industrial_user;
ALTER ROLE db_datawriter ADD MEMBER industrial_user;
ALTER ROLE db_ddladmin ADD MEMBER industrial_user; -- Para migraciones/EF Core
-- O específico:
GRANT EXECUTE ON SCHEMA::dbo TO industrial_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO industrial_user;
```

### Ejecutar Scripts DDL
```bash
# Opción 1: sqlcmd (Windows)
sqlcmd -S localhost -U industrial_user -P "PasswordSeguro123!" -d SistemaIndustrialDB -i Docs/sql/01_create_database.sql
sqlcmd -S localhost -U industrial_user -P "PasswordSeguro123!" -d SistemaIndustrialDB -i Docs/sql/02_create_indexes.sql
# ... resto de scripts en orden

# Opción 2: sqlcmd (Linux/Docker)
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U industrial_user -P "PasswordSeguro123!" -d SistemaIndustrialDB -i /opt/sistema-industrial/scripts/01_create_database.sql

# Opción 3: Azure Data Studio / SSMS
# Abrir cada .sql y ejecutar en orden
```

### Verificar Instalación
```sql
-- Verificar tablas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Verificar SPs
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE';

-- Verificar usuario admin
SELECT Id, Email, Nombre, Role, Activo FROM Usuarios WHERE Email = 'admin@industrial.com';

-- Probar SP
EXEC sp_Semanas_GetAll;
```

---

## Frontend - Build de Producción

### Requisitos
- Node.js 20+ LTS
- npm 10+

### Build
```bash
cd Frontend
npm ci                    # Install exacto desde package-lock.json
npm run build             # Genera carpeta dist/
```

### Verificar Build
```bash
ls -la dist/
# index.html, assets/index-<hash>.js, assets/index-<hash>.css, etc.
```

### Servir con Nginx (Producción)
```nginx
# /etc/nginx/sites-available/industrial
server {
    listen 80;
    server_name industrial.empresa.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name industrial.empresa.com;
    
    ssl_certificate /opt/sistema-industrial/ssl/cert.pem;
    ssl_certificate_key /opt/sistema-industrial/ssl/key.pem;
    
    root /opt/sistema-industrial/frontend;
    index index.html;
    
    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # Cache assets
    location /assets/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # Proxy API
    location /api/ {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN";
    add_header X-Content-Type-Options "nosniff";
    add_header X-XSS-Protection "1; mode=block";
    add_header Referrer-Policy "strict-origin-when-cross-origin";
}
```

---

## Monitoreo Básico

### Health Checks
```bash
# API Health
curl -f http://localhost:8080/health
curl -f http://localhost:8080/health/ready
curl -f http://localhost:8080/health/live

# DB Connectivity
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U industrial_user -P "****" -Q "SELECT 1"
```

### Logs
```bash
# Journalctl (systemd)
journalctl -u sistema-industrial-api -f

# Archivos (Serilog/File sink)
tail -f /opt/sistema-industrial/api/logs/api-$(date +%Y%m%d).log

# Seq (si configurado)
# https://seq.empresa.com
```

### Métricas Básicas (Prometheus - Futuro)
```yaml
# Prometheus scrape config
- job_name: 'sistema-industrial-api'
  static_configs:
  - targets: ['localhost:8080']
  metrics_path: '/metrics'  # Requiere endpoint /metrics (OpenTelemetry)
```

---

## Checklist Pre-Despliegue

- [ ] .NET 9 Runtime instalado
- [ ] SQL Server accesible y BD creada
- [ ] Scripts DDL ejecutados en orden
- [ ] Usuario BD con permisos correctos
- [ ] Puertos abiertos en firewall
- [ ] Certificados TLS válidos (prod)
- [ ] Variables de entorno configuradas
- [ ] `appsettings.Production.json` sin secrets
- [ ] Secrets en Key Vault / Docker secrets / K8s secrets
- [ ] Frontend build `dist/` generado
- [ ] Nginx/Reverse proxy configurado
- [ ] Health checks respondiendo
- [ ] Logs rotando (no llenan disco)
- [ ] Backup BD programado
- [ ] Rollback plan documentado