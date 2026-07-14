using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface ISimFlightDataProvider {
    Task<bool> IsConnectedAsync(CancellationToken ct);
    Task<SimulatorAircraftState> GetAircraftRawData(CancellationToken ct);
}
