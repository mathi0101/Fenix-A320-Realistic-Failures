using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Services;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace RealFenixFailures.Application.Services;

public class EngineOrchestrator : IEngineOrchestrator, IDisposable {

    #region Fields

    private readonly IPresetService _presetService;
    private readonly ISessionService _sessionService;
    private readonly ISimulatorConnectionService _simulatorConnectionService;
    private readonly IUserAircraftService _userAircraftService;
    private readonly IRealisticSessionManager _realisticSessionManager;
    private readonly FailureEngineSettings _settings;
    private readonly IFailureTrigger _failureTrigger;
    private readonly ILogger<EngineOrchestrator> _logger;

    private readonly ConcurrentBag<FailureTriggerLogDto> _recentLogs = new();
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    private readonly CancellationTokenSource _realisticEngineCts = new();
    private PeriodicTimer? _realisticEngineTimer;

    private FailurePreset? _activePreset;
    private FlightSession? _activeSession;

    private PeriodicTimer? _automaticTimer;
    private CancellationTokenSource? _automaticTimerCts;


    private List<string> _activeScenarioFailures = new();
    #endregion

    #region TrackedProperties

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool isEngineActive;
    private bool isTimerRunning;
    private UserAppMode currentMode = UserAppMode.None;
    private ConnectionStatusDto connectionStatus = new(false, false, FlightPhaseEnum.ColdAndDark);

    public bool IsEngineActive {
        get => isEngineActive;
        private set {
            if (isEngineActive != value) {
                isEngineActive = value;
                OnPropertyChanged(nameof(IsEngineActive));
            }
        }
    }

    public UserAppMode CurrentMode {
        get => currentMode;
        private set {
            if (currentMode != value) {
                currentMode = value;
                OnPropertyChanged(nameof(CurrentMode));
            }
        }
    }

    public bool IsTimerRunning {
        get => isTimerRunning;
        private set {
            if (isTimerRunning != value) {
                isTimerRunning = value;
                OnPropertyChanged(nameof(IsTimerRunning));
            }
        }
    }
    public ConnectionStatusDto ConnectionStatus {
        get => connectionStatus;
        private set {
            if (connectionStatus != value) {
                connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }
    }

    #endregion

    #region Constructor

    public EngineOrchestrator(
        IPresetService presetService,
        ISessionService sessionService,
        ISimFlightDataProvider flightDataProvider,
        IFailureTrigger failureTrigger,
        ISimulatorConnectionService simulatorService,
        IUserAircraftService userAircraftService,
        IRealisticSessionManager realisticSessionManager,
        IOptions<FailureEngineSettings> settings,
        ILogger<EngineOrchestrator> logger) {
        _presetService = presetService;
        _sessionService = sessionService;
        _simulatorConnectionService = simulatorService;
        _userAircraftService = userAircraftService;
        _realisticSessionManager = realisticSessionManager;
        _failureTrigger = failureTrigger;
        _settings = settings.Value;
        _logger = logger;
    }
    #endregion

    #region Public API

    #region UpdaterTimer

    public async Task StartAutomaticTimerAsync(CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (IsTimerRunning)
                return;

            _automaticTimerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _automaticTimer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.CheckConnectionIntervalSeconds));
            IsTimerRunning = true;

