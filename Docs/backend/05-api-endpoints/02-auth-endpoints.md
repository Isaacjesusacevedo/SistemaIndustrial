# Auth Endpoints

## Base Path: `/api/auth`

**Grupo `AllowAnonymous`. Modo demo**: `RequireAuthorization` comentado en logout/password (acceso libre).

---

### POST /api/auth/login

**Autentica usuario y retorna JWT + Refresh Token**

#### Request
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@industrial.com",
  "password": "Industrial2026"
}
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresAt": "2026-10-10T18:00:00Z",
    "user": {
      "id": 1,
      "email": "admin@industrial.com",
      "nombre": "Administrador",
      "role": "Admin"
    }
  },
  "errors": null
}
```

#### Response 401 Unauthorized
```json
{
  "success": false,
  "message": "Credenciales inválidas.",
  "data": null,
  "errors": null
}
```

#### Validaciones
- Email: requerido, formato válido
- Password: requerido, mínimo 8 caracteres
- Usuario debe existir y estar activo (`Activo = 1`)
- Password verificado con BCrypt (workFactor 12)

---

### POST /api/auth/refresh

**Renueva Access Token usando Refresh Token**

#### Request
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Token renovado",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
    "expiresAt": "2026-10-10T18:00:00Z",
    "user": {
      "id": 1,
      "email": "admin@industrial.com",
      "nombre": "Administrador",
      "role": "Admin"
    }
  }
}
```

#### Response 401 Unauthorized
```json
{
  "success": false,
  "message": "Refresh token inválido o expirado.",
  "data": null
}
```

#### Lógica
1. El refresh token es un JWT: se lee el claim `NameIdentifier` (userId)
2. Se verifica `BCrypt(request.RefreshToken, usuario.RefreshTokenHash)`
3. Generar nuevo access token (8h) + nuevo refresh token JWT (7d)
4. Actualizar hash del nuevo refresh token en BD (rotación)

---

### POST /api/auth/logout

**Invalida Refresh Token (logout real)**

#### Request
```http
POST /api/auth/logout
Authorization: Bearer <access_token>
Content-Type: application/json
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Sesión cerrada correctamente.",
  "data": null
}
```

#### Lógica
- Extraer `userId` del JWT (`ClaimTypes.NameIdentifier`)
- Setear `RefreshTokenHash = NULL`, `RefreshTokenExpiry = NULL` en BD

---

### POST /api/auth/register

**Deshabilitado en el código actual**

#### Request
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "operador@industrial.com",
  "password": "Operador2026",
  "nombre": "Juan Pérez",
  "role": "Operador"
}
```

#### Response 400 Bad Request (siempre)
```json
{
  "success": false,
  "message": "Registro no disponible en esta versión",
  "data": null
}
```

---

### PUT /api/auth/password

**Cambio de contraseña (usuario autenticado)**

#### Request
```http
PUT /api/auth/password
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "currentPassword": "Industrial2026",
  "newPassword": "NuevaPass2026"
}
```

#### Response 200 OK
```json
{
  "success": true,
  "message": "Contraseña cambiada correctamente.",
  "data": null
}
```

#### Response 400 Bad Request
```json
{
  "success": false,
  "message": "Contraseña actual incorrecta.",
  "data": null
}
```

#### Validaciones
- Requiere JWT válido
- `currentPassword` verificado con BCrypt
- `newPassword` ≥ 8 chars, distinto a actual
- Actualiza `PasswordHash` + invalida refresh tokens

---

## JWT Token Structure

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload (Claims)
```json
{
  "sub": "1",                    // NameIdentifier (UserId)
  "email": "admin@industrial.com", // Email
  "name": "Administrador",        // Name
  "role": "Admin",               // Role
  "iss": "SistemaIndustrial",    // Issuer
  "aud": "SistemaIndustrialClient", // Audience
  "exp": 1760112000,             // ExpiresAt (Unix timestamp)
  "iat": 1760083200              // IssuedAt (Unix timestamp)
}
```

### Configuración (appsettings)
```json
"Jwt": {
  "Key": "SistemaIndustrial@JWT#2026!ClaveSecretaMuyLarga$ParaFirmarTokens*32chars",
  "Issuer": "SistemaIndustrial",
  "Audience": "SistemaIndustrialClient"
}
```

- **Algoritmo**: HMAC-SHA256
- **Expiración Access Token**: 8 horas
- **Expiración Refresh Token**: 7 días
- **Key**: Mínimo 32 caracteres (256 bits)

---

## Frontend Integration

### Login Flow
```typescript
// composables/useAuth.ts
const login = async (email: string, password: string) => {
  const res = await fetch(`${API}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  
  if (!res.ok) throw new Error('Credenciales inválidas');
  
  const { data } = await res.json();
  
  // Guardar en localStorage
  localStorage.setItem('auth_token', data.token);
  localStorage.setItem('auth_refresh_token', data.refreshToken);
  localStorage.setItem('auth_user', JSON.stringify(data.user));
  
  return data.user;
};
```

### Authenticated Requests
```typescript
const authHeaders = () => {
  const token = localStorage.getItem('auth_token');
  return token
    ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
    : { 'Content-Type': 'application/json' };
};

// Uso
const res = await fetch(`${API}/api/semanas`, { headers: authHeaders() });
```

### Token Refresh (Interceptor)
```typescript
// En useApiFetch o axios interceptor
if (res.status === 401 && !isRefreshing) {
  isRefreshing = true;
  try {
    const refreshToken = localStorage.getItem('auth_refresh_token');
    const refreshRes = await fetch(`${API}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken })
    });
    
    if (refreshRes.ok) {
      const { data } = await refreshRes.json();
      localStorage.setItem('auth_token', data.token);
      localStorage.setItem('auth_refresh_token', data.refreshToken);
      // Reintentar request original
    } else {
      // Logout
      logout();
    }
  } finally {
    isRefreshing = false;
  }
}
```

---

## Seguridad

| Medida | Implementación |
|--------|----------------|
| **Password Hash** | BCrypt workFactor 12 |
| **JWT Signing** | HMAC-SHA256 (HS256) |
| **Token Expiry** | Access: 8h, Refresh: 7d |
| **Refresh Rotation** | Nuevo refresh token en cada uso |
| **Token Storage** | Frontend: localStorage (HttpOnly cookie futura) |
| **CORS** | Origen específico + credentials |
| **Rate Limit Login** | 5 req/min/IP (futuro) |
| **Audit Log** | Login/Logout/Failed attempts (futuro) |