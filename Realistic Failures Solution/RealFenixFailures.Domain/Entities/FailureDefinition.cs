using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class FailureDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AffectedSystem { get; set; } = string.Empty;
    public string ExternalFailureId { get; set; } = string.Empty;
    public FailureSeverity Severity { get; set; }
    public double Probability { get; set; }
    public FlightPhase ApplicableFlightPhase { get; set; }

    public ICollection<FailurePreset> Presets { get; set; } = new List<FailurePreset>();
}
