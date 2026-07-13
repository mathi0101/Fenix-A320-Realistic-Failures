namespace RealFenixFailures.Domain.Entities;

public class FlightSession {
    public int Id { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public int? UserAircraftId { get; set; }
    public int? RiskLevel { get; set; }

    public UserAircraft? UserAircraft { get; set; }
    public ICollection<TriggeredFailure> TriggeredFailures { get; set; } = new List<TriggeredFailure>();
}
