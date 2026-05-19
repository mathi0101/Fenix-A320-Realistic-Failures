using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFailureRuleEvaluator
{
    IReadOnlyList<FailureDefinition> EvaluateEligibleFailures(IEnumerable<FailureDefinition> candidates, FlightPhase currentPhase);
}
