using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Interfaces;

public interface ISimFlightDataProvider {
    Task<bool> IsConnectedAsync(CancellationToken ct);
    Task<FlightPhaseEnum> GetCurrentFlightPhaseAsync(CancellationToken ct);
}
