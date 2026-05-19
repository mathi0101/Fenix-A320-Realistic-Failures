using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect.Interfaces;

public interface ISimConnectClient
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    Task<SimAircraftState> GetAircraftStateAsync(CancellationToken cancellationToken);
}
