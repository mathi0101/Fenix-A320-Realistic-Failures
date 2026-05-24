using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Domain.Services;

public class PresetsLoader : IPresetsLoader {
    private readonly List<FailurePreset> _trainingPresets;

    public PresetsLoader() {
        _trainingPresets = GetTrainingPresets()
            .Select(p => new FailurePreset {
                Name = p.Name,
                Description = p.Description,
                TriggerDescription = p.TriggerDescription,
                FlightPhase = p.Phase,
                Difficulty = p.Difficulty,
                PresetType = PresetTypeEnum.TrainingMode,
                PresetFailureDefinitions = p.Failures?.ToList() ?? []
            })
            .ToList();
    }

    public Task<IReadOnlyList<FailurePreset>> GetTrainingPresetsAsync(CancellationToken cancellationToken) {
        return Task.FromResult<IReadOnlyList<FailurePreset>>(_trainingPresets);
    }
    #region Training
    private IReadOnlyCollection<PreloadedPresetDto> GetTrainingPresets() {
        return new List<PreloadedPresetDto>
        {
            new PreloadedPresetDto(
                "Engine Failure Before V1",
                "Falla de motor antes de V1. Requiere rejected takeoff.",
                "Se dispara antes de alcanzar V1. Procedimiento: RTO.",
                FlightPhaseEnum.Takeoff,
                DifficultyEnum.Easy,
                new List<PresetFailureDefinition>
                {
                    new PresetFailureDefinition
                    {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_1_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        Ias = Helpers.Intervalos.AbiertoEntre(80, 130),
                    },
                    new PresetFailureDefinition
                    {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_2_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        Ias = Helpers.Intervalos.AbiertoEntre(80, 130),
                    }
                }
            ),

            new PreloadedPresetDto(
                "Engine Failure After V1",
                "Falla de motor durante el roll de despegue, después de alcanzar V1.",
                "Se dispara automáticamente al detectar V1 durante el despegue.",
                FlightPhaseEnum.Takeoff,
                DifficultyEnum.Hard,
                new List<PresetFailureDefinition>
                {
                    new PresetFailureDefinition
                    {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_1_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        AfterEvent=FenixEvents.V1,
                        AfterEventSeconds = Helpers.Intervalos.AbiertoEntre(1, 5),
                    },
                    new PresetFailureDefinition
                    {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_2_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        AfterEvent=FenixEvents.V1,
                        AfterEventSeconds = Helpers.Intervalos.AbiertoEntre(1, 5),
                    }
                }
            ),

            new PreloadedPresetDto(
                "Engine Failure After V2",
                "Falla de motor luego de pasar V2 en ascenso inicial.",
                "Se dispara al superar V2 en el ascenso inicial.",
                FlightPhaseEnum.Climb,
                DifficultyEnum.Medium,
                [
                    new() {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_1_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        AfterEvent=FenixEvents.V2,
                        AfterEventSeconds = Helpers.Intervalos.AbiertoEntre(1, 15),
                    },
                    new() {
                        FenixFailureId = FenixFailures.Power_plant.Engine_failure.Eng_2_failure,
                        ProbabilityGroup = 1,
                        Probability = 1,
                        AfterEvent=FenixEvents.V2,
                        AfterEventSeconds = Helpers.Intervalos.AbiertoEntre(1, 15),
                    }
                ]
            ),

            new PreloadedPresetDto(
                "Hydraulic System Failure",
                "Pérdida del sistema hidráulico en crucero.",
                "Se dispara aleatoriamente durante la fase de crucero.",
                FlightPhaseEnum.Cruise,
                DifficultyEnum.Easy,
                new List<PresetFailureDefinition>
                {
                    // TODO: reemplazar por los IDs reales de hidráulico cuando los tengas
                }
            ),

            new PreloadedPresetDto(
                "Dual Bleed Failure",
                "Falla de sangrado en ambos motores. Pérdida de presurización.",
                "Se dispara en crucero. Requiere descenso de emergencia.",
                FlightPhaseEnum.Cruise,
                DifficultyEnum.Medium,
                new List<PresetFailureDefinition>
                {
                    // TODO: reemplazar por los IDs reales de bleed cuando los tengas
                }
            ),

            new PreloadedPresetDto(
                "GPWS Warning on Approach",
                "Activación de GPWS durante la aproximación final.",
                "Se dispara en la aproximación final por debajo de 1000ft AGL.",
                FlightPhaseEnum.Approach,
                DifficultyEnum.Easy,
                new List<PresetFailureDefinition>
                {
                    // TODO: reemplazar por el ID real de GPWS cuando lo tengas
                }
            ),

            new PreloadedPresetDto(
                "Gear Not Down on Final",
                "Tren de aterrizaje no baja correctamente en la final.",
                "Se dispara al seleccionar gear down en la aproximación.",
                FlightPhaseEnum.Approach,
                DifficultyEnum.Medium,
                new List<PresetFailureDefinition>
                {
                    // TODO: reemplazar por el ID real de gear cuando lo tengas
                }
            ),
        };
    }

    #endregion
}