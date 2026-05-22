using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

internal class FlightHistoryService : IFlightHistoryService {
    public async Task<FlightHistoryStatsDto> GetStatsAsync(CancellationToken ct) {
        return new FlightHistoryStatsDto(352, 1825, 528, 36, 42, 50);
    }
}
