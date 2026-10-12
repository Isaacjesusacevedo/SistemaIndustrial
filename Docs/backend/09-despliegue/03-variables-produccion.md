# Variables de Producción

## Secrets Management

### Principios
- **Nunca** commitear secrets al repositorio
- **Rotar** claves cada 90 días
- **Diferentes** valores por entorno (Dev/Staging/Prod)
- **Acceso mínimo** (principio de menor privilegio)

---

## Variables Críticas

### Base de Datos
```bash
# Connection String completa (recomendada)
ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=***REMOVED***;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# O componentes separados (para Key Vault)
DB_SERVER="tcp:prod-sql.database.windows.net,1433"
DB_NAME="SistemaIndustrialDB"
DB_USER="industrial_prod"
DB_PASSWORD="***"  # En Key Vault / Secrets Manager
```

### JWT / Autenticación
```bash
# Clave firma HS256 - MÍNIMO 32 chars (256 bits)
# Generar: openssl rand -base64 64
ASPNETCORE_JWT__KEY="xK9mP2qR5vW8zA3bC6dE9fG2hJ5kL8nQ1rT4uY7wX0zB3cV6mN9pQ2sT5vW8y"

# Issuer / Audience (no rotar frecuentemente)
ASPNETCORE_JWT__ISSUER="SistemaIndustrial"
ASPNETCORE_JWT__AUDIENCE="SistemaIndustrialClient"

# Expiraciones (opcional, defaults en código)
ASPNETCORE_JWT__ACCESSTOKENEXPIRYHOURS="8"
ASPNETCORE_JWT__REFRESHTOKENEXPIRYDAYS="7"
```

### CORS
```bash
# Orígenes exactos, HTTPS obligatorio en prod
ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://industrial.empresa.com"
ASPNETCORE_CORS__ALLOWEDORIGINS__1="https://admin.industrial.empresa.com"
```

### Logging / Observabilidad
```bash
# Seq (structured logging)
SEQ_API_KEY="***"
SEQ_SERVER_URL="https://seq.empresa.com"

# Azure Application Insights
APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=***;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/"

# Elastic/APM
ELASTIC_APM_SERVER_URL="https://apm.empresa.com"
ELASTIC_APM_SECRET_TOKEN="***"
```

### Feature Flags
```bash
ASPNETCORE_FEATURES__ENABLESWAGGER="false"
ASPNETCORE_FEATURES__ENABLERATELIMITING="true"
ASPNETCORE_FEATURES__ENABLEAUDITLOG="true"
ASPNETCORE_FEATURES__MAINTENANCEMODE="false"
```

---

## Azure Key Vault Integration

### Configuración en Program.cs
```csharp
var keyVaultUrl = builder.Configuration["KeyVault:Url"]; // https://kv-sistema-industrial.vault.azure.net/

if (!string.IsNullOrEmpty(keyVaultUrl) && !builder.Environment.IsDevelopment())
{
    var credential = new DefaultAzureCredential(); // Managed Identity / CLI / Env
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
}
```

### Secrets en Key Vault
| Secret Name | Valor | Rotación |
|-------------|-------|----------|
| `DbConnectionString` | Connection string completa | 90 días |
| `JwtKey` | Clave firma 64 chars | 90 días |
| `SeqApiKey` | API Key Seq | 180 días |
| `SmtpPassword` | Password SMTP (notificaciones) | 90 días |

### Acceso desde Código
```csharp
// Automático via Configuration providers
var connString = builder.Configuration["ConnectionStrings:SqlServer"];
var jwtKey = builder.Configuration["Jwt:Key"]; // Resuelve desde Key Vault si configurado
```

---

## AWS Secrets Manager

### Configuración
```csharp
// En Program.cs (requiere AWSSDK.SecretsManager)
if (builder.Environment.IsProduction())
{
    var client = new AmazonSecretsManagerClient(RegionEndpoint.USEast1);
    var request = new GetSecretValueRequest { SecretId = "prod/sistema-industrial" };
    var response = await client.GetSecretValueAsync(request);
    
    var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
    foreach (var kvp in secrets)
    {
        builder.Configuration[kvp.Key] = kvp.Value;
    }
}
```

### Secret JSON en AWS
```json
{
  "ConnectionStrings:SqlServer": "Server=...;Password=***...",
  "Jwt:Key": "***",
  "Cors:AllowedOrigins:0": "https://industrial.empresa.com",
  "SeqApiKey": "***"
}
```

---

## Docker Secrets

### docker-compose.prod.yml
```yaml
services:
  api:
    image: ghcr.io/empresa/sistema-industrial-api:latest
    secrets:
      - db_connection_string
      - jwt_key
      - cors_origins
      - seq_api_key
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080

secrets:
  db_connection_string:
    external: true  # docker secret create db_connection_string < file.txt
  jwt_key:
    external: true
  cors_origins:
    external: true
  seq_api_key:
    external: true
```

### Crear Secrets
```bash
echo "Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=***...;Encrypt=True;TrustServerCertificate=False;" | docker secret create db_connection_string -
echo "xK9mP2qR5vW8zA3bC6dE9fG2hJ5kL8nQ1rT4uY7wX0zB3cV6mN9pQ2sT5vW8y" | docker secret create jwt_key -
echo '["https://industrial.empresa.com","https://admin.industrial.empresa.com"]' | docker secret create cors_origins -
```

