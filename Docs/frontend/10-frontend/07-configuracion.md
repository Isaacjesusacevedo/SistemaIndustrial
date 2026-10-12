# Configuración y Build

## Variables de Entorno

> No hay archivos `.env` en el repo (solo `env.d.ts` con la referencia a `vite/client`). La URL del backend se resuelve en el cliente HTTP (`composables/useApiFetch.ts`). Si se agregan variables `VITE_*`, documentarlas aquí.

---

## Vite Configuration (`vite.config.ts`, real)

```typescript
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
```

> Configuración mínima: solo plugin Vue y alias `@`. No hay proxy `/api`, ni `manualChunks`, ni aliases `@components/@stores/...`, ni `loadEnv`, ni `define` de constantes. Todo lo avanzado (chunks, gzip, headers) queda como mejora futura.

---

## TypeScript Configuration

### `tsconfig.app.json` (real, extracto)
```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

> Único alias real: `@/*`. No existen aliases `@components`, `@stores`, etc. Ver `tsconfig.app.json` / `tsconfig.json` / `tsconfig.node.json` para el resto.

### `tsconfig.node.json`
```json
{
  "compilerOptions": {
    "composite": true,
    "skipLibCheck": true,
    "module": "ESNext",
    "moduleResolution": "bundler",
    "allowSyntheticDefaultImports": true,
    "strict": true
  },
  "include": ["vite.config.ts", "vite-env.d.ts"]
}
```

### `vite-env.d.ts`
```typescript
/// <reference types="vite/client" />
/// <reference types="vue/macros-global" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_APP_TITLE: string
  readonly VITE_APP_VERSION: string
  readonly VITE_ENABLE_MOCK_API: boolean
  readonly VITE_ENABLE_DEVTOOLS: boolean
  readonly VITE_TOKEN_STORAGE: 'localStorage' | 'sessionStorage' | 'cookie'
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

// Tipos globales para Vue
declare module 'vue' {
  interface ComponentCustomProperties {
    $toast: ReturnType<typeof import('@/composables/useToast').useToast>
  }
}
```

---

## ESLint / Prettier / Husky (pendiente)

No hay `.eslintrc.cjs`, `.prettierrc`, hooks Husky ni `commitlint` en el repo (aunque `eslint` y `prettier` están en `devDependencies`). La configuración de lint/format/hooks queda como trabajo futuro. Ejemplo de configuración propuesta (no existe en el repo):

```javascript
module.exports = {
  root: true,
  env: {
    browser: true,
    es2022: true,
    node: true
  },
  extends: [
    'eslint:recommended',
    'plugin:vue/vue3-essential',
    'plugin:@typescript-eslint/recommended',
    'plugin:vue-scoped-css/recommended',
    'prettier'
  ],
  parser: 'vue-eslint-parser',
  parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module',
    parser: '@typescript-eslint/parser',
    extraFileExtensions: ['.vue']
  },
  plugins: ['vue', '@typescript-eslint', 'vue-scoped-css', 'prettier'],
  rules: {
    // Vue
    'vue/multi-word-component-names': 'off', // Permitir componentes de una palabra
    'vue/no-unused-vars': 'error',
    'vue/require-default-prop': 'off',
    'vue/require-prop-types': 'warn',
    'vue/valid-define-props': 'error',
    'vue/valid-define-emits': 'error',
    
    // TypeScript
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    '@typescript-eslint/consistent-type-imports': 'error',
    '@typescript-eslint/no-non-null-assertion': 'warn',
    
    // General
    'no-console': ['warn', { allow: ['warn', 'error'] }],
    'no-debugger': 'warn',
    'prefer-const': 'error',
    'no-var': 'error',
    
    // Prettier
    'prettier/prettier': 'error'
  },
  overrides: [
    {
      files: ['**/*.vue'],
      rules: {
        'vue/block-order': ['error', { order: ['script', 'template', 'style'] }]
      }
    }
  ]
}
```

---

## Prettier Configuration (propuesta, no existe `.prettierrc` en el repo)

```json
{
  "semi": true,
  "singleQuote": true,
  "tabWidth": 2,
  "trailingComma": "es5",
  "printWidth": 100,
  "bracketSpacing": true,
  "arrowParens": "avoid",
  "endOfLine": "lf",
  "vueIndentScriptAndStyle": true,
  "htmlWhitespaceSensitivity": "css",
  "insertPragma": false,
  "proseWrap": "preserve",
  "quoteProps": "as-needed"
}
```

---

## Package.json Scripts (reales)

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc -b && vite build",
    "preview": "vite preview"
  }
}
```

> No hay scripts de lint, format, tests ni typecheck separado en el código real. Los comandos `lint`, `test:unit`, `test:e2e` documentados antes no existen: son trabajo futuro.

---

## Husky Git Hooks (propuesta, no hay `.husky/` en el repo)

### `.husky/pre-commit`
```bash
#!/usr/bin/env sh
. "$(dirname -- "$0")/_/husky.sh"

echo "🔍 Running pre-commit checks..."

# TypeScript check
npm run typecheck || exit 1

# ESLint
npm run lint:check || exit 1

# Prettier
npm run format:check || exit 1

echo "✅ Pre-commit checks passed"
```

