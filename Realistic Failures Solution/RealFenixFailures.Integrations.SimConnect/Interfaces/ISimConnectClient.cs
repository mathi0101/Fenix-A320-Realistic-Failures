using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect.Interfaces;

public interface ISimConnectClient {
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    public event Action<SimConnectAircraftState>? OnAircraftStateChanged;
    public event Action<bool>? OnConnectionStateChanged;
    Task<SimConnectAircraftState> GetAircraftStateAsync(CancellationToken cancellationToken);
}
