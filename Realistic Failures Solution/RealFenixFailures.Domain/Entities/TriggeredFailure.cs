using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class TriggeredFailure {
    public int Id { get; set; }
    public int FlightSessionId { get; set; }
    public int FailureDefinitionId { get; set; }
    public int? PresetId { get; set; }
    public DateTimeOffset TriggeredAtUtc { get; set; }
    public FlightPhase FlightPhase { get; set; }

    public FlightSession? FlightSession { get; set; }
    public FenixFailureDefinition? FailureDefinition { get; set; }
    public FailurePreset? Preset { get; set; }
}
