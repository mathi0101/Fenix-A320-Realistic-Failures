using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect;

public class MockSimConnectClient : ISimConnectClient {
    private bool _isConnected;
    private SimAircraftState _aircraftState;

    // Constructor que permite configurar las respuestas por defecto
    public MockSimConnectClient(bool isConnected = true, SimAircraftState aircraftState = null) {
        _isConnected = isConnected;
        _aircraftState = aircraftState ?? new SimAircraftState { IsConnected = true };
    }

    // Método para actualizar manualmente el estado de conexión
    public void SetConnectionStatus(bool isConnected) {
        _isConnected = isConnected;
    }

    // Método para actualizar manualmente el estado del avión
    public void SetAircraftState(SimAircraftState aircraftState) {
        _aircraftState = aircraftState;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken) {
        return Task.FromResult(_isConnected);
    }

    public Task<SimAircraftState> GetAircraftStateAsync(CancellationToken cancellationToken) {
        return Task.FromResult(_aircraftState);
    }
}