using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect;

public class MockSimConnectClient : ISimConnectClient {
    private bool _isConnected;
    private SimConnectAircraftState _aircraftState;

    public event Action<SimConnectAircraftState>? OnAircraftStateChanged;
    public event Action<bool>? OnConnectionStateChanged;

    // Constructor que permite configurar las respuestas por defecto
    public MockSimConnectClient(bool isConnected = true, SimConnectAircraftState aircraftState = null) {
        _isConnected = isConnected;
        _aircraftState = aircraftState ?? new SimConnectAircraftState { IsConnected = true, IsOnGround = true };
    }

    // Método para actualizar manualmente el estado de conexión
    public void SetConnectionStatus(bool isConnected) {
        _isConnected = isConnected;
    }

    // Método para actualizar manualmente el estado del avión
    public void SetAircraftState(SimConnectAircraftState aircraftState) {
        _aircraftState = aircraftState;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken) {
        return Task.FromResult(_isConnected);
    }

    public Task<SimConnectAircraftState> GetAircraftStateAsync(CancellationToken cancellationToken) {
        return Task.FromResult(_aircraftState);
    }
}