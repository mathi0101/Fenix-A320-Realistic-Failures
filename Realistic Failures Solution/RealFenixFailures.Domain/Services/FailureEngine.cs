using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

public class FailureEngine : IFailureEngine {
    private readonly IFailureRuleEvaluator _ruleEvaluator;
    private readonly Random _random;

    public FailureEngine(IFailureRuleEvaluator ruleEvaluator) {
        _ruleEvaluator = ruleEvaluator;
        _random = Random.Shared;
    }

    public TriggeredFailure? TryTriggerFailure(FailurePreset preset, FlightPhaseEnum currentPhase, double globalProbability, DateTimeOffset timestampUtc) {
        if (preset.PresetFailureDefinitions.Count == 0) {
            return null;
        }

        if (_random.NextDouble() > globalProbability) {
            return null;
        }

        var eligible = _ruleEvaluator.EvaluateEligibleFailures(preset.PresetFailureDefinitions.Select(x => x.FenixFailure), currentPhase);
        if (eligible.Count == 0) {
            return null;
        }

        //var weightedCandidates = eligible
        //    .Where(x => x.Probability > 0)
        //    .ToList();
        var weightedCandidates = eligible;


        if (eligible.Count == 0) {
            return null;
        }

        var totalWeight = weightedCandidates.Sum(x => 1); //x.Probability
        var roll = _random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        foreach (var candidate in weightedCandidates) {
            //cumulative += candidate.Probability;
            cumulative += 1;
            if (roll <= cumulative) {
                return new TriggeredFailure {
                    FenixFailureId = candidate.FenixFailureId,
                    PresetId = preset.Id,
                    TriggeredAtUtc = timestampUtc,
                    FlightPhase = currentPhase
                };
            }
        }

        return null;
    }
}
