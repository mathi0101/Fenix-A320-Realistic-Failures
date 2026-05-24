using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.UI.Commands;
using RealFenixFailures.UI.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace RealFenixFailures.UI.ViewModels;

public class DebugViewModel : ObservableObject {
    private readonly IEngineOrchestrator _orchestrator;
    private readonly IPresetService _presetService;
    private readonly IFailureEngineSettings _settings;
    private readonly ILogger<DebugViewModel> _logger;
    private readonly DispatcherTimer _timer;

    private string _simConnectStatus = "Disconnected";
    private string _fenixStatus = "Disconnected";
    private FlightPhaseEnum _currentFlightPhase = FlightPhaseEnum.Unknown;
    private FailurePreset? _selectedPreset;
    private bool _isEngineActive;
    private double _globalProbability;
    private int _checkIntervalSeconds;

    public DebugViewModel(
        IEngineOrchestrator orchestrator,
        IPresetService presetService,
        IFailureEngineSettings settings,
        ILogger<DebugViewModel> logger) {
        _orchestrator = orchestrator;
        _presetService = presetService;
        _settings = settings;
        _logger = logger;

        _globalProbability = settings.GlobalProbability;
        _checkIntervalSeconds = settings.CheckIntervalSeconds;

        Presets = new ObservableCollection<FailurePreset>();
        LogEntries = new ObservableCollection<string>();

        RefreshCommand = new RelayCommand(() => _ = RefreshAsync());
        ToggleEngineCommand = new RelayCommand(() => _ = ToggleEngineAsync());
        ApplySettingsCommand = new RelayCommand(ApplySettings);

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(_checkIntervalSeconds);
        _timer.Tick += async (_, _) => await PollAsync();
        _timer.Start();
    }

    public ObservableCollection<FailurePreset> Presets { get; }
    public ObservableCollection<string> LogEntries { get; }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ToggleEngineCommand { get; }
    public RelayCommand ApplySettingsCommand { get; }

    public string SimConnectStatus {
        get => _simConnectStatus;
        set => SetProperty(ref _simConnectStatus, value);
    }

    public string FenixStatus {
        get => _fenixStatus;
        set => SetProperty(ref _fenixStatus, value);
    }

    public FlightPhaseEnum CurrentFlightPhase {
        get => _currentFlightPhase;
        set => SetProperty(ref _currentFlightPhase, value);
    }

    public FailurePreset? SelectedPreset {
        get => _selectedPreset;
        set => SetProperty(ref _selectedPreset, value);
    }

    public bool IsEngineActive {
        get => _isEngineActive;
        set {
            if (SetProperty(ref _isEngineActive, value)) {
                OnPropertyChanged(nameof(EngineToggleText));
            }
        }
    }

    public string EngineToggleText => IsEngineActive ? "Desactivar motor de fallas" : "Activar motor de fallas";

    public double GlobalProbability {
        get => _globalProbability;
        set => SetProperty(ref _globalProbability, value);
    }

    public int CheckIntervalSeconds {
        get => _checkIntervalSeconds;
        set => SetProperty(ref _checkIntervalSeconds, value);
    }

    public async Task InitializeAsync() {
        await RefreshAsync();
    }

    private async Task RefreshAsync() {
        var presets = await _presetService.GetTrainingPresetsAsync(CancellationToken.None);
        Presets.Clear();
        foreach (var preset in presets) {
            Presets.Add(preset);
        }

        if (SelectedPreset is null && Presets.Count > 0) {
            SelectedPreset = Presets[0];
            await _orchestrator.SetActivePresetAsync(SelectedPreset.Id, CancellationToken.None);
        }

        await UpdateStatusAsync();
        await RefreshLogsAsync();
    }

    private async Task ToggleEngineAsync() {
        if (SelectedPreset is null) {
            return;
        }

        await _orchestrator.SetActivePresetAsync(SelectedPreset.Id, CancellationToken.None);
        var nextState = !IsEngineActive;
        await _orchestrator.ToggleEngineAsync(nextState, CancellationToken.None);
        IsEngineActive = nextState;
        _logger.LogInformation("Failure engine state changed to {State}", nextState);
    }

    private void ApplySettings() {
        _settings.GlobalProbability = Math.Clamp(GlobalProbability, 0, 1);
        _settings.CheckIntervalSeconds = Math.Max(1, CheckIntervalSeconds);
        _timer.Interval = TimeSpan.FromSeconds(_settings.CheckIntervalSeconds);
        _logger.LogInformation("Updated settings: Probability={Probability}, Interval={Interval}", _settings.GlobalProbability, _settings.CheckIntervalSeconds);
    }

    private async Task PollAsync() {
        await _orchestrator.PollAndTriggerAsync(CancellationToken.None);
        await UpdateStatusAsync();
        await RefreshLogsAsync();
    }

    private async Task UpdateStatusAsync() {
        var status = await _orchestrator.GetConnectionStatusAsync(CancellationToken.None);
        SimConnectStatus = status.IsSimConnectConnected ? "Connected" : "Disconnected";
        FenixStatus = status.IsFenixConnected ? "Connected" : "Disconnected";
        CurrentFlightPhase = status.CurrentFlightPhase;
    }

    private async Task RefreshLogsAsync() {
        var logs = await _orchestrator.GetRecentFailuresAsync(CancellationToken.None);
        LogEntries.Clear();
        foreach (var item in logs.OrderByDescending(x => x.TriggeredAtUtc).Take(100)) {
            LogEntries.Add($"{item.TriggeredAtUtc:HH:mm:ss} | {item.PresetName} | {item.FlightPhase} | {item.FailureName}");
        }
    }
}