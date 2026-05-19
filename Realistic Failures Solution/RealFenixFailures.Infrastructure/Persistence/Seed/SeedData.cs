using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid PresetColdAndDarkId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC001");
    public static readonly Guid PresetRandomNonCriticalId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC002");
    public static readonly Guid PresetRandomWithCriticalId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC003");
    public static readonly Guid PresetTrainingModeId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC004");
    public static readonly Guid PresetRealisticModeId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC005");
    public static readonly Guid PresetCustomId = Guid.Parse("08FCD6C9-0E35-4A28-86E5-4AB78A4CC006");

    public static readonly Guid CabinLightFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1001");
    public static readonly Guid TempSensorFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1002");
    public static readonly Guid GalleyPowerFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1003");
    public static readonly Guid SecAvionicsFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1004");
    public static readonly Guid PackFaultFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1005");
    public static readonly Guid HydraulicFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1006");
    public static readonly Guid EngineFireFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1007");
    public static readonly Guid AntiIceFailureId = Guid.Parse("C13AFC03-95E4-439F-B464-95CE3A7E1008");

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FailurePreset>().HasData(
            new FailurePreset { Id = PresetColdAndDarkId, Name = "Cold & Dark Immersion", Description = "Fallas menores iniciales para forzar checklist real.", PresetType = PresetType.ColdAndDarkImmersion },
            new FailurePreset { Id = PresetRandomNonCriticalId, Name = "Random Non-Critical", Description = "Fallas aleatorias menores durante el vuelo.", PresetType = PresetType.RandomNonCritical },
            new FailurePreset { Id = PresetRandomWithCriticalId, Name = "Random with Critical", Description = "Fallas aleatorias con posibilidad de fallas críticas.", PresetType = PresetType.RandomWithCritical },
            new FailurePreset { Id = PresetTrainingModeId, Name = "Training Mode", Description = "Fallas críticas por fase para entrenamiento.", PresetType = PresetType.TrainingMode },
            new FailurePreset { Id = PresetRealisticModeId, Name = "Realistic Mode", Description = "Distribución cercana a operación real A320.", PresetType = PresetType.RealisticMode },
            new FailurePreset { Id = PresetCustomId, Name = "Custom", Description = "Preset vacío para reglas del usuario.", PresetType = PresetType.Custom }
        );

        modelBuilder.Entity<FailureDefinition>().HasData(
            new FailureDefinition { Id = CabinLightFailureId, Name = "Cabin Light Burnout", AffectedSystem = "Cabin", ExternalFailureId = "F_PNEUMATIC_CPC_1", Severity = FailureSeverity.Minor, Probability = 0.23, ApplicableFlightPhase = FlightPhase.ColdAndDark },
            new FailureDefinition { Id = TempSensorFailureId, Name = "Cabin Temp Sensor Out of Range", AffectedSystem = "AirConditioning", ExternalFailureId = "F_PNEUMATIC_PACK_1_OVERHEAT", Severity = FailureSeverity.Minor, Probability = 0.18, ApplicableFlightPhase = FlightPhase.ColdAndDark },
            new FailureDefinition { Id = GalleyPowerFailureId, Name = "Galley Bus Intermittent", AffectedSystem = "Electrical", ExternalFailureId = "F_ELEC_AC_ESS_FEED_1", Severity = FailureSeverity.Minor, Probability = 0.14, ApplicableFlightPhase = FlightPhase.Cruise },
            new FailureDefinition { Id = SecAvionicsFailureId, Name = "Secondary Avionics Degraded", AffectedSystem = "Avionics", ExternalFailureId = "F_FMGC_1", Severity = FailureSeverity.Major, Probability = 0.1, ApplicableFlightPhase = FlightPhase.Cruise },
            new FailureDefinition { Id = PackFaultFailureId, Name = "PACK Fault", AffectedSystem = "Pneumatics", ExternalFailureId = "F_PNEUMATIC_TRIM_AIR", Severity = FailureSeverity.Major, Probability = 0.08, ApplicableFlightPhase = FlightPhase.Climb },
            new FailureDefinition { Id = HydraulicFailureId, Name = "Hydraulic Low Pressure", AffectedSystem = "Hydraulics", ExternalFailureId = "F_HYD_LOW_GREEN", Severity = FailureSeverity.Critical, Probability = 0.04, ApplicableFlightPhase = FlightPhase.Descent },
            new FailureDefinition { Id = EngineFireFailureId, Name = "Engine Fire Warning", AffectedSystem = "Engine", ExternalFailureId = "F_OH_FIRE_ENG_1", Severity = FailureSeverity.Critical, Probability = 0.02, ApplicableFlightPhase = FlightPhase.Takeoff },
            new FailureDefinition { Id = AntiIceFailureId, Name = "Wing Anti-Ice Valve Fault", AffectedSystem = "IceProtection", ExternalFailureId = "F_PNEUMATIC_WAI_1", Severity = FailureSeverity.Major, Probability = 0.06, ApplicableFlightPhase = FlightPhase.Approach }
        );

        modelBuilder.Entity("PresetFailureDefinition").HasData(
            new { FailurePresetId = PresetColdAndDarkId, FailureDefinitionId = CabinLightFailureId },
            new { FailurePresetId = PresetColdAndDarkId, FailureDefinitionId = TempSensorFailureId },
            new { FailurePresetId = PresetRandomNonCriticalId, FailureDefinitionId = CabinLightFailureId },
            new { FailurePresetId = PresetRandomNonCriticalId, FailureDefinitionId = TempSensorFailureId },
            new { FailurePresetId = PresetRandomNonCriticalId, FailureDefinitionId = GalleyPowerFailureId },
            new { FailurePresetId = PresetRandomNonCriticalId, FailureDefinitionId = SecAvionicsFailureId },
            new { FailurePresetId = PresetRandomWithCriticalId, FailureDefinitionId = GalleyPowerFailureId },
            new { FailurePresetId = PresetRandomWithCriticalId, FailureDefinitionId = SecAvionicsFailureId },
            new { FailurePresetId = PresetRandomWithCriticalId, FailureDefinitionId = PackFaultFailureId },
            new { FailurePresetId = PresetRandomWithCriticalId, FailureDefinitionId = HydraulicFailureId },
            new { FailurePresetId = PresetRandomWithCriticalId, FailureDefinitionId = EngineFireFailureId },
            new { FailurePresetId = PresetTrainingModeId, FailureDefinitionId = EngineFireFailureId },
            new { FailurePresetId = PresetTrainingModeId, FailureDefinitionId = PackFaultFailureId },
            new { FailurePresetId = PresetTrainingModeId, FailureDefinitionId = HydraulicFailureId },
            new { FailurePresetId = PresetTrainingModeId, FailureDefinitionId = AntiIceFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = CabinLightFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = TempSensorFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = GalleyPowerFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = SecAvionicsFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = PackFaultFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = HydraulicFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = EngineFireFailureId },
            new { FailurePresetId = PresetRealisticModeId, FailureDefinitionId = AntiIceFailureId }
        );
    }
}