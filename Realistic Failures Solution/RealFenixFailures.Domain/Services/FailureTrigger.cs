using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

public class FailureTrigger : IFailureTrigger {
    private readonly IFailureRuleEvaluator _ruleEvaluator;
    private readonly Random _random;

    public FailureTrigger(IFailureRuleEvaluator ruleEvaluator) {
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

        var eligible = _ruleEvaluator.EvaluateEligibleFailures(preset.PresetFailureDefinitions, currentPhase);
        if (eligible.Count == 0) {
            return null;
        }

        var totalWeight = eligible.Sum(x => x.Probability);
        var roll = _random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        foreach (var candidate in eligible) {
            cumulative += 1 + candidate.Probability;
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

    public IReadOnlyList<PresetFailureDefinition> GetTriggeredPresetFailures(FailurePreset preset) {
        var triggeredFailures = new List<PresetFailureDefinition>();

        var sameFailureGroups = preset.PresetFailureDefinitions.Where(x => x.Probability > 0 && x.ProbabilityGroup is not null).GroupBy(x => x.ProbabilityGroup);
        var independantFailures = preset.PresetFailureDefinitions.Where(x => x.Probability > 0 && x.ProbabilityGroup is null);

        foreach (var group in sameFailureGroups) {
            var candidates = group.ToArray();
            var totalWeight = candidates.Sum(x => x.Probability);
            var roll = _random.NextDouble() * totalWeight;
            var cumulative = 0.0;

            foreach (var candidate in candidates) {
                cumulative += candidate.Probability;
                if (roll <= cumulative) {
                    triggeredFailures.Add(candidate);
                    break;
                }
            }
        }

        foreach (var candidate in independantFailures) {
            var roll = _random.NextDouble();
            if (roll <= candidate.Probability) {
                triggeredFailures.Add(candidate);
            }
        }

        return triggeredFailures;
    }
}
