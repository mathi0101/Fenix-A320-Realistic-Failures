using Microsoft.Extensions.Options;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Helpers;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Services;

namespace RealFenixFailures.Infrastructure.Services;

public class PresetsLoader : IPresetsLoader {
    private readonly FailureEngineSettings _settings;

    public PresetsLoader(IOptions<FailureEngineSettings> options) {
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<FailurePreset>> GetTrainingPresetsAsync(CancellationToken ct) {
        List<FailurePreset> trainingPresets;
        var jsonDtos = await EmbeddedJsonLoader.LoadFromEmbeddedJson<List<JsonPreloadedPresetDto>>(_settings.TrainingPresetsJson, ct);
        trainingPresets = [.. jsonDtos!.Select(MapToFailurePreset)];


        return trainingPresets;
    }

    private FailurePreset MapToFailurePreset(JsonPreloadedPresetDto dto) {
        var phase = JsonEnumParser.ParseEnum(dto.Phase, ComplexFlightPhaseEnum.Unknown);
        var difficulty = JsonEnumParser.ParseEnum(dto.Difficulty, DifficultyEnum.Easy);

        var preset = new FailurePreset {
            Name = dto.Name,
            Description = dto.Description,
            TriggerDescription = dto.TriggerDescription,
            FlightPhase = phase,
            Difficulty = difficulty,
            PresetType = PresetTypeEnum.TrainingMode,
            PresetFailureDefinitions = dto.Failures?.Select(MapToPresetFailureDefinition).ToList() ?? []
        };

        return preset;
    }

    private PresetFailureDefinition MapToPresetFailureDefinition(JsonPresetFailureDto f) {
        return new PresetFailureDefinition {
            FenixFailureId = f.FenixFailureId,
            ProbabilityGroup = f.ProbabilityGroup,
            Probability = f.Probability,
            Ias = f.Ias,
            Above_Altitude = f.Above_Altitude,
            Below_Altitude = f.Below_Altitude,
            AfterEvent = f.AfterEvent,
            AfterEventSeconds = f.AfterEventSeconds
        };
    }
}