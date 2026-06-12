using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimConnectClient : ISimConnectClient, IDisposable {
    private Microsoft.FlightSimulator.SimConnect.SimConnect? _simConnect;
    private ILogger<SimConnectClient> _logger;
    private bool IsConnected => _simConnect != null;

    public SimConnectClient(ILogger<SimConnectClient> logger) {
        _logger = logger;
    }

    // Eventos para notificar cambios
    public event Action<SimAircraftState>? OnAircraftStateChanged;
    public event Action<bool>? OnConnectionStateChanged;

    // Último estado conocido
    private SimAircraftState _lastKnownState = new SimAircraftState();

    // Definiciones para SimConnect
    private enum DATA_DEFINITION {
        AircraftState
    }

    private enum DATA_REQUEST {
        RequestAircraftState
    }


    #region Public

    public async Task<bool> ConnectAsync(CancellationToken ct) {
        try {
            // Intentar conectar con SimConnect
            _simConnect = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                "RealFenixFailures",
                IntPtr.Zero,
                0,
                null,
                0);
            // Registrar manejadores de eventos
            RegisterSimConnectHandlers();

            // Registrar definiciones de datos
            RegisterDataDefinitions();

            // Notificar cambio de estado
            OnConnectionStateChanged?.Invoke(true);

            // Iniciar solicitud de datos
            RequestData();

            return true;
        } catch (Exception ex) {
            // Loggear error si tienes logger
            _logger.LogDebug($"Error al conectar SimConnect: {ex.Message}");
            Disconnect();
            return false;
        }
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct) {
        if (!IsConnected)
            await ConnectAsync(ct);
        return IsConnected;
    }

    public Task<SimAircraftState> GetAircraftStateAsync(CancellationToken ct) {
        UpdateData();
        return Task.FromResult(_lastKnownState.Clone());
    }




    public void UpdateData() {
        if (_simConnect == null) return;

        try {
            // Solicitar datos actualizados
            _simConnect.RequestDataOnSimObjectType(
                DATA_REQUEST.RequestAircraftState,
                DATA_DEFINITION.AircraftState,
                0,
                SIMCONNECT_SIMOBJECT_TYPE.USER);
        } catch (Exception ex) {
            _logger.LogWarning($"Error actualizando datos: {ex.Message}");
        }
    }
    public void Disconnect() {
        if (IsConnected) {
            try {
                _simConnect!.Dispose();
            } catch (Exception ex) {
                _logger.LogDebug($"Error cerrando conexión SimConnect: {ex.Message}");
            } finally {
                _simConnect = null;
            }

            OnConnectionStateChanged?.Invoke(false);
        }
    }

    public void Dispose() {
        Disconnect();
    }
    #endregion

    #region SimConnect Events
    private void RegisterSimConnectHandlers() {
        if (_simConnect == null) return;

        _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
        _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
        _simConnect.OnRecvException += SimConnect_OnRecvException;
        _simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
    }
    private void SimConnect_OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_OPEN data) {
        _logger.LogInformation("Conexión SimConnect abierta");
        _logger.LogDebug("Sender: {@sender}", sender);
        _logger.LogInformation("Data: {@data}", data);
    }

    private void SimConnect_OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV data) {
        _logger.LogInformation("Conexión SimConnect cerrada");
        _logger.LogDebug("Sender: {@sender}", sender);
        _logger.LogDebug("Data: {@data}", data);
        Disconnect();
    }

    private void SimConnect_OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) {
        _logger.LogWarning($"Excepción SimConnect: {data.dwException}");
    }

    private void SimConnect_OnRecvSimobjectDataBytype(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data) {
        _logger.LogInformation("Event SimConnect_OnRecvSimobjectDataBytype");
        _logger.LogDebug("Sender: {@sender}", sender);
        _logger.LogDebug("Data: {@data}", data);
        if (data.dwRequestID == (uint)DATA_REQUEST.RequestAircraftState) {
            var aircraftData = (AircraftStateData)data.dwData[0];
            var newState = MapToAircraftState(aircraftData);


            _lastKnownState = newState;

            // Notificar cambio de estado
            OnAircraftStateChanged?.Invoke(newState.Clone());
        }
    }
    #endregion

    #region Private

    private void RequestData() {
        if (_simConnect == null) return;

        // Solicitar datos cada cierto tiempo
        _simConnect.RequestDataOnSimObjectType(
            DATA_REQUEST.RequestAircraftState,
            DATA_DEFINITION.AircraftState,
            0,
            SIMCONNECT_SIMOBJECT_TYPE.USER);
    }


    private void RegisterDataDefinitions() {
        if (_simConnect == null) return;

        // Registrar definición de datos del estado de la aeronave
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "VERTICAL SPEED", "feet per minute", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "SIM ON GROUND", "bool", SIMCONNECT_DATATYPE.INT8, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "FLAPS HANDLE INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG COMBUSTION:1", "bool", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG COMBUSTION:2", "bool", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG THROTTLE LEVER POSITION:2", "percent", SIMCONNECT_DATATYPE.INT32, 0, 0);
        _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.INT32, 0, 0);

        // Registrar la estructura
        _simConnect.RegisterDataDefineStruct<AircraftStateData>(DATA_DEFINITION.AircraftState);
    }


    private SimAircraftState MapToAircraftState(AircraftStateData data) {
        // Mapear datos crudos sin procesamiento de lógica de negocio
        return new SimAircraftState() {
            IsConnected = true,
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Altitude = data.Altitude,
            Heading = data.Heading,
            GroundSpeed = data.GroundSpeed,
            AirspeedTrue = data.AirspeedTrue,
            VerticalSpeed = data.VerticalSpeed,
            IsOnGround = data.IsOnGround == 1,
            FlapsHandleIndex = (int)data.FlapsHandleIndex,
            Engine1Running = data.Engine1Running == 1,
            Engine2Running = data.Engine2Running == 1,
            ThrottlePercent1 = data.ThrottlePercent1,
            ThrottlePercent2 = data.ThrottlePercent2,
            RadioHeight = data.RadioHeight
        };
    }


    #endregion

}