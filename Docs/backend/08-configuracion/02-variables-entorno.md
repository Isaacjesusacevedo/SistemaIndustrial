# Variables de Entorno

## Visión General

Las variables de entorno permiten configurar la aplicación sin modificar código ni archivos de configuración, esencial para:
- **Seguridad**: Secrets fuera del repositorio
- **Portabilidad**: Mismo build en Dev/Staging/Prod
- **CI/CD**: Inyección automática en pipelines

---

## Variables por Categoría

### Aplicación Base

| Variable | Descripción | Requerida | Default |
|----------|-------------|-----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno actual | Sí | `Production` |
| `ASPNETCORE_URLS` | URLs de escucha | No | `http://+:8080` |
| `ASPNETCORE_HTTPS_PORT` | Puerto HTTPS | No | `8081` |

### Base de Datos

| Variable | Descripción | Requerida | Ejemplo |
|----------|-------------|-----------|---------|
| `ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER` | Connection string completa | Sí | `Server=...;Database=...;User=...;Password=...` |
| `DB_SERVER` | Servidor (para componer) | No | `prod-sql.database.windows.net` |
| `DB_NAME` | Nombre BD | No | `SistemaIndustrialDB` |
| `DB_USER` | Usuario | No | `industrial_prod` |
| `DB_PASSWORD` | Contraseña | Sí* | `***` |

*Si no se usa `ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER` completa.

### JWT / Autenticación

| Variable | Descripción | Requerida | Ejemplo |
|----------|-------------|-----------|---------|
| `ASPNETCORE_JWT__KEY` | Clave firma HS256 (32+ chars) | Sí | `ClaveSuperSecretaDe64CaracteresMinimo!!` |
| `ASPNETCORE_JWT__ISSUER` | Emisor token | No | `SistemaIndustrial` |
| `ASPNETCORE_JWT__AUDIENCE` | Audiencia token | No | `SistemaIndustrialClient` |
| `ASPNETCORE_JWT__ACCESSTOKENEXPIRYHOURS` | Horas expiración access | No | `8` |
| `ASPNETCORE_JWT__REFRESHTOKENEXPIRYDAYS` | Días expiración refresh | No | `7` |

### CORS

| Variable | Descripción | Requerida | Ejemplo |
|----------|-------------|-----------|---------|
| `ASPNETCORE_CORS__ALLOWEDORIGINS__0` | Origen 1 | Sí* | `https://industrial.empresa.com` |
| `ASPNETCORE_CORS__ALLOWEDORIGINS__1` | Origen 2 | No | `https://admin.industrial.empresa.com` |

*Array indexado: `__0`, `__1`, etc.

### Logging / Observabilidad

| Variable | Descripción | Requerida | Ejemplo |
|----------|-------------|-----------|---------|
| `ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT` | Nivel log default | No | `Information` |
| `SEQ_API_KEY` | API Key Seq | No* | `...` |
| `SEQ_SERVER_URL` | URL Seq | No* | `https://seq.empresa.com` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure Monitor | No* | `InstrumentationKey=...` |

*Solo si se usa el sink correspondiente.

### Feature Flags

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ASPNETCORE_FEATURES__ENABLESWAGGER` | Habilitar Swagger | `false` (prod) |
| `ASPNETCORE_FEATURES__ENABLERATELIMITING` | Rate limiting | `false` |
| `ASPNETCORE_FEATURES__ENABLEAUDITLOG` | Auditoría | `true` |
| `ASPNETCORE_FEATURES__MAINTENANCEMODE` | Modo mantenimiento | `false` |

---

## Entornos

### Desarrollo Local (.env local / User Secrets)
```bash
# .env (no commitear) o User Secrets
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=(localdb)\\MSSQLLocalDB;Database=SistemaIndustrialDB;Trusted_Connection=True;TrustServerCertificate=True;"
ASPNETCORE_JWT__KEY="ClaveDesarrolloNoUsarEnProduccionMinimo32Chars!!"
ASPNETCORE_CORS__ALLOWEDORIGINS__0="http://localhost:5173"
ASPNETCORE_FEATURES__ENABLESWAGGER="true"
```

**User Secrets (Recomendado Dev)**:
```bash
cd SistemaIndustrial.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=localhost;Database=SistemaIndustrialDB;User Id=industrial_user;Password=Industrial2026;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "ClaveDesarrolloNoUsarEnProduccionMinimo32Chars!!"
```

### Development (Servidor Compartido)
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=dev-sql;Database=SistemaIndustrialDB_Dev;User Id=dev_user;Password=${DEV_DB_PASS};TrustServerCertificate=True;"
ASPNETCORE_JWT__KEY="ClaveDesarrolloCompartidaMinimo32CaracteresAleatorios!!"
ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://dev.industrial.empresa.com"
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT="Debug"
```

