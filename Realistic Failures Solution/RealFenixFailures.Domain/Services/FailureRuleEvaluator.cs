using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

public class FailureRuleEvaluator : IFailureRuleEvaluator {
    private readonly Random _random;

    public FailureRuleEvaluator() {
        _random = Random.Shared;
    }

    public IReadOnlyList<PresetFailureDefinition> EvaluateEligibleFailures(IEnumerable<PresetFailureDefinition> candidates, ComplexFlightPhaseEnum currentPhase) {
        throw new NotImplementedException();
    }

    public IReadOnlyList<PresetFailureDefinition> EvaluateEligibleFailures(FailurePreset preset) {
        throw new NotImplementedException();

    }
}
