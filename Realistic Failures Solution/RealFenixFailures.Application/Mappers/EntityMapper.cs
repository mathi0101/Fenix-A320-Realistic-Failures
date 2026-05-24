using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Mappers;

internal static class EntityMapper {

    internal static PresetFailureDto ToDto(this PresetFailureDefinition pfd) {
        return new PresetFailureDto {
            FenixFailureId = pfd.FenixFailureId,
            ProbabilityGroup = pfd.ProbabilityGroup,
            Probability = pfd.Probability,
            Ias = pfd.Ias,
            Above_Altitude = pfd.Above_Altitude,
            Below_Altitude = pfd.Below_Altitude,
            Time = pfd.Time,
            AfterEvent = pfd.AfterEvent,
            AfterEventSeconds = pfd.AfterEventSeconds,
        };
    }
    internal static PresetDto ToDto(this FailurePreset pfd) {
        return new PresetDto {
            Id = pfd.Id,
            Name = pfd.Name,
            Description = pfd.Description,
            PresetType = pfd.PresetType,
            Phase = pfd.FlightPhase,
            Difficulty = pfd.Difficulty,
            TriggerDescription = pfd.TriggerDescription,
            Failures = pfd.PresetFailureDefinitions.Select(p => p.ToDto()).ToList()
        };
    }
}
