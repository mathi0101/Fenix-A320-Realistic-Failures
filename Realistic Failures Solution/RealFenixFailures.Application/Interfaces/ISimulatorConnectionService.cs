using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface ISimulatorConnectionService {
    Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken ct);

    Task<bool> ExecuteFailureAsync(PresetFailureDefinition failureDefinition, FlightSession session, CancellationToken ct);
    Task<IReadOnlyList<PresetFailureDefinition>> ExecutePresetAsync(FailurePreset preset, FlightSession session, CancellationToken ct);
    Task<bool> ResetAllFailuresAsync(CancellationToken ct);
}
