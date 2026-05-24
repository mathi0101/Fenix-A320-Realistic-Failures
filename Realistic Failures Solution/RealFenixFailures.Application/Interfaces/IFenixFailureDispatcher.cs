using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixFailureDispatcher {
    Task<bool> IsConnectedAsync(CancellationToken ct);
    Task ExecuteFailureAsync(PresetFailureDefinition failureDefinition, FlightSession session, CancellationToken ct);
    Task ExecutePresetAsync(FailurePreset preset, FlightSession session, CancellationToken ct);
    Task ResetAllFailuresAsync(CancellationToken ct);
}
