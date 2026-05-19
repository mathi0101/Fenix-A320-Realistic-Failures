using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IFailureOrchestrator
{
    bool IsEngineActive { get; }
    Task SetActivePresetAsync(Guid presetId, CancellationToken cancellationToken);
    Task ToggleEngineAsync(bool isActive, CancellationToken cancellationToken);
    Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken cancellationToken);
    Task PollAndTriggerAsync(CancellationToken cancellationToken);
    Task StartTrainingScenarioAsync(Guid id, CancellationToken none);
}