### Acceso en Container
```csharp
// En Program.cs - leer de /run/secrets/
var secretsPath = "/run/secrets/";
if (Directory.Exists(secretsPath))
{
    foreach (var file in Directory.GetFiles(secretsPath))
    {
        var key = Path.GetFileName(file).Replace("_", ":").Replace("-", ":");
        var value = File.ReadAllText(file).Trim();
        builder.Configuration[key] = value;
    }
}
```

---

## Kubernetes Secrets

### Crear Secret
```bash
# Desde archivos
kubectl create secret generic sistema-industrial-secrets \
  --from-literal=ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER="Server=tcp:prod-sql.database.windows.net,1433;Database=SistemaIndustrialDB;User Id=industrial_prod;Password=***...;Encrypt=True;TrustServerCertificate=False;" \
  --from-literal=ASPNETCORE_JWT__KEY="xK9mP2qR5vW8zA3bC6dE9fG2hJ5kL8nQ1rT4uY7wX0zB3cV6mN9pQ2sT5vW8y" \
  --from-literal=ASPNETCORE_CORS__ALLOWEDORIGINS__0="https://industrial.empresa.com" \
  --from-literal=SEQ_API_KEY="***" \
  -n industrial

# O desde archivo
kubectl create secret generic sistema-industrial-secrets --from-file=./secrets/ -n industrial
```

### External Secrets Operator (Recomendado Prod)
```yaml
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: sistema-industrial-secrets
  namespace: industrial
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: azure-keyvault
    kind: ClusterSecretStore
  target:
    name: sistema-industrial-secrets
    creationPolicy: Owner
  data:
  - secretKey: ASPNETCORE_CONNECTIONSTRINGS__SQLSERVER
    remoteRef:
      key: DbConnectionString
  - secretKey: ASPNETCORE_JWT__KEY
    remoteRef:
      key: JwtKey
  - secretKey: SEQ_API_KEY
    remoteRef:
      key: SeqApiKey
```

---

## Rotación de Secrets

### JWT Key Rotation (Sin Downtime)
```bash
# 1. Generar nueva clave
NEW_JWT_KEY=$(openssl rand -base64 64)

# 2. Actualizar en Key Vault / Secrets Manager
az keyvault secret set --vault-name kv-sistema-industrial --name JwtKey --value "$NEW_JWT_KEY"

# 3. Rolling restart de pods (K8s) / servicios (systemd/Docker)
#    Los tokens existentes siguen válidos hasta expirar (8h)
#    Los nuevos tokens usan la nueva clave automáticamente

# 4. Verificar
curl -f https://industrial.empresa.com/health
```

### DB Password Rotation
```bash
# 1. Cambiar password en SQL Server / Azure SQL
az sql server update -n industrial-sql-prod -g rg-prod --admin-password "$NEW_DB_PASS"

# 2. Actualizar secret
az keyvault secret set --vault-name kv-sistema-industrial --name DbPassword --value "$NEW_DB_PASS"

# 3. Restart API (necesario para nueva conexión)
kubectl rollout restart deployment/sistema-industrial-api -n industrial
# O systemd:
sudo systemctl restart sistema-industrial-api
```

---

## Auditoría de Secrets

### Script de Verificación
```bash
#!/bin/bash
# check-secrets.sh - Ejecutar en CI/CD

echo "=== Verificando Secrets ==="

# Verificar que no hay secrets en código
if git log --all --full-history --oneline --grep="password\|secret\|key" -- "*.json" "*.cs" | grep -v "test\|\.md"; then
    echo "⚠️  Posibles secrets en historial git"
fi

# Verificar longitud JWT Key
JWT_KEY="${ASPNETCORE_JWT__KEY:-}"
if [ ${#JWT_KEY} -lt 32 ]; then
    echo "❌ JWT Key muy corta (< 32 chars): ${#JWT_KEY}"
    exit 1
fi

# Verificar que no es clave por defecto
if [[ "$JWT_KEY" == *"Desarrollo"* ]] || [[ "$JWT_KEY" == *"Test"* ]] || [[ "$JWT_KEY" == *"Default"* ]]; then
    echo "❌ JWT Key parece ser de desarrollo/test"
    exit 1
fi

# Verificar CORS HTTPS en prod
if [ "$ASPNETCORE_ENVIRONMENT" = "Production" ]; then
    for origin in $(echo "$ASPNETCORE_CORS__ALLOWEDORIGINS__0" | tr ',' ' '); do
        if [[ ! "$origin" =~ ^https:// ]]; then
            echo "❌ CORS origin no HTTPS en producción: $origin"
            exit 1
        fi
    done
fi

echo "✅ Secrets verificados correctamente"
```

---

## Checklist de Seguridad

- [ ] JWT Key ≥ 32 chars, aleatoria, única por entorno
- [ ] DB Password rotada, en Key Vault/Secrets Manager
- [ ] CORS origins exactos, HTTPS only en prod
- [ ] No secrets en repositorio (git history limpio)
- [ ] Secrets rotados cada 90 días (JWT, DB)
- [ ] Acceso a secrets auditado (Key Vault access policies)
- [ ] Feature flags: Swagger OFF, RateLimiting ON en prod
- [ ] TLS 1.2+ en todas las comunicaciones
- [ ] Health checks no exponen información sensible