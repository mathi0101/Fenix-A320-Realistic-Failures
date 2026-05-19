namespace RealFenixFailures.Domain.Interfaces;

public interface IFlightHistoryService {
    Task<FlightHistoryStatsDto> GetStatsAsync(CancellationToken ct);
}

public record FlightHistoryStatsDto(
    int TotalFlights,
    double TotalFlightHours,
    int TotalFailuresTriggered,
    int Engine1WearPercent,
    int Engine2WearPercent,
    int HydraulicsWearPercent
);