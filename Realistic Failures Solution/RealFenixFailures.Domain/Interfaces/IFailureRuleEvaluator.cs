using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFailureRuleEvaluator {
    IReadOnlyList<PresetFailureDefinition> EvaluateEligibleFailures(IEnumerable<PresetFailureDefinition> candidates, FlightPhaseEnum currentPhase);
    IReadOnlyList<PresetFailureDefinition> EvaluateEligibleFailures(FailurePreset preset);
}
