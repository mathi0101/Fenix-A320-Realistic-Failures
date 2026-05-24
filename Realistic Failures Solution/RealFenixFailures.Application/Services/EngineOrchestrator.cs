using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using System.Collections.Concurrent;

namespace RealFenixFailures.Application.Services;

public class EngineOrchestrator : IEngineOrchestrator, IDisposable {
    private readonly IPresetService _presetService;
    private readonly ISessionService _sessionService;
    private readonly IFlightDataProvider _flightDataProvider;
    private readonly IFenixFailureDispatcher _fenixDispatcher;
    private readonly IFailureEngineSettings _settings;
    private readonly IFailureTrigger _failureTrigger;
    private readonly ILogger<EngineOrchestrator> _logger;

    private readonly ConcurrentBag<FailureTriggerLogDto> _recentLogs = new();
    private readonly SemaphoreSlim _pollingLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();

    private PeriodicTimer? _timer;
    private FailurePreset? _activePreset;
    private FlightSession? _activeSession;
    private List<string> _activeScenarioFailures = new();

    public bool IsEngineActive { get; private set; }

    public EngineOrchestrator(
        IPresetService presetService,
        ISessionService sessionService,
        IFlightDataProvider flightDataProvider,
        IFailureTrigger failureTrigger,
        IFenixFailureDispatcher fenixDispatcher,
        IFailureEngineSettings settings,
        ILogger<EngineOrchestrator> logger) {
        _presetService = presetService;
        _sessionService = sessionService;
        _flightDataProvider = flightDataProvider;
        _fenixDispatcher = fenixDispatcher;
        _failureTrigger = failureTrigger;
        _settings = settings;
        _logger = logger;
    }

    #region Public API

    public async Task SetActivePresetAsync(int presetId, CancellationToken cancellationToken) {
        _activePreset = await _presetService.GetByIdAsync(presetId, cancellationToken);
        _logger.LogInformation("Preset {PresetId} loaded: {PresetName}", presetId, _activePreset?.Name);
    }

    public async Task ToggleEngineAsync(bool isActive, CancellationToken cancellationToken) {
        if (IsEngineActive == isActive) return;

        IsEngineActive = isActive;

        if (!isActive) {
            await StopEngineAsync(cancellationToken);
            return;
        }

        if (_activePreset == null) {
            _logger.LogWarning("Cannot activate engine: no active preset.");
            return;
        }

        await StartEngineAsync(cancellationToken);
    }

