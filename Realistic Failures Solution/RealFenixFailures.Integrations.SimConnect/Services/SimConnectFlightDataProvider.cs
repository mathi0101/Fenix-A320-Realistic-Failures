using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Integrations.SimConnect.Interfaces;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimConnectFlightDataProvider : IFlightDataProvider {
    private readonly ISimConnectClient _client;

    public SimConnectFlightDataProvider(ISimConnectClient client) {
        _client = client;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken) {
        return _client.IsConnectedAsync(cancellationToken);
    }

    public async Task<FlightPhaseEnum> GetCurrentFlightPhaseAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);
        return state.FlightPhase;
    }
}
