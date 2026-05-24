using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFailureRuleEvaluator {
    IReadOnlyList<FenixFailureDefinition> EvaluateEligibleFailures(IEnumerable<FenixFailureDefinition> candidates, FlightPhaseEnum currentPhase);
}
