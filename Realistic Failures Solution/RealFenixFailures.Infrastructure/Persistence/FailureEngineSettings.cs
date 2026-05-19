using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Infrastructure.Persistence;

public class FailureEngineSettings
{
    public const string SectionName = "FailureEngine";

    public double GlobalProbability { get; set; } = 0.05;
    public int CheckIntervalSeconds { get; set; } = 10;
    public FlightPhase ForcedFlightPhaseForStub { get; set; } = FlightPhase.Cruise;
}
