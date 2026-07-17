using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFailureTrigger {
    IReadOnlyList<PresetFailureDefinition> GetTriggeredPresetFailures(FailurePreset preset);
    TriggeredFailure? TryTriggerFailure(FailurePreset preset, ComplexFlightPhaseEnum currentPhase, double globalProbability, DateTime timestampUtc);
}
