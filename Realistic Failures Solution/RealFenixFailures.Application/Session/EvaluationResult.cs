namespace RealFenixFailures.Application.Session;

/// <summary>
/// Immutable result returned by <c>SessionProcessorEngine.EvaluateAsync</c> on every timer tick.
/// The manager inspects this to decide whether it must pause the session (lost connection)
/// and which failures (if any) were triggered so it can raise the corresponding events.
/// </summary>
public sealed class EvaluationResult {
    /// <summary>
    /// When true, the manager should pause the session (e.g. the simulator/Fenix connection was lost).
    /// </summary>
    public bool ShouldPauseSession { get; }

    /// <summary>
    /// Failures that were successfully triggered during this evaluation tick.
    /// Empty when nothing was triggered.
    /// </summary>
    public IReadOnlyList<TriggeredFailureInfo> TriggeredFailures { get; }

    private EvaluationResult(bool shouldPauseSession, IReadOnlyList<TriggeredFailureInfo> triggeredFailures) {
        ShouldPauseSession = shouldPauseSession;
        TriggeredFailures = triggeredFailures;
    }

    /// <summary>
    /// The simulator/Fenix connection is not available. The manager should pause the session.
    /// </summary>
    public static EvaluationResult Disconnected() =>
        new(shouldPauseSession: true, triggeredFailures: Array.Empty<TriggeredFailureInfo>());

    /// <summary>
    /// The tick completed normally but no failure was triggered (cooldown, probability roll, guards, etc.).
    /// </summary>
    public static EvaluationResult NoAction() =>
        new(shouldPauseSession: false, triggeredFailures: Array.Empty<TriggeredFailureInfo>());

    /// <summary>
    /// The tick completed and one or more failures were triggered.
    /// </summary>
    public static EvaluationResult Completed(IReadOnlyList<TriggeredFailureInfo> triggeredFailures) =>
        new(shouldPauseSession: false, triggeredFailures: triggeredFailures ?? Array.Empty<TriggeredFailureInfo>());
}

/// <summary>
/// Lightweight, transport-friendly description of a failure that was triggered during evaluation.
/// The manager maps this onto <c>FailureTriggeredEventArgs</c> when raising UI events.
/// </summary>
public record TriggeredFailureInfo(
    string FenixFailureId,
    string Description,
    string PresetName,
    DateTime TriggeredAtUtc);
