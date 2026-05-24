using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IEngineOrchestrator {
    bool IsEngineActive { get; }
    Task SetActivePresetAsync(int presetId, CancellationToken cancellationToken);
    Task ToggleEngineAsync(bool isActive, CancellationToken cancellationToken);
    Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken cancellationToken);
    Task<List<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken cancellationToken);
    Task PollAndTriggerAsync(CancellationToken cancellationToken);
}
