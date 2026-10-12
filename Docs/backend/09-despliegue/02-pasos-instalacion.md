# Pasos de Instalación

## 1. Preparación del Servidor

### Ubuntu 22.04 / 24.04 LTS
```bash
# Actualizar sistema
sudo apt update && sudo apt upgrade -y

# Instalar dependencias
sudo apt install -y nginx certbot python3-certbot-nginx curl gnupg2

# Instalar .NET 9 Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-9.0

# Verificar
dotnet --version  # Debe mostrar 9.0.x
```

### Windows Server 2022
```powershell
# Instalar .NET 9 Runtime
# Descargar de https://dotnet.microsoft.com/download/dotnet/9.0
# Ejecutar instalador aspnetcore-runtime-9.0-win-x64.exe

# Instalar SQL Server Express (opcional)
# Descargar de https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Configurar IIS (opcional, si no se usa Kestrel directo)
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-NetFxExtensibility45, IIS-HealthAndDiagnostics, IIS-HttpLogging, IIS-LoggingLibraries, IIS-RequestFiltering, IIS-Security, IIS-Performance
```

---

## 2. Base de Datos

### Opción A: SQL Server Local (Mismo servidor)
```bash
# Ubuntu - SQL Server 2022 Express en Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Industrial2026" \
   -p 1433:1433 --name sqlserver \
   -v sql_data:/var/opt/mssql \
   -d mcr.microsoft.com/mssql/server:2022-latest

# Esperar a que esté listo (30-60s)
docker logs -f sqlserver
```

### Opción B: Azure SQL Database
```bash
# Crear via Azure CLI
az sql server create -n industrial-sql-prod -g rg-prod -l eastus -u sqladmin -p "StrongPassword123!"
az sql db create -s industrial-sql-prod -g rg-prod -n SistemaIndustrialDB --service-objective S0
az sql server firewall-rule create -s industrial-sql-prod -g rg-prod -n AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### Ejecutar Scripts DDL
```bash
# Clonar repo o copiar scripts
git clone https://github.com/empresa/sistema-industrial.git /tmp/sistema-industrial
cd /tmp/sistema-industrial

