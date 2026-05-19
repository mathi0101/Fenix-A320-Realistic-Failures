using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class TriggeredFailure
{
    public Guid Id { get; set; }
    public Guid FlightSessionId { get; set; }
    public Guid FailureDefinitionId { get; set; }
    public Guid? PresetId { get; set; }
    public DateTimeOffset TriggeredAtUtc { get; set; }
    public FlightPhase FlightPhase { get; set; }

    public FlightSession? FlightSession { get; set; }
    public FailureDefinition? FailureDefinition { get; set; }
    public FailurePreset? Preset { get; set; }
}
