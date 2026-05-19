# Realistic A320 Fenix Failures

Aplicación de escritorio WPF (.NET 10) para simular fallas realistas del A320 Fenix, con arquitectura limpia, persistencia en SQLite y dispatch de fallas vía API HTTP local de Fenix.

## Objetivo

El sistema permite:

- Seleccionar presets de fallas por fase de vuelo
- Ejecutar un motor probabilístico de fallas
- Disparar fallas reales del Fenix A320 por HTTP
- Guardar sesiones y fallas disparadas en SQLite
- Monitorear estado de conexión y eventos desde UI

## Stack técnico

- .NET 10
- WPF + MVVM
- Clean Architecture
- EF Core + SQLite
- Serilog
- `IHost` + DI de `Microsoft.Extensions.Hosting`
- Integración Fenix por `HttpClient`
- Integración SimConnect como stub (extensible)

## Estructura de la solución

```text
RealFenixFailures.slnx
├─ RealFenixFailures.Domain
├─ RealFenixFailures.Application
├─ RealFenixFailures.Infrastructure
├─ RealFenixFailures.Integrations.Fenix
├─ RealFenixFailures.Integrations.SimConnect
└─ RealFenixFailures.UI
```

### Capas

- **Domain**
  - Entidades, enums, contratos de dominio, reglas/motor puro
  - Sin dependencias de infraestructura

- **Application**
  - Casos de uso y orquestación
  - DTOs para UI
  - Contratos para integrar infraestructura e integraciones

- **Infrastructure**
  - EF Core (`DbContext`, repositorios, seed, migrations)
  - Logging (Serilog)
  - Settings persistentes del engine

- **Integrations.Fenix**
  - Cliente HTTP y modelos del payload Fenix
  - Health check cacheado
  - Trigger/reset de fallas manuales

- **Integrations.SimConnect**
  - Cliente stub para estado de vuelo
  - Punto de extensión para SimConnect real

- **UI**
  - WPF + MVVM (dashboard, estado, controles)
  - Boot con Generic Host y DI

## Flujo funcional

1. La UI inicia `IHost`, crea scope y aplica migraciones automáticamente.
2. El usuario selecciona preset y activa engine.
3. El orquestador consulta fase de vuelo y conectividad.
4. El motor de dominio evalúa probabilidad y fase.
5. Si corresponde, se dispara falla en Fenix (`saveManual`) y se persiste en DB.
6. La UI actualiza estado y log en tiempo real.
7. Al desactivar engine, se hace reset de fallas activas en Fenix.

## Configuración

Archivo: `RealFenixFailures.UI/appsettings.json`

### Secciones clave

- `ConnectionStrings:Sqlite`
  - Ruta del archivo SQLite (por defecto `realfenixfailures.db`)

- `FailureEngine`
  - `GlobalProbability`: probabilidad global de disparo por ciclo
  - `CheckIntervalSeconds`: intervalo de polling
  - `ForcedFlightPhaseForStub`: fase usada por stub

- `FenixApi`
  - `BaseUrl`: host del endpoint Fenix
  - `Port`: por defecto 8083
  - `ManualFailuresPath`: `GET` para listar fallas manuales
  - `SaveManualPath`: `POST` para aplicar cambios
  - `HealthCheckIntervalSeconds`: TTL del health check cacheado

- `Serilog`
  - Nivel de logs
  - Output a archivo `logs/realfenix-.log`

## Endpoints Fenix usados

- `GET /fenix/failures/manual`
- `POST /fenix/failures/saveManual`

La app arma URL final desde `BaseUrl + Port + Path`.

## Archivo de referencia de fallas

Fuente usada para modelar la respuesta/API de Fenix:

- `C:/Users/mathi/OneDrive/Documentos/Stash/repos/FNX Realistic Failures System/RF/failures.json`

## Cómo ejecutar el proyecto

Desde la raíz de la solución:

```powershell
dotnet restore .\RealFenixFailures.slnx
dotnet build .\RealFenixFailures.slnx -c Debug
dotnet run --project .\RealFenixFailures.UI\RealFenixFailures.UI.csproj
```

## Comandos útiles de validación

```powershell
dotnet build .\RealFenixFailures.slnx -c Debug
dotnet build .\RealFenixFailures.slnx -c Release
```

## Base de datos y EF Core Migrations

### Qué hay implementado