### Staging / QA
```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=staging-sql;Database=SistemaIndustrialDB_Staging;User Id=staging_user;Password=${STAGING_DB_PASS};Encrypt=True;TrustServerCertificate=False;"
ASPNETCORE_JWT__KEY="${STAGING_JWT_KEY}"
ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://staging.industrial.empresa.com"
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT="Information"
ASPNETCORE_FEATURES__ENABLESWAGGER="true"
ASPNETCORE_FEATURES__ENABLERATELIMITING="true"
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=${PROD_DB_PASS};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
ASPNETCORE_JWT__KEY="${PROD_JWT_KEY}"
ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://industrial.empresa.com"
ASPNETCORE_CORS__ALLOWEDORIGINS__1="https://admin.industrial.empresa.com"
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT="Information"
ASPNETCORE_FEATURES__ENABLESWAGGER="false"
ASPNETCORE_FEATURES__ENABLERATELIMITING="true"
ASPNETCORE_FEATURES__ENABLEAUDITLOG="true"
SEQ_API_KEY="${SEQ_API_KEY}"
SEQ_SERVER_URL="https://seq.empresa.com"
```

---

## Inyección en CI/CD

### GitHub Actions
```yaml
# .github/workflows/deploy.yml
env:
  ASPNETCORE_ENVIRONMENT: Production
  
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Set up .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Build
        run: dotnet publish SistemaIndustrial.API -c Release -o ./publish
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'sistema-industrial-api'
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        env:
          ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER: ${{ secrets.PROD_CONNECTION_STRING }}
          ASPNETCORE_JWT__KEY: ${{ secrets.PROD_JWT_KEY }}
          ASPNETCORE_CORS__ALLOWEDORIGINS__0: ${{ secrets.PROD_CORS_ORIGIN }}
          SEQ_API_KEY: ${{ secrets.SEQ_API_KEY }}
```

### GitLab CI
```yaml
# .gitlab-ci.yml
variables:
  ASPNETCORE_ENVIRONMENT: "Production"
  
deploy_production:
  stage: deploy
  image: mcr.microsoft.com/dotnet/aspnet:9.0
  script:
    - dotnet SistemaIndustrial.API.dll
  environment:
    name: production
    url: https://industrial.empresa.com
  variables:
    ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER: $PROD_CONNECTION_STRING
    ASPNETCORE_JWT__KEY: $PROD_JWT_KEY
  only:
    - main
```

### Azure DevOps
```yaml
# azure-pipelines.yml
variables:
  - group: sistema-industrial-secrets  # Variable group con secrets
  
stages:
- stage: Deploy
  jobs:
  - deployment: DeployAPI
    environment: Production
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure-RG-Prod'
              appName: 'sistema-industrial-api'
              package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
            env:
              ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER: $(PROD_CONNECTION_STRING)
              ASPNETCORE_JWT__KEY: $(PROD_JWT_KEY)
```

---

## Docker

### Dockerfile (Multi-stage)
```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln .
COPY SistemaIndustrial.AD/*.csproj ./SistemaIndustrial.AD/
COPY SistemaIndustrial.BR/*.csproj ./SistemaIndustrial.BR/
COPY SistemaIndustrial.API/*.csproj ./SistemaIndustrial.API/
RUN dotnet restore
COPY . .
RUN dotnet publish SistemaIndustrial.API -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SistemaIndustrial.API.dll"]
```

### docker-compose.yml (Desarrollo)
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Industrial2026
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Industrial2026" -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 10

  api:
    build: .
    ports:
      - "5127:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER=Server=sqlserver;Database=SistemaIndustrialDB;User Id=sa;Password=Industrial2026;TrustServerCertificate=True;
      - ASPNETCORE_JWT__KEY=ClaveDesarrolloDockerMinimo32CaracteresAleatorios!!
      - ASPNETCORE_CORS__ALLOWEDORIGINS__0=http://localhost:5173
    depends_on:
      sqlserver:
        condition: service_healthy
    volumes:
      - ./SistemaIndustrial.API/appsettings.Development.json:/app/appsettings.Development.json:ro

  frontend:
    build: ./Frontend
    ports:
      - "5173:5173"
    environment:
      - VITE_API_URL=http://localhost:5127
    depends_on:
      - api

