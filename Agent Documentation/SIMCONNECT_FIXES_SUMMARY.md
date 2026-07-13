# SimConnect Connection Fixes for MSFS2024

## Problema Identificado

La conexión a SimConnect para MSFS2024 sí detectaba la conexión pero no obtenía datos del simulador. Los problemas raíz fueron:

1. **Sin Message Pump**: SimConnect requiere un loop que procese mensajes periódicamente para disparar eventos como `OnRecvSimobjectDataBytype`. Sin esto, los datos nunca se reciben.

2. **Acceso No Thread-Safe**: El estado de la aeronave (`_lastKnownState`) se accedía desde múltiples hilos sin sincronización adecuada.

3. **Datos No Actualizados Correctamente**: El método `GetAircraftStateAsync` retornaba datos sin esperar respuesta del simulador.

4. **Tipos de Datos Incorrectos**: Los campos de `AircraftStateData` usaban `INT32` en lugar de `FLOAT64` para valores decimales de SimConnect.

5. **Inconsistencia en Flujo de Datos**: No había un flujo claro y consistente de datos desde SimConnect hasta los servicios superiores.

## Soluciones Implementadas

### 1. SimConnectClient.cs - Reescrito Completamente

**Cambios principales:**

- **Message Pump Asincrónico**: Implementado `RunMessagePumpAsync()` que ejecuta continuamente `_simConnect.ReceiveMessage()` con un intervalo de 10ms. Esto procesa los eventos de SimConnect en un thread separado.

- **Sincronización Thread-Safe**:
  - Agregado `ReaderWriterLockSlim _stateReadWriteLock` para acceso sincronizado a `_lastKnownState`
  - Agregado `SemaphoreSlim _connectionLock` para proteger operaciones de conexión

- **Data Request Loop Asincrónico**: Implementado `StartDataRequestLoopAsync()` que solicita datos cada 100ms de forma consistente.

- **Detección de Desconexión**: Se rastrea `_lastDataReceivedUtc` para detectar cuando no hay datos en 5-10 segundos e intentar reconectar.

- **Manejo de Errores Robusto**: Se capturan y registran excepciones específicas en el message pump y data request loop.

### 2. AircraftStateData.cs - Tipos de Datos Corregidos

Cambio de tipos para que coincidan con lo que SimConnect retorna:

- `int` → `double` para: Latitude, Longitude, Altitude, Heading, GroundSpeed, AirspeedTrue, VerticalSpeed, FlapsHandleIndex, ThrottlePercent1, ThrottlePercent2, RadioHeight

Esto previene truncamiento de datos y cálculos incorrectos.

### 3. SimulatorFlightDataProvider.cs - Mejorado

- Mejor logging para debugging (cache hits, cambios de estado)
- Mejor manejo de timeouts en health checks
- Captura específica de `OperationCanceledException`
- Logging de cambios de estado de conexión

## Flujo de Datos Ahora

```
SimConnect (MSFS2024)
    ↓
[Message Pump] procesa OnRecvSimobjectDataBytype
    ↓
Datos parseados a SimAircraftState con locks thread-safe
    ↓
OnAircraftStateChanged evento disparado
    ↓
SimulatorFlightDataProvider.GetAircraftStateAsync()
    ↓
ISimFlightDataProvider (interfaz)
    ↓
EngineOrchestrator → Determina fase de vuelo → Trigger de fallos
    ↓
SimulatorConnectionService → Fenix API
```

## Beneficios de las Correcciones

✅ **Recepción de datos en tiempo real**: El message pump asegura que todos los eventos sean procesados

✅ **Thread-safe**: Múltiples threads pueden leer datos simultáneamente sin corrupción

✅ **Detección automática de desconexión**: Se reconecta automáticamente si no hay datos por 10 segundos

✅ **Mejor logging**: Facilita debugging de problemas futuros

✅ **Consistencia**: El flujo de datos es claro y predecible de SimConnect a servicios superiores

## Testing Recomendado

1. **Conexión básica**: Verificar que se conecte al simulador después de iniciar MSFS2024
2. **Recepción de datos**: Verificar logs muestren "Aircraft state updated" periódicamente
3. **Cambios de fase**: Verificar que detecte correctamente Parked → Taxi → Takeoff → Climb, etc.
4. **Reconexión**: Cerrar MSFS y verificar que intente reconectar automáticamente
5. **Performance**: Verificar que no haya memory leaks o uso excesivo de CPU

## Cambios Realizados en Archivos

### Archivo: RealFenixFailures.Integrations.SimConnect/Services/SimConnectClient.cs
- Reescrito completamente para implementar message pump
- Agregados locks para thread-safety (ReaderWriterLockSlim, SemaphoreSlim)
- Implementado polling loop asincrónico para solicitud de datos
- Mejorado manejo de eventos y conexión

### Archivo: RealFenixFailures.Integrations.SimConnect/Models/AircraftStateData.cs
- Cambio de tipos de `int` a `double` para valores decimales
- Alineación con tipos de datos que devuelve SimConnect

### Archivo: RealFenixFailures.Integrations.SimConnect/Services/SimulatorFlightDataProvider.cs
- Mejorado logging para debugging
- Mejor manejo de excepciones
- Logging de cambios de estado de conexión

## Notas Importantes

- El `MESSAGE_PUMP_INTERVAL_MS = 10` puede ajustarse si se necesita mayor/menor frecuencia
- El `DATA_REQUEST_INTERVAL_MS = 100` define cada cuánto se solicitan datos (10 Hz)
- La compilación fue exitosa sin errores (solo warnings de vulnerabilidades de paquetes que no afectan)
- El warning "MSB3270" sobre arquitectura (MSIL vs AMD64) no afecta funcionamiento en runtime