# Ejecutar en orden
for script in Docs/sql/*.sql; do
    echo "Ejecutando $script..."
    /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Industrial2026" -d SistemaIndustrialDB -i "$script"
done

# Verificar
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Industrial2026" -d SistemaIndustrialDB -Q "SELECT COUNT(*) FROM Usuarios;"
```

---

## 3. Backend - Publicar y Configurar

### Publicar Aplicación
```bash
# En máquina de build / CI
cd SistemaIndustrial.API
dotnet publish -c Release -o ./publish --self-contained false

# Copiar al servidor
scp -r ./publish/* usuario@servidor:/opt/sistema-industrial/api/
```

### Configurar appsettings.Production.json
```bash
# En servidor
sudo -u sistema-industrial tee /opt/sistema-industrial/api/appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "ClaveSuperSecretaDe64CaracteresMinimoParaProduccion!!",
    "Issuer": "SistemaIndustrial",
    "Audience": "SistemaIndustrialClient"
  },
  "Cors": {
    "AllowedOrigins": ["https://industrial.empresa.com"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "industrial.empresa.com",
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:8080" },
      "Https": {
        "Url": "https://+:8081",
        "Certificate": {
          "Path": "/opt/sistema-industrial/ssl/cert.pem",
          "KeyPath": "/opt/sistema-industrial/ssl/key.pem"
        }
      }
    }
  }
}
EOF
```

### Crear Usuario y Permisos
```bash
# Usuario sistema
sudo useradd -r -s /bin/false -d /opt/sistema-industrial sistema-industrial

# Directorio logs
sudo mkdir -p /opt/sistema-industrial/api/logs
sudo chown -R sistema-industrial:sistema-industrial /opt/sistema-industrial
sudo chmod 750 /opt/sistema-industrial
sudo chmod 640 /opt/sistema-industrial/api/appsettings.Production.json
```

### Systemd Service
```bash
sudo tee /etc/systemd/system/sistema-industrial-api.service << 'EOF'
[Unit]
Description=Sistema Industrial API
After=network.target sqlserver.service
Requires=sqlserver.service

[Service]
Type=notify
User=sistema-industrial
Group=sistema-industrial
WorkingDirectory=/opt/sistema-industrial/api
ExecStart=/usr/bin/dotnet /opt/sistema-industrial/api/SistemaIndustrial.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=sistema-industrial-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:8080;https://+:8081
# Variables de entorno para secrets (alternativa a appsettings)
# Environment=ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER=Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True
# Environment=ASPNETCORE_JWT__KEY=ClaveSuperSecretaDe64CaracteresMinimoParaProduccion!!

# Seguridad
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/sistema-industrial/api/logs
CapabilityBoundingSet=CAP_NET_BIND_SERVICE
AmbientCapabilities=CAP_NET_BIND_SERVICE

[Install]
WantedBy=multi-user.target
EOF

# Recargar y habilitar
sudo systemctl daemon-reload
sudo systemctl enable sistema-industrial-api
sudo systemctl start sistema-industrial-api
sudo systemctl status sistema-industrial-api
```

### Verificar API
```bash
# Health check
curl -f http://localhost:8080/health
curl -f https://localhost:8081/health -k  # -k para cert auto-firmado

# Logs
journalctl -u sistema-industrial-api -f
```

---

## 4. Frontend - Build y Despliegue

### Build de Producción
```bash
# En máquina de build
cd Frontend
npm ci
npm run build  # Genera dist/

# Copiar al servidor
scp -r ./dist/* usuario@servidor:/opt/sistema-industrial/frontend/
```

### Configurar Nginx
```bash
sudo tee /etc/nginx/sites-available/industrial << 'EOF'
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
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    
    root /opt/sistema-industrial/frontend;
    index index.html;
    
    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # Assets cache
    location /assets/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # API Proxy
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
        proxy_read_timeout 90s;
    }
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN";
    add_header X-Content-Type-Options "nosniff";
    add_header X-XSS-Protection "1; mode=block";
    add_header Referrer-Policy "strict-origin-when-cross-origin";
    add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' https://industrial.empresa.com;";
    
    # Gzip
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;
}
EOF

# Habilitar sitio
sudo ln -sf /etc/nginx/sites-available/industrial /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Certificados SSL (Let's Encrypt)
```bash
# Obtener certificado
sudo certbot --nginx -d industrial.empresa.com --non-interactive --agree-tos -m admin@empresa.com

# Verificar renovación automática
sudo systemctl status certbot.timer
```

---

## 5. Configurar Primer Usuario Admin

```bash
# Solo en Development/Staging - En producción usar script seguro
curl -X POST https://industrial.empresa.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@empresa.com",
    "password": "PasswordSeguro123!",
    "nombre": "Administrador",
    "role": "Admin"
  }'
```

**Producción**: Usar script de inicialización seguro o crear usuario via SQL con hash BCrypt pre-generado.

---

## 6. Verificación Post-Instalación

### Checklist Funcional
```bash
# 1. API Health
curl -f https://industrial.empresa.com/health

# 2. Login
curl -X POST https://industrial.empresa.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@empresa.com","password":"PasswordSeguro123!"}'

# 3. Crear semana (con token del login anterior)
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
curl -X POST https://industrial.empresa.com/api/semanas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"numeroSemana":1,"anio":2026,"fechaInicio":"2026-01-05T00:00:00","fechaFin":"2026-01-11T00:00:00"}'

# 4. Frontend accesible
curl -f https://industrial.empresa.com/ | grep -q "Sistema Industrial"

# 5. Dashboard métricas
curl -X GET https://industrial.empresa.com/api/dashboard/metricas/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Verificar Logs
```bash
# API logs
journalctl -u sistema-industrial-api --since "1 hour ago" | grep -i error

# Nginx logs
sudo tail -f /var/log/nginx/access.log /var/log/nginx/error.log
```

---

## 7. Backup y Mantenimiento

### Backup Diario BD (Cron)
```bash
# Script backup
sudo tee /opt/sistema-industrial/scripts/backup-db.sh << 'EOF'
#!/bin/bash
set -e
BACKUP_DIR="/opt/sistema-industrial/backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="SistemaIndustrialDB"
CONTAINER="sqlserver"

mkdir -p "$BACKUP_DIR"

/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Industrial2026" \
  -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_DIR/${DB_NAME}_$DATE.bak' WITH NOFORMAT, NOINIT, NAME = N'$DB_NAME-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Mantener últimos 30 días
find "$BACKUP_DIR" -name "${DB_NAME}_*.bak" -mtime +30 -delete

echo "Backup completado: ${DB_NAME}_$DATE.bak"
EOF

chmod +x /opt/sistema-industrial/scripts/backup-db.sh

# Crontab (2 AM daily)
sudo crontab -u sistema-industrial -e
# 0 2 * * * /opt/sistema-industrial/scripts/backup-db.sh >> /opt/sistema-industrial/api/logs/backup.log 2>&1
```

### Log Rotation
```bash
sudo tee /etc/logrotate.d/sistema-industrial << 'EOF'
/opt/sistema-industrial/api/logs/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 640 sistema-industrial sistema-industrial
    sharedscripts
    postrotate
        systemctl reload sistema-industrial-api > /dev/null 2>&1 || true
    endscript
}

/var/log/nginx/industrial*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 640 www-data www-data
    sharedscripts
    postrotate
        systemctl reload nginx > /dev/null 2>&1 || true
    endscript
}
EOF
```

---

## 8. Rollback Plan

### Backend Rollback
```bash
# Mantener versión anterior en /opt/sistema-industrial/api-previous/
# En deploy.sh:
#   mv /opt/sistema-industrial/api /opt/sistema-industrial/api-$(date +%Y%m%d_%H%M%S)
#   mv /opt/sistema-industrial/api-new /opt/sistema-industrial/api

# Rollback rápido:
sudo systemctl stop sistema-industrial-api
mv /opt/sistema-industrial/api /opt/sistema-industrial/api-failed
mv /opt/sistema-industrial/api-previous /opt/sistema-industrial/api
sudo systemctl start sistema-industrial-api
```

### Database Rollback
```bash
# Restaurar desde backup
/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "****" \
  -Q "RESTORE DATABASE [SistemaIndustrialDB] FROM DISK = N'/opt/sistema-industrial/backups/SistemaIndustrialDB_20261010_020000.bak' WITH REPLACE, RECOVERY"
```

### Frontend Rollback
```bash
# Similar a backend - mantener dist/ anterior
mv /opt/sistema-industrial/frontend /opt/sistema-industrial/frontend-failed
mv /opt/sistema-industrial/frontend-previous /opt/sistema-industrial/frontend
sudo systemctl reload nginx
```