volumes:
  sql_data:
```

### docker-compose.prod.yml (Producción)
```yaml
version: '3.8'

services:
  api:
    image: ghcr.io/empresa/sistema-industrial-api:${VERSION:-latest}
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '1'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    secrets:
      - db_connection_string
      - jwt_key
      - cors_origins
    ports:
      - "8080:8080"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 5s
      retries: 3
      start_period: 20s
    networks:
      - backend

secrets:
  db_connection_string:
    external: true
  jwt_key:
    external: true
  cors_origins:
    external: true

networks:
  backend:
    external: true
```

---

## Kubernetes

### ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: sistema-industrial-config
  namespace: industrial
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  ASPNETCORE_JWT__ISSUER: "SistemaIndustrial"
  ASPNETCORE_JWT__AUDIENCE: "SistemaIndustrialClient"
  ASPNETCORE_CORS__ALLOWEDORIGINS__0: "https://industrial.empresa.com"
  ASPNETCORE_CORS__ALLOWEDORIGINS__1: "https://admin.industrial.empresa.com"
  ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT: "Information"
  ASPNETCORE_FEATURES__ENABLESWAGGER: "false"
  ASPNETCORE_FEATURES__ENABLERATELIMITING: "true"
  ASPNETCORE_FEATURES__ENABLEAUDITLOG: "true"
```

### Secret
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: sistema-industrial-secrets
  namespace: industrial
type: Opaque
stringData:
  ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER: "Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=...;Encrypt=True;TrustServerCertificate=False;"
  ASPNETCORE_JWT__KEY: "ClaveSuperSecretaDe64CaracteresMinimoParaProduccion!!"
  SEQ_API_KEY: "..."
```

### Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sistema-industrial-api
  namespace: industrial
spec:
  replicas: 3
  selector:
    matchLabels:
      app: sistema-industrial-api
  template:
    metadata:
      labels:
        app: sistema-industrial-api
    spec:
      containers:
      - name: api
        image: ghcr.io/empresa/sistema-industrial-api:v1.0.0
        ports:
        - containerPort: 8080
        envFrom:
        - configMapRef:
            name: sistema-industrial-config
        - secretRef:
            name: sistema-industrial-secrets
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 20
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 10
      imagePullSecrets:
      - name: ghcr-secret
```

### Service + Ingress
```yaml
apiVersion: v1
kind: Service
metadata:
  name: sistema-industrial-api
  namespace: industrial
spec:
  selector:
    app: sistema-industrial-api
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: sistema-industrial-ingress
  namespace: industrial
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
spec:
  tls:
  - hosts:
    - industrial.empresa.com
    secretName: industrial-tls
  rules:
  - host: industrial.empresa.com
    http:
      paths:
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: sistema-industrial-api
            port:
              number: 80
```

---

## Validación de Variables

### Script de Verificación (Startup)
```csharp
// En Program.cs - validar variables críticas al inicio
var requiredVars = new[]
{
    "ConnectionStrings:SqlServer",
    "Jwt:Key"
};

foreach (var varName in requiredVars)
{
    var value = builder.Configuration[varName];
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Configuración requerida faltante: {varName}");
    }
}

// Validar longitud JWT Key
var jwtKey = builder.Configuration["Jwt:Key"];
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key debe tener al menos 32 caracteres (256 bits para HS256).");
}

// Validar que no sea clave por defecto en producción
if (builder.Environment.IsProduction() && jwtKey.Contains("Desarrollo", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT Key de desarrollo detectada en producción.");
}
```

---

## Checklist de Despliegue

- [ ] `ASPNETCORE_ENVIRONMENT` correcto
- [ ] Connection String válida y accesible
- [ ] JWT Key ≥ 32 chars, aleatoria, única por entorno
- [ ] CORS origins exactos (HTTPS en prod)
- [ ] Logging configurado (Serilog/Seq/AppInsights)
- [ ] Health checks responden
- [ ] HTTPS/TLS configurado (Ingress/Load Balancer)
- [ ] Secrets en Key Vault / Secret Manager (no en código)
- [ ] Feature flags según entorno
- [ ] Rate limiting habilitado en prod