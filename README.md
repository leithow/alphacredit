# AlphaCredit

Proyecto full-stack con backend en .NET Core y frontend en React.

## Estructura del Proyecto

```
alphacredit/
├── backend/
│   └── AlphaCredit.Api/          # API .NET Core
├── frontend/
│   └── alphacredit-app/          # Aplicación React
└── README.md
```

## Backend (.NET Core)

### Requisitos
- .NET SDK 9.0 o superior
- PostgreSQL 12 o superior

### Base de Datos

El proyecto usa PostgreSQL con Entity Framework Core. Ver [backend/DATABASE.md](backend/DATABASE.md) para documentación completa.

**Configuración rápida:**

1. Asegúrate de tener PostgreSQL instalado y corriendo
2. Crea la base de datos:
   ```bash
   psql -U postgres
   CREATE DATABASE alphacredit;
   \q
   ```
3. Actualiza la cadena de conexión en `backend/AlphaCredit.Api/appsettings.json`
4. La migración ya está aplicada, pero si necesitas recrear la base de datos:
   ```bash
   cd backend/AlphaCredit.Api
   dotnet ef database update
   ```

**Esquema de base de datos:**
- 36 tablas para gestión de microfinanzas/préstamos
- Entidades: Personas, Préstamos, Garantías, Fondos, Catálogos, etc.
- Ver [backend/DATABASE.md](backend/DATABASE.md) para detalles completos

### Ejecutar el Backend

```bash
cd backend/AlphaCredit.Api
dotnet run
```

El backend se ejecutará en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Endpoints Disponibles

- `GET /api/health` - Verifica el estado del servicio
- `GET /weatherforecast` - Endpoint de ejemplo

## Frontend (React)

### Requisitos
- Node.js 14.0 o superior
- npm o yarn

### Ejecutar el Frontend

```bash
cd frontend/alphacredit-app
npm install  # Solo la primera vez
npm start
```

El frontend se ejecutará en `http://localhost:3000`

## Configuración

### CORS
El backend está configurado para aceptar peticiones desde `http://localhost:3000`. Si necesitas cambiar esto, edita [backend/AlphaCredit.Api/Program.cs](backend/AlphaCredit.Api/Program.cs#L13).

### Variables de Entorno
El frontend usa variables de entorno definidas en [frontend/alphacredit-app/.env](frontend/alphacredit-app/.env):
- `REACT_APP_API_URL` - URL del backend API

## Desarrollo

### Backend
- Los controladores están en [backend/AlphaCredit.Api/Controllers/](backend/AlphaCredit.Api/Controllers/)
- Configuración principal en [backend/AlphaCredit.Api/Program.cs](backend/AlphaCredit.Api/Program.cs)

### Frontend
- Componente principal en [frontend/alphacredit-app/src/App.js](frontend/alphacredit-app/src/App.js)
- La aplicación incluye un ejemplo de integración con el backend

## Probar la Integración

1. Inicia el backend:
   ```bash
   cd backend/AlphaCredit.Api
   dotnet run
   ```

2. En otra terminal, inicia el frontend:
   ```bash
   cd frontend/alphacredit-app
   npm start
   ```

3. Abre `http://localhost:3000` en tu navegador
4. Haz clic en "Check Backend Health" para verificar la conexión

## Estado del Proyecto

### Completado ✅
- ✅ Estructura del proyecto backend (.NET Core 9.0)
- ✅ Estructura del proyecto frontend (React)
- ✅ Configuración de CORS
- ✅ Base de datos PostgreSQL con 36 tablas
- ✅ Entity Framework Core configurado
- ✅ Migraciones de base de datos
- ✅ Modelos de dominio completos
- ✅ DbContext con todas las entidades

### Próximos Pasos

- Implementar Repositorios y Servicios
- Crear controladores API para las entidades principales
- Agregar DTOs (Data Transfer Objects)
- Implementar validaciones con FluentValidation
- Agregar autenticación y autorización (JWT)
- Crear interfaces de usuario en React
- Implementar logging con Serilog
- Agregar pruebas unitarias
- Configurar despliegue en producción

## Tecnologías Utilizadas

### Backend
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- Npgsql (PostgreSQL driver)
- PostgreSQL 12+

### Frontend
- React 18
- JavaScript/ES6+
- CSS3

## Documentación Adicional

- [Documentación de Base de Datos](backend/DATABASE.md) - Esquema completo, entidades y comandos útiles
