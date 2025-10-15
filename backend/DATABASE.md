# AlphaCredit - Configuración de Base de Datos

## Esquema de Base de Datos

Este proyecto utiliza PostgreSQL como motor de base de datos y Entity Framework Core como ORM.

## Estructura del Proyecto

```
backend/AlphaCredit.Api/
├── Models/               # Entidades del dominio (37 tablas)
├── Data/                 # DbContext y configuraciones
└── Controllers/          # Controladores API
```

## Entidades Implementadas

### Catálogos Básicos (15)
- **Sexo** - Catálogo de géneros
- **EstadoCivil** - Estados civiles
- **TipoIdentificacion** - Tipos de documentos de identidad
- **OperadorTelefono** - Operadoras telefónicas
- **Moneda** - Monedas
- **FrecuenciaPago** - Frecuencias de pago
- **FormaPago** - Formas de pago
- **DestinoCredito** - Destinos del crédito
- **EstadoPrestamo** - Estados de préstamo
- **TipoGarantia** - Tipos de garantía
- **EstadoGarantia** - Estados de garantía
- **ComponentePrestamo** - Componentes del préstamo
- **EstadoComponente** - Estados de componentes
- **TipoCuenta** - Tipos de cuenta bancaria
- **TipoFondo** - Tipos de fondo

### Entidades Geográficas (2)
- **Pais** - Países
- **UbicacionGeografica** - Ubicaciones geográficas (jerárquico)

### Actividades Económicas (2)
- **ActividadCnbs** - Clasificación CNBS
- **ActividadEconomica** - Actividades económicas

### Personas (4)
- **Persona** - Datos de personas (clientes, empleados, proveedores)
- **PersonaTelefono** - Teléfonos de personas
- **PersonaActividad** - Actividades económicas de personas
- **PersonaReferencia** - Referencias personales

### Organizaciones (3)
- **Empresa** - Empresas
- **Sucursal** - Sucursales
- **Banco** - Bancos

### Entidades Bancarias (2)
- **CuentaBancaria** - Cuentas bancarias
- **Transaccion** - Transacciones

### Préstamos (3)
- **Prestamo** - Préstamos
- **PrestamoComponente** - Componentes del préstamo (cuotas, intereses)
- **MovimientoPrestamo** - Movimientos/pagos de préstamos

### Garantías (2)
- **Garantia** - Garantías
- **PrestamoGarantia** - Relación préstamo-garantía

### Fondos (2)
- **Fondo** - Fondos
- **FondoMovimiento** - Movimientos de fondos

### Sistema (3)
- **ParametrosSistema** - Parámetros del sistema
- **FechaSistema** - Fechas del sistema
- **UserCustomizations** - Personalizaciones de usuario

## Configuración

### 1. Cadena de Conexión

Edita [appsettings.json](AlphaCredit.Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "AlphaCreditDb": "Host=localhost;Database=alphacredit;Username=tu_usuario;Password=tu_password"
  }
}
```

### 2. Crear la Base de Datos

#### Opción A: Usar Migraciones de EF Core

```bash
cd backend/AlphaCredit.Api

# Crear migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migración a la base de datos
dotnet ef database update
```

#### Opción B: Usar el esquema existente

Si ya tienes la base de datos creada con el esquema de dbdiagram.io:

1. Asegúrate de que la base de datos PostgreSQL esté corriendo
2. Importa el script SQL generado por dbdiagram.io
3. Actualiza la cadena de conexión en `appsettings.json`

### 3. Verificar la Conexión

El proyecto incluye endpoints para verificar la conexión:

```bash
# Iniciar el backend
cd backend/AlphaCredit.Api
dotnet run

# En otro terminal, probar
curl http://localhost:5000/api/health
```

## Comandos Útiles de Entity Framework

```bash
# Ver todas las migraciones
dotnet ef migrations list

# Crear nueva migración
dotnet ef migrations add NombreDeMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreDeMigracion

# Eliminar última migración (si no se aplicó)
dotnet ef migrations remove

# Generar script SQL de las migraciones
dotnet ef migrations script

# Ver el modelo de datos
dotnet ef dbcontext info

# Scaffold desde base de datos existente (si necesitas)
dotnet ef dbcontext scaffold "Host=localhost;Database=alphacredit;Username=postgres;Password=pass" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -f
```

## Características Implementadas

### Relaciones
- ✅ Relaciones uno a muchos
- ✅ Relaciones muchos a muchos
- ✅ Relaciones auto-referenciadas (UbicacionGeografica)
- ✅ Claves foráneas con restricciones

### Índices
- ✅ Índices únicos (Persona.PersonaIdentificacion, ActividadCnbs.ActividadCnbsCodigo, etc.)
- ✅ Índices compuestos

### Configuración Especial
- ✅ Prevención de ciclos en cascada
- ✅ Claves compuestas (UserCustomizations)
- ✅ Secuencias de PostgreSQL
- ✅ Tipos de datos específicos de PostgreSQL

## Próximos Pasos

1. **Crear Repositorios**: Implementar el patrón Repository para acceso a datos
2. **Agregar DTOs**: Crear Data Transfer Objects para las APIs
3. **Implementar Servicios**: Lógica de negocio
4. **Agregar Validaciones**: FluentValidation para reglas de negocio
5. **Implementar Autenticación**: JWT para seguridad
6. **Agregar Logging**: Serilog para logs estructurados
7. **Unit Tests**: xUnit para pruebas unitarias

## Notas Importantes

- Las tablas usan nomenclatura en **minúsculas** siguiendo convenciones de PostgreSQL
- Los campos de auditoría incluyen: `usercrea`, `usermodifica`, `fechacrea`, `fechamodifica`
- Algunos campos tienen valores por defecto desde secuencias de PostgreSQL
- Se recomienda usar transacciones para operaciones complejas
- Los campos GUID se usan para sincronización/replicación

## Troubleshooting

### Error de conexión
```
Verifica que PostgreSQL esté corriendo:
sudo systemctl status postgresql  # Linux
Get-Service postgresql*           # Windows
```

### Error de migración
```
Si hay conflictos, elimina la carpeta Migrations y la tabla __EFMigrationsHistory:
rm -rf Migrations/
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Error de permisos
```
Asegúrate de que el usuario de la base de datos tenga permisos:
GRANT ALL PRIVILEGES ON DATABASE alphacredit TO tu_usuario;
```