### `.husky/commit-msg`
```bash
#!/usr/bin/env sh
. "$(dirname -- "$0")/_/husky.sh"

# Validar mensaje de commit (Conventional Commits)
npx --no-install commitlint --edit "$1"
```

### `commitlint.config.js`
```javascript
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      ['feat', 'fix', 'docs', 'style', 'refactor', 'perf', 'test', 'chore', 'build', 'ci', 'revert']
    ],
    'subject-case': [2, 'always', ['sentence-case', 'lower-case']],
    'subject-max-length': [2, 'always', 100]
  }
}
```

---

## Docker para Frontend (propuesta, no hay `Dockerfile`/`nginx.conf` en el repo)

### `Dockerfile`
```dockerfile
# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copiar package files
COPY package*.json ./
RUN npm ci

# Copiar source
COPY . .

# Build arguments para env
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# Production stage
FROM nginx:alpine AS production

# Copiar build
COPY --from=build /app/dist /usr/share/nginx/html

# Configuración nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Puerto
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
```

### `nginx.conf`
```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;
    
    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # Cache static assets
    location /assets/ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }
    
    # Gzip
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml application/json;
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN";
    add_header X-Content-Type-Options "nosniff";
    add_header X-XSS-Protection "1; mode=block";
    add_header Referrer-Policy "strict-origin-when-cross-origin";
    
    # Proxy API (si no hay reverse proxy externo)
    location /api/ {
        proxy_pass http://backend:5127;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### `docker-compose.yml`
```yaml
version: '3.8'

services:
  frontend:
    build:
      context: ./Frontend
      dockerfile: Dockerfile
      args:
        VITE_API_URL: http://backend:5127
    ports:
      - "5173:80"
    depends_on:
      - backend
    networks:
      - app-network

  backend:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5127:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    volumes:
      - ./SistemaIndustrial.API/appsettings.Development.json:/app/appsettings.Development.json:ro
    networks:
      - app-network

networks:
  app-network:
    driver: bridge
```

---

## CI/CD Pipeline (propuesta, no hay workflows en el repo)

### `.github/workflows/frontend.yml`
```yaml
name: Frontend CI/CD

on:
  push:
    branches: [main, develop]
    paths:
      - 'Frontend/**'
  pull_request:
    branches: [main]
    paths:
      - 'Frontend/**'

jobs:
  lint-and-test:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./Frontend
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: Frontend/package-lock.json
      
      - name: Install dependencies
        run: npm ci
      
      - name: TypeScript check
        run: npm run typecheck
      
      - name: ESLint
        run: npm run lint:check
      
      - name: Prettier
        run: npm run format:check
      
      - name: Unit tests
        run: npm run test:unit
      
      - name: Build
        run: npm run build
        env:
          VITE_API_URL: ${{ secrets.VITE_API_URL }}
      
      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: frontend-dist
          path: Frontend/dist
          retention-days: 7

  deploy-staging:
    needs: lint-and-test
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    environment: staging
    
    steps:
      - name: Deploy to Staging
        run: |
          # SSH deploy o kubectl apply
          echo "Deploying to staging..."

  deploy-production:
    needs: lint-and-test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    
    steps:
      - name: Deploy to Production
        run: |
          # kubectl apply / docker push / etc.
          echo "Deploying to production..."
```

---

## Performance Budget (propuesta, sin Lighthouse CI en el repo)

### Lighthouse CI (`.lighthouserc.json`)
```json
{
  "ci": {
    "collect": {
      "numberOfRuns": 3,
      "startServerCommand": "npm run preview:dist",
      "url": "http://localhost:4173"
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", { "minScore": 0.9 }],
        "categories:accessibility": ["error", { "minScore": 0.95 }],
        "categories:best-practices": ["error", { "minScore": 0.9 }],
        "categories:seo": ["error", { "minScore": 0.9 }],
        "first-contentful-paint": ["error", { "maxNumericValue": 2000 }],
        "largest-contentful-paint": ["error", { "maxNumericValue": 2500 }],
        "cumulative-layout-shift": ["error", { "maxNumericValue": 0.1 }],
        "total-blocking-time": ["error", { "maxNumericValue": 300 }],
        "interactive": ["error", { "maxNumericValue": 3000 }]
      }
    }
  }
}
```

---

## Bundle Analysis (propuesta)

```bash
# Analizar tamaño de bundles
npm run build
npx vite-bundle-analyzer dist

# O con rollup-plugin-visualizer
npm install -D rollup-plugin-visualizer
# Agregar a vite.config.ts plugins
```

---

## Checklist Pre-Deploy

- [ ] `npm run build` (`vue-tsc -b && vite build`) pasa sin errores
- [ ] Bundle size < 500kb (gzipped) para initial load
- [ ] Variables de entorno de producción configuradas (hoy no hay `.env` en el repo)
- [ ] Health check endpoint responde
- [ ] Cache headers configurados en nginx/CDN
- [ ] HTTPS habilitado
- [ ] Security headers presentes