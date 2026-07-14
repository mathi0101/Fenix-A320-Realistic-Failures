using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class TriggeredFailure {
    public int Id { get; set; }
    public int FlightSessionId { get; set; }
    public required string FenixFailureId { get; set; }
    public DateTimeOffset TriggeredAt { get; set; }
    public FlightPhaseEnum FlightPhase { get; set; }

    public FlightSession? FlightSession { get; set; }
    public FenixFailureDefinition? FenixFailure { get; set; }
}
