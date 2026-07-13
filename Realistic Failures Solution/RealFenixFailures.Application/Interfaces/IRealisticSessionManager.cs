using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IRealisticSessionManager {
    Task StartAsync(RealisticSessionContext context, CancellationToken ct);
    Task StopAsync(CancellationToken ct);
    Task PauseAsync(CancellationToken ct);
    Task ResumeAsync(CancellationToken ct);
    Task RemoveTemporaryFailuresAsync(CancellationToken ct);

    event EventHandler<FailureTriggeredEventArgs>? FailureTriggered;
    event EventHandler<SessionErrorEventArgs>? SessionError;
}

public class FailureTriggeredEventArgs : EventArgs {
    public required string FenixFailureId { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset TriggeredAtUtc { get; init; }
}

public class SessionErrorEventArgs : EventArgs {
    public required string Message { get; init; }
    public required Exception Exception { get; init; }
}
