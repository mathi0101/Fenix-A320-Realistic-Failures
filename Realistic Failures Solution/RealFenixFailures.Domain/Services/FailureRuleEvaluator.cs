using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

public class FailureRuleEvaluator : IFailureRuleEvaluator {
    public IReadOnlyList<FenixFailureDefinition> EvaluateEligibleFailures(IEnumerable<FenixFailureDefinition> candidates, FlightPhaseEnum currentPhase) {
        //return candidates
        //    .Where(f => f.ApplicableFlightPhase == FlightPhase.Unknown || f.ApplicableFlightPhase == currentPhase || true)
        //    .ToList();

        return candidates.ToList();
    }
}
