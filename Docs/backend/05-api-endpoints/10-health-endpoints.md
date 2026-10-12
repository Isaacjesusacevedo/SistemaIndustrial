# Health Endpoints

## Base Path: `/health`

**Públicos (sin autenticación), mapeados inline en `Program.cs`.**

---

### GET /health

```json
{ "status": "Healthy", "timestamp": "2026-10-10T10:00:00Z" }
```

---

### GET /health/ready

```json
{ "status": "Ready", "database": "Connected" }
```

---

### GET /health/live

```json
{ "status": "Alive" }
```
