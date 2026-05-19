using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

internal class FlightHistoryService : IFlightHistoryService {
    public Task<FlightHistoryStatsDto> GetStatsAsync(CancellationToken ct) {
        throw new NotImplementedException();
    }
}
