namespace RealFenixFailures.Domain.Entities;

public class AircraftSystemWear {
    public int Id { get; set; }
    public int UserAircraftId { get; set; }
    public int WearableSystemId { get; set; }
    public double WearPercentage { get; set; }
    public DateTimeOffset LastUpdatedAtUtc { get; set; }

    public UserAircraft? UserAircraft { get; set; }
    public AircraftWearableSystem? WearableSystem { get; set; }
}
