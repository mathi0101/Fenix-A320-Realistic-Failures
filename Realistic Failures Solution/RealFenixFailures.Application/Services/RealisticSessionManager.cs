using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Services;

public class RealisticSessionManager : IRealisticSessionManager {
    private readonly ILogger<RealisticSessionManager> _logger;
    private readonly ISimulatorConnectionService _simulatorConnectionService;
    private readonly IPresetService _presetService;
    private RealisticSessionState? _sessionState;
    private PeriodicTimer? _evaluationTimer;
    private CancellationTokenSource? _evaluationCts;
    private readonly ReaderWriterLockSlim _stateLock = new();
    private const int EvaluationIntervalSeconds = 5;

    public event EventHandler<FailureTriggeredEventArgs>? FailureTriggered;
    public event EventHandler<SessionErrorEventArgs>? SessionError;

    public RealisticSessionManager(
        ILogger<RealisticSessionManager> logger,
        ISimulatorConnectionService simulatorConnectionService,
        IPresetService presetService) {
        _logger = logger;
        _simulatorConnectionService = simulatorConnectionService;
        _presetService = presetService;
    }

    public async Task StartAsync(RealisticSessionContext context, CancellationToken ct) {
        try {
            _stateLock.EnterWriteLock();
            try {
                _sessionState = new RealisticSessionState {
                    Session = context.Session,
                    Aircraft = context.Aircraft
                };
            } finally {
                _stateLock.ExitWriteLock();
            }

            await InitializeAvailablePresetsAsync(ct);

            _evaluationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _evaluationTimer = new PeriodicTimer(TimeSpan.FromSeconds(EvaluationIntervalSeconds));
            _ = Task.Run(() => RunEvaluationLoopAsync(_evaluationCts.Token), _evaluationCts.Token);

            _logger.LogInformation("Realistic session started for aircraft {Registration}", context.Aircraft.Registration);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error starting realistic session");
            SessionError?.Invoke(this, new SessionErrorEventArgs { Message = "Failed to start realistic session", Exception = ex });
        }
    }

    public async Task StopAsync(CancellationToken ct) {
        try {
            _evaluationCts?.Cancel();
            _evaluationTimer?.Dispose();
            _evaluationTimer = null;

            _stateLock.EnterWriteLock();
            try {
                _sessionState = null;
            } finally {
                _stateLock.ExitWriteLock();
            }

            _logger.LogInformation("Realistic session stopped");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error stopping realistic session");
        }
    }

    public Task PauseAsync(CancellationToken ct) {
        _evaluationCts?.Cancel();
        _logger.LogInformation("Realistic session paused");
        return Task.CompletedTask;
    }

    public async Task ResumeAsync(CancellationToken ct) {
        try {
            _evaluationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _evaluationTimer = new PeriodicTimer(TimeSpan.FromSeconds(EvaluationIntervalSeconds));
            _ = Task.Run(() => RunEvaluationLoopAsync(_evaluationCts.Token), _evaluationCts.Token);

            _logger.LogInformation("Realistic session resumed");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error resuming realistic session");
            SessionError?.Invoke(this, new SessionErrorEventArgs { Message = "Failed to resume session", Exception = ex });
        }
    }

    public async Task RemoveTemporaryFailuresAsync(CancellationToken ct) {
        try {
            _stateLock.EnterReadLock();
            try {
                if (_sessionState == null) return;

                var temporaryFailures = _sessionState.ActiveFailures.ToList();
                foreach (var failure in temporaryFailures) {
                    _sessionState.ActiveFailures.Remove(failure.Key);
                }
            } finally {
                _stateLock.ExitReadLock();
            }

            _logger.LogInformation("Temporary failures removed");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error removing temporary failures");
        }
    }