- `RealFenixDbContext` en `Infrastructure`
- Factory de diseño: `RealFenixDbContextFactory`
- Migraciones en `RealFenixFailures.Infrastructure/Migrations`
- Migración aplicada para `ExternalFailureId`

### Regla práctica

Cada vez que cambies una entidad mapeada a tabla:

1. Modificá entidad en Domain (o su relación)
2. Ajustá configuración en `RealFenixDbContext` si corresponde
3. Si afecta seed, actualizá `SeedData`
4. Generá nueva migration
5. Revisá el código de migration antes de aplicar
6. Aplicá migration a la DB
7. Compilá Debug/Release

### Comandos de migrations

#### Crear migration

```powershell
dotnet ef migrations add NombreDeLaMigration --project .\RealFenixFailures.Infrastructure\RealFenixFailures.Infrastructure.csproj --startup-project .\RealFenixFailures.UI\RealFenixFailures.UI.csproj --context RealFenixDbContext
```

#### Aplicar migrations

```powershell
dotnet ef database update --project .\RealFenixFailures.Infrastructure\RealFenixFailures.Infrastructure.csproj --startup-project .\RealFenixFailures.UI\RealFenixFailures.UI.csproj --context RealFenixDbContext
```

#### Ver lista de migrations

```powershell
dotnet ef migrations list --project .\RealFenixFailures.Infrastructure\RealFenixFailures.Infrastructure.csproj --startup-project .\RealFenixFailures.UI\RealFenixFailures.UI.csproj --context RealFenixDbContext
```

#### Si te equivocaste en una migration recién creada

```powershell
dotnet ef migrations remove --project .\RealFenixFailures.Infrastructure\RealFenixFailures.Infrastructure.csproj --startup-project .\RealFenixFailures.UI\RealFenixFailures.UI.csproj --context RealFenixDbContext
```

### Buenas prácticas al cambiar entidades/tablas

- No editar migrations viejas ya aplicadas en entornos compartidos
- Crear siempre una migration nueva para cada cambio de esquema
- Evitar cambios destructivos sin respaldo
- Si renombrás propiedad/tabla, validar que EF no interprete como drop/create involuntario
- Revisar `Up` y `Down` antes de ejecutar `database update`
- Si cambian datos seed, EF puede generar `UpdateData`; revisar que sea correcto

## Seeds y catálogo de fallas

- Los presets y fallas iniciales se cargan desde `SeedData`
- `ExternalFailureId` mapea cada falla local al `id` real de Fenix
- Si agregás una nueva falla:
  1. Crear `FailureDefinition` en seed
  2. Asignar `ExternalFailureId` válido de `failures.json`
  3. Asociar la falla a uno o más presets
  4. Generar/aplicar migration

## Integraciones y extensibilidad

### SimConnect

Actualmente es stub. Para implementación real:

- Completar `ISimConnectClient` / `SimConnectClient`
- Mapear telemetría real a `FlightPhase`
- Mantener contrato `IFlightDataProvider`

### Fenix API

La integración ya está funcional para:

- Health check
- Consulta de fallas manuales
- Activación/desactivación por `saveManual`
- Reset de fallas al detener engine

## Logs y troubleshooting

### Logs

- Archivo diario: `logs/realfenix-.log`

### Problemas comunes

- **No conecta a Fenix**
  - Verificar `BaseUrl`, `Port`, firewall y que Fenix exponga la API

- **Migration no detecta cambios**
  - Confirmar que cambiaste modelo/fluido en `DbContext`
  - Limpiar/build y volver a generar migration

- **Errores de diseño en EF Tools**
  - Verificar `RealFenixDbContextFactory`
  - Verificar `--project` y `--startup-project`

## Flujo recomendado para seguir desarrollando

1. Definir cambio funcional
2. Implementar en Domain/Application
3. Implementar persistencia/integración
4. Ajustar UI
5. Generar y aplicar migration (si hay cambios de schema)
6. Build Debug + Release
7. Probar flujo completo con Fenix corriendo

## Estado actual

- Solución compila en Debug y Release
- Integración HTTP Fenix operativa
- Migrations funcionando y aplicadas
- Persistencia SQLite y seeds activos

---

Si querés, en el próximo paso te puedo dejar una checklist de PR (tipo plantilla) para que cada cambio futuro tenga validación técnica consistente.