using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Infrastructure.Configuration;

public class FailureEngineSettings {
    public const string SectionName = "FailureEngine";

    public double GlobalProbability { get; set; } = 0.05;
    public int CheckIntervalSeconds { get; set; } = 10;
    public FlightPhaseEnum ForcedFlightPhaseForStub { get; set; } = FlightPhaseEnum.Cruise;

    public string FailuresJson { get; set; } = string.Empty;
    public string TrainingPresetsJson { get; set; } = string.Empty;
}
