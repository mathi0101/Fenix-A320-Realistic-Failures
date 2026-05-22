using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Services;


public class TrainingPresetService : ITrainingPresetService {
    private readonly List<TrainingPresetDto> _presets =
[
    new TrainingPresetDto
    {
        Id = 1,
        Name = "Engine Failure Before V1",
        Description = "Falla de motor antes de V1. Requiere rejected takeoff.",
        Phase = FlightPhase.Takeoff,
        Difficulty = TrainingScenarioDifficultyEnum.Hard,
        TriggerDescription = "Se dispara antes de alcanzar V1. Procedimiento: RTO."
    },
    new TrainingPresetDto
    {
        Id = 2,
        Name = "Engine Failure After V1",
        Description = "Falla de motor durante el roll de despegue, después de alcanzar V1.",
        Phase = FlightPhase.Takeoff,
        Difficulty = TrainingScenarioDifficultyEnum.Medium,
        TriggerDescription = "Se dispara automáticamente al detectar V1 durante el despegue."
    },
    new TrainingPresetDto
    {
        Id = 3,
        Name = "Engine Failure After V2",
        Description = "Falla de motor luego de pasar V2 en ascenso inicial.",
        Phase = FlightPhase.Climb,
        Difficulty = TrainingScenarioDifficultyEnum.Medium,
        TriggerDescription = "Se dispara al superar V2 en el ascenso inicial."
    },
    new TrainingPresetDto
    {
        Id = 4,
        Name = "Hydraulic System Failure",
        Description = "Pérdida del sistema hidráulico azul en crucero.",
        Phase = FlightPhase.Cruise,
        Difficulty = TrainingScenarioDifficultyEnum.Medium,
        TriggerDescription = "Se dispara aleatoriamente durante la fase de crucero."
    },
    new TrainingPresetDto
    {
        Id = 5,
        Name = "Dual Bleed Failure",
        Description = "Falla de sangrado en ambos motores. Pérdida de presurización.",
        Phase = FlightPhase.Cruise,
        Difficulty = TrainingScenarioDifficultyEnum.Hard,
        TriggerDescription = "Se dispara en crucero. Requiere descenso de emergencia."
    },
    new TrainingPresetDto
    {
        Id = 6,
        Name = "GPWS Warning on Approach",
        Description = "Activación de GPWS durante la aproximación final.",
        Phase = FlightPhase.Approach,
        Difficulty = TrainingScenarioDifficultyEnum.Easy,
        TriggerDescription = "Se dispara en la aproximación final por debajo de 1000ft AGL."
    },
    new TrainingPresetDto
    {
        Id = 7,
        Name = "Gear Not Down on Final",
        Description = "Tren de aterrizaje no baja correctamente en la final.",
        Phase = FlightPhase.Approach,
        Difficulty = TrainingScenarioDifficultyEnum.Hard,
        TriggerDescription = "Se dispara al seleccionar gear down en la aproximación."
    },
];

    public async Task<IReadOnlyList<TrainingPresetDto>> GetTrainingPresetsAsync(CancellationToken cancellationToken) {
        return _presets;
    }
}
