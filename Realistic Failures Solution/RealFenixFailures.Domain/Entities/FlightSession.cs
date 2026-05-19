namespace RealFenixFailures.Domain.Entities;

public class FlightSession
{
    public Guid Id { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public Guid PresetId { get; set; }

    public FailurePreset? Preset { get; set; }
    public ICollection<TriggeredFailure> TriggeredFailures { get; set; } = new List<TriggeredFailure>();
}
