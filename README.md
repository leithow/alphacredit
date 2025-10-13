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
- .NET SDK 8.0 o superior

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

## Próximos Pasos

- Agregar autenticación y autorización
- Implementar base de datos
- Agregar más endpoints y funcionalidades
- Configurar despliegue en producción
