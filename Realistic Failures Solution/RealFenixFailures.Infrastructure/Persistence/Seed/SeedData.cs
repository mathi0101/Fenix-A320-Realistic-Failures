using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Infrastructure.Persistence;

public static class SeedData {
    public static readonly int PresetColdAndDarkId = 1;
    public static readonly int PresetRandomNonCriticalId = 2;
    public static readonly int PresetRandomWithCriticalId = 3;
    public static readonly int PresetTrainingModeId = 4;
    public static readonly int PresetRealisticModeId = 5;
    public static readonly int PresetCustomId = 6;

    public static void Apply(ModelBuilder modelBuilder) {
        modelBuilder.Entity<FailurePreset>().HasData(
            new FailurePreset { Id = PresetColdAndDarkId, Name = "Cold & Dark Immersion", Description = "Fallas menores iniciales para forzar checklist real.", PresetType = PresetType.ColdAndDarkImmersion },
            new FailurePreset { Id = PresetRandomNonCriticalId, Name = "Random Non-Critical", Description = "Fallas aleatorias menores durante el vuelo.", PresetType = PresetType.RandomNonCritical },
            new FailurePreset { Id = PresetRandomWithCriticalId, Name = "Random with Critical", Description = "Fallas aleatorias con posibilidad de fallas críticas.", PresetType = PresetType.RandomWithCritical },
            new FailurePreset { Id = PresetTrainingModeId, Name = "Training Mode", Description = "Fallas críticas por fase para entrenamiento.", PresetType = PresetType.TrainingMode },
            new FailurePreset { Id = PresetRealisticModeId, Name = "Realistic Mode", Description = "Distribución cercana a operación real A320.", PresetType = PresetType.RealisticMode },
            new FailurePreset { Id = PresetCustomId, Name = "Custom", Description = "Preset vacío para reglas del usuario.", PresetType = PresetType.Custom }
        );


    }
}