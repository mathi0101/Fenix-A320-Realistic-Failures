using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;
using System.Collections.Concurrent;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimConnectClient : ISimConnectClient, IDisposable {
    private Microsoft.FlightSimulator.SimConnect.SimConnect? _simConnect;
    private ILogger<SimConnectClient> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly ReaderWriterLockSlim _stateReadWriteLock = new();
    private readonly ConcurrentQueue<Action> _messageQueue = new();
    private CancellationTokenSource? _messagePumpCts;
    private Task? _messagePumpTask;

    private bool _isConnected = false;
    private SimAircraftState _lastKnownState = new SimAircraftState();
    private DateTimeOffset _lastDataReceivedUtc = DateTimeOffset.MinValue;
    private const int MESSAGE_PUMP_INTERVAL_MS = 10;
    private const int DATA_REQUEST_INTERVAL_MS = 100;

    public SimConnectClient(ILogger<SimConnectClient> logger) {
        _logger = logger;
    }

    public event Action<SimAircraftState>? OnAircraftStateChanged;
    public event Action<bool>? OnConnectionStateChanged;

    private enum DATA_DEFINITION {
        AircraftState
    }

    private enum DATA_REQUEST {
        RequestAircraftState
    }

    #region Public Methods

    public async Task<bool> ConnectAsync(CancellationToken ct) {
        await _connectionLock.WaitAsync(ct);
        try {
            if (_isConnected) {
                _logger.LogDebug("Already connected to SimConnect");
                return true;
            }

            try {
                _simConnect = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                    "RealFenixFailures",
                    IntPtr.Zero,
                    0,
                    null,
                    0);

                RegisterSimConnectHandlers();
                RegisterDataDefinitions();
                
                _isConnected = true;
                _logger.LogInformation("Successfully connected to SimConnect");
                OnConnectionStateChanged?.Invoke(true);

                StartMessagePump();
                await StartDataRequestLoopAsync(ct);

                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error connecting to SimConnect: {Message}", ex.Message);
                Disconnect();
                return false;
            }
        } finally {
            _connectionLock.Release();
        }
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct) {
        // Si no estamos conectados, intentar conectar
        if (!_isConnected) {
            return await ConnectAsync(ct);
        }

        // Verificar que la conexión siga siendo válida (datos recientes)
        var timeSinceLastData = DateTimeOffset.UtcNow - _lastDataReceivedUtc;
        if (timeSinceLastData > TimeSpan.FromSeconds(5)) {
            _logger.LogWarning("No data received from SimConnect for {Seconds} seconds", timeSinceLastData.TotalSeconds);
            // Intentar reconectar si no hay datos
            if (timeSinceLastData > TimeSpan.FromSeconds(10)) {
                _logger.LogWarning("Reconnecting to SimConnect due to data timeout");
                Disconnect();
                return await ConnectAsync(ct);
            }
        }

        return _isConnected;
    }

    public Task<SimAircraftState> GetAircraftStateAsync(CancellationToken ct) {
        _stateReadWriteLock.EnterReadLock();
        try {
            return Task.FromResult(_lastKnownState.Clone());
        } finally {
            _stateReadWriteLock.ExitReadLock();
        }
    }

    public void Disconnect() {
        if (_isConnected) {
            try {
                _isConnected = false;
                _messagePumpCts?.Cancel();
                _messagePumpTask?.Wait(TimeSpan.FromSeconds(2));
                _messagePumpCts?.Dispose();
                _messagePumpCts = null;
                
                _simConnect?.Dispose();
                _simConnect = null;
                
                _logger.LogInformation("Disconnected from SimConnect");
                OnConnectionStateChanged?.Invoke(false);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during disconnect");
            }
        }
    }

    public void Dispose() {
        Disconnect();
        _connectionLock.Dispose();
        _stateReadWriteLock.Dispose();
    }

    #endregion

    #region Private Methods - Message Pump

    private void StartMessagePump() {
        if (_messagePumpCts != null) return;

        _messagePumpCts = new CancellationTokenSource();
        _messagePumpTask = Task.Run(() => RunMessagePumpAsync(_messagePumpCts.Token));
        _logger.LogInformation("Message pump started");
    }

    private async Task RunMessagePumpAsync(CancellationToken ct) {
        try {
            while (!ct.IsCancellationRequested && _isConnected) {
                try {
                    if (_simConnect != null) {
                        _simConnect.ReceiveMessage();
                    }
                } catch (InvalidOperationException) {
                    _logger.LogWarning("SimConnect not ready for message pump");
                    _isConnected = false;
                    break;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error in message pump");
                }

                await Task.Delay(MESSAGE_PUMP_INTERVAL_MS, ct);
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Message pump canceled");
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in message pump");
        }
    }

    #endregion

    #region Private Methods - Data Request Loop

    private async Task StartDataRequestLoopAsync(CancellationToken ct) {
        try {
            while (!ct.IsCancellationRequested && _isConnected) {
                RequestData();
                await Task.Delay(DATA_REQUEST_INTERVAL_MS, ct);
            }
        } catch (OperationCanceledException) {
            _logger.LogDebug("Data request loop canceled");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error in data request loop");
        }
    }

    private void RequestData() {
        if (_simConnect == null || !_isConnected) return;

        try {
            _simConnect.RequestDataOnSimObjectType(
                DATA_REQUEST.RequestAircraftState,
                DATA_DEFINITION.AircraftState,
                0,
                SIMCONNECT_SIMOBJECT_TYPE.USER);
        } catch (Exception ex) {
            _logger.LogDebug(ex, "Error requesting data from SimConnect");
        }
    }

    #endregion

    #region Private Methods - SimConnect Event Handlers

    private void RegisterSimConnectHandlers() {
        if (_simConnect == null) return;

        _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
        _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
        _simConnect.OnRecvException += SimConnect_OnRecvException;
        _simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
    }

    private void SimConnect_OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_OPEN data) {
        _logger.LogInformation("SimConnect connection opened successfully");
    }

    private void SimConnect_OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV data) {
        _logger.LogInformation("SimConnect quit event received");
        _isConnected = false;
    }

    private void SimConnect_OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) {
        _logger.LogWarning("SimConnect exception: {Exception}", data.dwException);
    }

    private void SimConnect_OnRecvSimobjectDataBytype(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data) {
        try {
            if (data.dwRequestID == (uint)DATA_REQUEST.RequestAircraftState) {
                var aircraftData = (AircraftStateData)data.dwData[0];
                var newState = MapToAircraftState(aircraftData);

                _stateReadWriteLock.EnterWriteLock();
                try {
                    _lastKnownState = newState;
                    _lastDataReceivedUtc = DateTimeOffset.UtcNow;
                } finally {
                    _stateReadWriteLock.ExitWriteLock();
                }

                _logger.LogDebug("Aircraft state updated: Alt={Altitude}, GS={GroundSpeed}, Phase={IsOnGround}", 
                    newState.Altitude, newState.GroundSpeed, newState.IsOnGround);
                
                OnAircraftStateChanged?.Invoke(newState.Clone());
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error processing aircraft state data");
        }
    }

    #endregion

    #region Private Methods - Data Definition and Mapping

    private void RegisterDataDefinitions() {
        if (_simConnect == null) return;

        try {
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE LATITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE LONGITUDE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "VERTICAL SPEED", "feet per minute", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "SIM ON GROUND", "bool", SIMCONNECT_DATATYPE.INT32, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "FLAPS HANDLE INDEX", "number", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG COMBUSTION:1", "bool", SIMCONNECT_DATATYPE.INT32, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG COMBUSTION:2", "bool", SIMCONNECT_DATATYPE.INT32, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "GENERAL ENG THROTTLE LEVER POSITION:2", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);
            _simConnect.AddToDataDefinition(DATA_DEFINITION.AircraftState, "PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, 0);

            _simConnect.RegisterDataDefineStruct<AircraftStateData>(DATA_DEFINITION.AircraftState);
            _logger.LogDebug("Data definitions registered successfully");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error registering data definitions");
            throw;
        }
    }

    private SimAircraftState MapToAircraftState(AircraftStateData data) {
        return new SimAircraftState {
            IsConnected = true,
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Altitude = (int)data.Altitude,
            Heading = (int)data.Heading,
            GroundSpeed = (int)data.GroundSpeed,
            TrueAirspeed = (int)data.AirspeedTrue,
            VerticalSpeed = (int)data.VerticalSpeed,
            IsOnGround = data.IsOnGround == 1,
            FlapsHandleIndex = (int)data.FlapsHandleIndex,
            Engine1Running = data.Engine1Running == 1,
            Engine2Running = data.Engine2Running == 1,
            ThrottlePercent1 = data.ThrottlePercent1,
            ThrottlePercent2 = data.ThrottlePercent2,
            RadioHeight = (int)data.RadioHeight
        };
    }

    #endregion
}