    public void SetPollingInterval(TimeSpan interval) {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive.", nameof(interval));

        _settings.CheckIntervalSeconds = (int)interval.TotalSeconds;
        RestartPolling();
    }

    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken cancellationToken) {
        var simConnected = await _flightDataProvider.IsConnectedAsync(cancellationToken);
        var fenixConnected = await _fenixDispatcher.IsConnectedAsync(cancellationToken);
        var phase = simConnected
            ? await _flightDataProvider.GetCurrentFlightPhaseAsync(cancellationToken)
            : FlightPhaseEnum.Unknown;

        return new ConnectionStatusDto(simConnected, fenixConnected, phase);
    }

    public Task<List<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken cancellationToken) {
        return Task.FromResult(_recentLogs.OrderByDescending(x => x.TriggeredAtUtc).Take(100).ToList());
    }

    #endregion

    #region Internal Logic

    private async Task StartEngineAsync(CancellationToken ct) {
        if (_activePreset == null) return;

        if (_activePreset.PresetType == PresetTypeEnum.RealisticMode)
            await StartRealisticModeAsync(ct);
        else
            await ApplyScenarioPresetAsync(ct);

    }

    private async Task StopEngineAsync(CancellationToken ct) {
        if (_activePreset == null) return;
        if (_activePreset.PresetType == PresetTypeEnum.RealisticMode)
            StopPolling();
        else
            await ResetScenarioPresetAsync(ct);


        _activeSession = null;
        _activeScenarioFailures.Clear();
        await _fenixDispatcher.ResetAllFailuresAsync(ct);
        _logger.LogInformation("Engine stopped and failures reset.");
    }

    private async Task ApplyScenarioPresetAsync(CancellationToken ct) {
        if (_activePreset == null) return;

        _activeSession = await _sessionService.StartSessionAsync(_activePreset.Id, ct);

        await _fenixDispatcher.ExecutePresetAsync(_activePreset, _activeSession, ct);

        foreach (var def in _activePreset.PresetFailureDefinitions) {
            var log = new FailureTriggerLogDto(DateTimeOffset.UtcNow, def.FenixFailureId, def.FenixFailure!.Name, def.Preset!.FlightPhase, _activePreset.Name);
            _recentLogs.Add(log);
        }

    }

    private async Task ResetScenarioPresetAsync(CancellationToken ct) {
        await _fenixDispatcher.ResetAllFailuresAsync(ct);
        _activeScenarioFailures.Clear();
    }

    private async Task StartRealisticModeAsync(CancellationToken ct) {
        _activeSession = await _sessionService.StartSessionAsync(_activePreset!.Id, ct);
        StartPolling();
        _logger.LogInformation("Realistic mode started with polling interval: {Interval}s", _settings.CheckIntervalSeconds);
    }

    private void StartPolling() {
        StopPolling();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.CheckIntervalSeconds));
        _ = Task.Run(RunPollingLoopAsync, _cts.Token);
    }

    private void StopPolling() {
        _timer?.Dispose();
        _timer = null;
    }

    private void RestartPolling() {
        if (IsEngineActive && _activePreset?.PresetType == PresetTypeEnum.RealisticMode) {
            StartPolling();
        }
    }

    private async Task RunPollingLoopAsync() {
        if (_timer == null) return;

        try {
            // Ejecutar primer tick inmediato
            await PollAndTriggerAsync(_cts.Token);

            while (await _timer.WaitForNextTickAsync(_cts.Token) && !_cts.Token.IsCancellationRequested) {
                await PollAndTriggerAsync(_cts.Token);
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Polling loop canceled.");
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in polling loop.");
        }
    }

    public async Task PollAndTriggerAsync(CancellationToken ct) {
        if (!IsEngineActive || _activePreset?.PresetType == PresetTypeEnum.RealisticMode || _activeSession == null)
            return;

        if (!await _fenixDispatcher.IsConnectedAsync(ct))
            return;

        await _pollingLock.WaitAsync(ct);
        try {
            var phase = await _flightDataProvider.GetCurrentFlightPhaseAsync(ct);
            var trigger = _failureTrigger.TryTriggerFailure(_activePreset!, phase, _settings.GlobalProbability, DateTimeOffset.UtcNow);

            if (trigger == null) return;

            trigger.FlightSessionId = _activeSession.Id;

            var failureDef = _activePreset!.PresetFailureDefinitions
                .FirstOrDefault(x => x.FenixFailure?.FenixFailureId == trigger.FenixFailureId);

            if (failureDef?.FenixFailure == null) return;

            await _fenixDispatcher.ExecuteFailureAsync(failureDef, _activeSession, ct);
            //await _triggeredFailureRepository.AddAsync(trigger, ct);
            //await _triggeredFailureRepository.SaveChangesAsync(ct);

            var log = new FailureTriggerLogDto(trigger.TriggeredAtUtc, failureDef.FenixFailureId, failureDef.FenixFailure.Name, phase, _activePreset.Name);
            _recentLogs.Add(log);

            _logger.LogInformation("Triggered random failure: {FailureName} at phase {Phase}", failureDef.FenixFailure.Name, phase);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error while polling for failure triggers.");
        } finally {
            _pollingLock.Release();
        }
    }

    #endregion

    public void Dispose() {
        _cts.Cancel();
        _timer?.Dispose();
        _pollingLock.Dispose();
        _cts.Dispose();
    }
}