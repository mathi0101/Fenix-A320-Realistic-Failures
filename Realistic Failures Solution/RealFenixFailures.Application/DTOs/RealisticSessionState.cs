using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.DTOs;

public class RealisticSessionState {
    public FlightSession Session { get; set; } = null!;
    public UserAircraft Aircraft { get; set; } = null!;
    
    public List<FailurePreset> ExecutedPresets { get; set; } = new();
    public List<FailurePreset> AvailablePresets { get; set; } = new();
    
    public List<int> ExecutedFailureIds { get; set; } = new();
    public Dictionary<int, DateTimeOffset> ActiveFailures { get; set; } = new();
    public List<int> ArmedFailureIds { get; set; } = new();
    public List<int> PendingFailureIds { get; set; } = new();
    
    public DateTimeOffset LastFailureTriggeredAtUtc { get; set; } = DateTimeOffset.MinValue;
    public int FailureCount { get; set; } = 0;
}
