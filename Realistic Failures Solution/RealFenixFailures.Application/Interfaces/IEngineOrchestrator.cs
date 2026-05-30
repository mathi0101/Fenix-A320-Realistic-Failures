using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IEngineOrchestrator {
    bool IsEngineActive { get; }
    Task ActivatePresetAsync(int presetId, CancellationToken ct);
    Task DeactivatePresetAsync(CancellationToken ct);
    Task ToggleEngineAsync(bool isActive, CancellationToken ct);
    Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken ct);
    Task<List<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken ct);
    Task PollAndTriggerAsync(CancellationToken ct);
}
