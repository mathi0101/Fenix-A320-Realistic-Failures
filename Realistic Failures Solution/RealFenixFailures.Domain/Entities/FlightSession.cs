namespace RealFenixFailures.Domain.Entities;

public class FlightSession {
    public int Id { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int RiskLevel { get; set; } // 1- Low | 2- Moderate | 3-Hard
    public int UserAircraftId { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public UserAircraft? UserAircraft { get; set; }
    public ICollection<TriggeredFailure> TriggeredFailures { get; set; } = new List<TriggeredFailure>();
}