    private async Task InitializeAvailablePresetsAsync(CancellationToken ct) {
        if (_sessionState is null) return;

        try {
            _sessionState.AvailablePresets = (await _presetService.GetRealisticPresetsAsync(ct)).ToList();
            _logger.LogInformation("Loaded {Count} realistic presets for session", _sessionState.AvailablePresets.Count);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error loading realistic presets");
            SessionError?.Invoke(this, new SessionErrorEventArgs { Message = "Failed to load presets", Exception = ex });
        }
    }

    private async Task RunEvaluationLoopAsync(CancellationToken ct) {
        if (_evaluationTimer == null) return;

        try {
            while (await _evaluationTimer.WaitForNextTickAsync(ct) && !ct.IsCancellationRequested) {
                await EvaluateAndExecuteAsync(ct);
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Evaluation loop canceled");
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in evaluation loop");
        }
    }

    private async Task EvaluateAndExecuteAsync(CancellationToken ct) {
        _stateLock.EnterReadLock();
        try {
            if (_sessionState == null) return;

            var connectionStatus = await _simulatorConnectionService.GetConnectionStatusAsync(ct);
            var currentPhase = connectionStatus.CurrentFlightPhase;

            var applicablePresets = SelectApplicablePresets(currentPhase);
            foreach (var preset in applicablePresets) {
                var candidateFailures = FilterCandidateFailuresByPreset(preset, currentPhase);
                foreach (var failure in candidateFailures) {
                    if (ShouldTriggerFailure(failure)) {
                        await TriggerFailureAsync(failure, preset, ct);
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during evaluation");
        } finally {
            _stateLock.ExitReadLock();
        }
    }

    private List<FailurePreset> SelectApplicablePresets(FlightPhaseEnum phase) {
        if (_sessionState == null) return new();

        return _sessionState.AvailablePresets
            .Where(p => !_sessionState.ExecutedPresets.Contains(p) && IsApplicableToPhase(p, phase))
            .ToList();
    }

    private List<PresetFailureDefinition> FilterCandidateFailuresByPreset(FailurePreset preset, FlightPhaseEnum phase) {
        if (_sessionState == null) return new();

        return preset.PresetFailureDefinitions
            .Where(def => !_sessionState.ExecutedFailureIds.Contains(def.FenixFailureId.GetHashCode()))
            .ToList();
    }

    private bool IsApplicableToPhase(FailurePreset preset, FlightPhaseEnum currentPhase) {
        return preset.FlightPhase == currentPhase;
    }

    private bool ShouldTriggerFailure(PresetFailureDefinition failure) {
        if (_sessionState == null) return false;

        var timeSinceLastFailure = DateTimeOffset.UtcNow - _sessionState.LastFailureTriggeredAtUtc;
        if (timeSinceLastFailure.TotalSeconds < 10) {
            return false;
        }

        var baseProbability = 0.1;
        var random = new Random().NextDouble();
        return random < baseProbability;
    }

    private async Task TriggerFailureAsync(PresetFailureDefinition failure, FailurePreset preset, CancellationToken ct) {
        if (_sessionState == null) return;

        try {
            _sessionState.ExecutedFailureIds.Add(failure.FenixFailureId.GetHashCode());
            _sessionState.ActiveFailures[failure.FenixFailureId.GetHashCode()] = DateTimeOffset.UtcNow;
            _sessionState.LastFailureTriggeredAtUtc = DateTimeOffset.UtcNow;
            _sessionState.FailureCount++;

            if (!_sessionState.ExecutedPresets.Contains(preset)) {
                _sessionState.ExecutedPresets.Add(preset);
            }

            FailureTriggered?.Invoke(this, new FailureTriggeredEventArgs {
                FenixFailureId = failure.FenixFailureId,
                Description = failure.FenixFailure?.Name ?? "Unknown failure",
                TriggeredAtUtc = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Triggered failure {FailureId} from preset {PresetName}",
                failure.FenixFailureId, preset.Name);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error triggering failure");
            SessionError?.Invoke(this, new SessionErrorEventArgs { Message = "Failed to trigger failure", Exception = ex });
        }
    }
}