            _ = Task.Run(() => RunAutomaticTimerLoopAsync(_automaticTimerCts.Token), _automaticTimerCts.Token);
        } finally {
            _operationLock.Release();
        }
    }
    public async Task UpdateConnection(CancellationToken ct) {
        ConnectionStatus = await _simulatorConnectionService.GetConnectionStatusAsync(ct);
    }
    public async Task StopAutomaticTimerAsync(CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (!IsTimerRunning)
                return;

            _automaticTimerCts?.Cancel();
            _automaticTimer?.Dispose();
            _automaticTimer = null;
            IsTimerRunning = false;
        } finally {
            _operationLock.Release();
        }
    }

    private async Task RunAutomaticTimerLoopAsync(CancellationToken ct) {
        if (_automaticTimer == null) return;

        try {
            while (await _automaticTimer.WaitForNextTickAsync(ct) && !ct.IsCancellationRequested) {
                ConnectionStatus = await _simulatorConnectionService.GetConnectionStatusAsync(ct);
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Automatic timer loop canceled.");
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in automatic timer loop.");
        }
    }

    #endregion

    #region Realistic Mode

    public async Task StartRealisticModeAsync(RiskLevel risk, CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (!ConnectionStatus.IsSimConnectConnected || !ConnectionStatus.IsFenixConnected) {
                _logger.LogInformation("Tried to start Realistic mode but there is a system disconnected: {@state}", connectionStatus);
                return;
            }
            if (IsEngineActive && CurrentMode != UserAppMode.None) {
                _logger.LogInformation("Tried to start Realistic mode but another mode is already running");
                return;
            }

            var aircraft = await _userAircraftService.GetOrCreateDefaultAsync(ct);
            _activeSession = await _sessionService.StartSessionAsync(risk, aircraft.Id, ct);

            var systemWears = await _userAircraftService.GetSystemWearsAsync(aircraft.Id, ct);

            var context = new RealisticSessionContext(
                aircraft,
                _activeSession,
                systemWears
            );

            CurrentMode = UserAppMode.Realistic;
            IsEngineActive = true;

            await _realisticSessionManager.StartAsync(context, ct);

            _logger.LogInformation("Realistic mode started for aircraft {Registration}. RealisticSessionManager will select from available presets.",
                aircraft.Registration);
        } finally {
            _operationLock.Release();
        }
    }

    #endregion

    #region Training

    public async Task StartTrainingPresetAsync(int presetId, CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (!ConnectionStatus.IsFenixConnected) {
                _logger.LogInformation("Tried to start Training preset but Fenix is disconnected: {@state}", connectionStatus);
                return;
            }
            if (IsEngineActive && CurrentMode != UserAppMode.None) {
                _logger.LogWarning("Cannot start training preset: another mode is already running");
                return;
            }

            _activePreset = await _presetService.GetByIdAsync(presetId, ct);
            if (_activePreset == null) {
                _logger.LogError("Failed to load preset with ID {PresetId}", presetId);
                return;
            }

            var armedFailures = await _simulatorConnectionService.ExecutePresetAsync(_activePreset, _activeSession, ct);

            if (!armedFailures.IsSuccess) {
                _recentLogs.Add(new FailureTriggerLogDto(DateTime.UtcNow, "", "Error al activar preset", _activePreset.FlightPhase, _activePreset.Name));
                return;
            }

            foreach (var def in armedFailures.Value!) {
                var log = new FailureTriggerLogDto(
                    DateTime.UtcNow,
                    def.FenixFailureId,
                    def.FenixFailure!.Name,
                    def.Preset!.FlightPhase,
                    _activePreset.Name);
                _recentLogs.Add(log);
            }

            CurrentMode = UserAppMode.Training;
            IsEngineActive = true;

            _logger.LogInformation("Training preset {PresetId} loaded: {PresetName}",
                presetId, _activePreset.Name);
        } finally {
            _operationLock.Release();
        }
    }

    #endregion

    #region CustomPresets

    public async Task StartCustomModeAsync(int presetId, bool activateImmediately, CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (!ConnectionStatus.IsSimConnectConnected || !ConnectionStatus.IsFenixConnected) {
                _logger.LogInformation("Tried to start Custom Preset but there is a system disconnected: {@state}", connectionStatus);
                return;
            }
            if (IsEngineActive && CurrentMode != UserAppMode.None) {
                _logger.LogWarning("Cannot start custom mode: another mode is already running");
                return;
            }

            _activePreset = await _presetService.GetByIdAsync(presetId, ct);
            if (_activePreset == null) {
                _logger.LogError("Failed to load preset with ID {PresetId}", presetId);
                return;
            }

            if (activateImmediately) {
                foreach (var def in _activePreset.PresetFailureDefinitions) {
                    await _simulatorConnectionService.ExecuteFailureAsync(def, _activeSession, ct);

                    var log = new FailureTriggerLogDto(
                        DateTimeOffset.UtcNow,
                        def.FenixFailureId,
                        def.FenixFailure!.Name,
                        (await _simulatorConnectionService.GetConnectionStatusAsync(ct)).CurrentFlightPhase,
                        _activePreset.Name);
                    _recentLogs.Add(log);
                }

                StartPolling();
            } else {
                var armedFailures = await _simulatorConnectionService.ExecutePresetAsync(_activePreset, _activeSession, ct);

                if (!armedFailures.IsSuccess) {
                    _recentLogs.Add(new FailureTriggerLogDto(DateTime.UtcNow, "", "Error al activar preset", _activePreset.FlightPhase, _activePreset.Name));
                    return;
                }

                foreach (var def in armedFailures.Value!) {
                    var log = new FailureTriggerLogDto(
                        DateTime.UtcNow,
                        def.FenixFailureId,
                        def.FenixFailure!.Name,
                        def.Preset!.FlightPhase,
                        _activePreset.Name);
                    _recentLogs.Add(log);
                }
            }

            CurrentMode = UserAppMode.Custom;
            IsEngineActive = true;

            _logger.LogInformation("Custom mode started with {Count} failures. Activate immediately: {Activate}",
                _activePreset.PresetFailureDefinitions.Count, activateImmediately);
        } finally {
            _operationLock.Release();
        }
    }

    #endregion

    public async Task StopCurrentModeAsync(CancellationToken ct) {
        await _operationLock.WaitAsync(ct);
        try {
            if (!IsEngineActive || CurrentMode == UserAppMode.None) {
                return;
            }

            if (CurrentMode == UserAppMode.Realistic) {
                await _realisticSessionManager.StopAsync(ct);
            }

            StopPolling();

            await _simulatorConnectionService.ResetAllFailuresAsync(ct);

            _activePreset = null;
            _activeSession = null;
            _activeScenarioFailures.Clear();

            CurrentMode = UserAppMode.None;
            IsEngineActive = false;

            _logger.LogInformation("Engine stopped and failures reset.");
        } finally {
            _operationLock.Release();
        }
    }

    public Task<List<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken ct) {
        return Task.FromResult(_recentLogs.OrderByDescending(x => x.TriggeredAtUtc).Take(100).ToList());
    }

    public Task<bool> IsPresetArmedAsync(CancellationToken ct) {
        if (CurrentMode == UserAppMode.Training && _activePreset != null) {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    #endregion

    #region Internal Logic

    private void StartPolling() {
        StopPolling();
        _realisticEngineTimer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.CheckIntervalSeconds));
        _ = Task.Run(RunPollingLoopAsync, _realisticEngineCts.Token);
    }

    private void StopPolling() {
        _realisticEngineTimer?.Dispose();
        _realisticEngineTimer = null;
    }

    private async Task RunPollingLoopAsync() {
        if (_realisticEngineTimer == null) return;

        try {
            await PollAndTriggerAsync(_realisticEngineCts.Token);

            while (await _realisticEngineTimer.WaitForNextTickAsync(_realisticEngineCts.Token) && !_realisticEngineCts.Token.IsCancellationRequested) {
                await PollAndTriggerAsync(_realisticEngineCts.Token);
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Polling loop canceled.");
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error in polling loop.");
        }
    }

    private async Task PollAndTriggerAsync(CancellationToken ct) {
        if (!IsEngineActive || _activePreset == null || _activeSession == null)
            return;

        if (!connectionStatus.IsSimConnectConnected || !connectionStatus.IsFenixConnected)
            return;

        await _operationLock.WaitAsync(ct);
        try {
            var phase = connectionStatus.CurrentFlightPhase;

            if (CurrentMode == UserAppMode.Realistic) {
                var trigger = _failureTrigger.TryTriggerFailure(
                    _activePreset,
                    phase,
                    0.2,
                    DateTimeOffset.UtcNow);

                if (trigger != null) {
                    trigger.FlightSessionId = _activeSession.Id;

                    var failureDef = _activePreset.PresetFailureDefinitions
                        .FirstOrDefault(x => x.FenixFailure?.FenixFailureId == trigger.FenixFailureId);

                    if (failureDef?.FenixFailure != null) {
                        await _simulatorConnectionService.ExecuteFailureAsync(failureDef, _activeSession, ct);

                        var log = new FailureTriggerLogDto(
                            trigger.TriggeredAtUtc,
                            failureDef.FenixFailureId,
                            failureDef.FenixFailure.Name,
                            phase,
                            _activePreset.Name);
                        _recentLogs.Add(log);

                        _logger.LogInformation("Triggered random failure: {FailureName} at phase {Phase}",
                            failureDef.FenixFailure.Name, phase);
                    }
                }
            } else if (CurrentMode == UserAppMode.Custom && _activePreset.PresetType == PresetTypeEnum.Custom) {
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error while polling for failure triggers.");
        } finally {
            _operationLock.Release();
        }
    }

    #endregion

    #region INotifyPropertyChanged

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public void Dispose() {
        _realisticEngineCts.Cancel();
        _realisticEngineTimer?.Dispose();
        _operationLock.Dispose();
        _realisticEngineCts.Dispose();
    }
}