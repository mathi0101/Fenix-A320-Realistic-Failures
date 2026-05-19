using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimConnectClient : ISimConnectClient
{
    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<SimAircraftState> GetAircraftStateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new SimAircraftState(false, FlightPhase.Unknown));
    }
}