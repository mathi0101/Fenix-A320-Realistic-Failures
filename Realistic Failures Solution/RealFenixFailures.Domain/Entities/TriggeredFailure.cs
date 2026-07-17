using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class TriggeredFailure {
    public int Id { get; set; }
    public required int FlightSessionId { get; set; }
    public required string FenixFailureId { get; set; }
    public DateTime TriggeredAt { get; set; }
    public ComplexFlightPhaseEnum FlightPhase { get; set; }

    public FlightSession? FlightSession { get; set; }
    public FenixFailureDefinition? FenixFailure { get; set; }
}
