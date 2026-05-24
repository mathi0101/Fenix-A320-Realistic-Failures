using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFailureEngine {
    TriggeredFailure? TryTriggerFailure(FailurePreset preset, FlightPhaseEnum currentPhase, double globalProbability, DateTimeOffset timestampUtc);
}
