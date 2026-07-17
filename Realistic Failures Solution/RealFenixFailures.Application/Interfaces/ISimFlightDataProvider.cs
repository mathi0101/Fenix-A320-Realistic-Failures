using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface ISimFlightDataProvider {
    Task<bool> IsConnectedAsync(CancellationToken ct);
    Task<SimulatorAircraftStateSnapshot> GetAircraftRawData(CancellationToken ct);
}
