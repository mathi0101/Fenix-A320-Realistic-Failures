using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Interfaces;

public interface IFlightDataProvider
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    Task<FlightPhase> GetCurrentFlightPhaseAsync(CancellationToken cancellationToken);
}
