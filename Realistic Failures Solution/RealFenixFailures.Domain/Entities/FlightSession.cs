namespace RealFenixFailures.Domain.Entities;

public class FlightSession {
    public int Id { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public int PresetId { get; set; }

    public FailurePreset? Preset { get; set; }
    public ICollection<TriggeredFailure> TriggeredFailures { get; set; } = new List<TriggeredFailure>();
}
