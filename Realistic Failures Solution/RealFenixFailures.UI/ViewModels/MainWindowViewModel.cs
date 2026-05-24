using global::RealFenixFailures.Application.Interfaces;
using global::RealFenixFailures.Domain.Enums;
using global::RealFenixFailures.UI.Commands;
using global::RealFenixFailures.UI.ViewModels.Base;
using Microsoft.Extensions.Logging;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.UI.ViewModels.Extra;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RealFenixFailures.UI.ViewModels;

// ─── MainViewModel ─────────────────────────────────────────────────────────────

public class MainWindowViewModel : ObservableObject {
    private readonly IEngineOrchestrator _orchestrator;
    private readonly IPresetService _presetService;
    private readonly IFlightHistoryService _flightHistoryService;
    private readonly ILogger<MainWindowViewModel> _logger;

    // Connection state
    private string _simConnectStatus = "Disconnected";
    private string _fenixStatus = "Disconnected";
    private FlightPhaseEnum _currentFlightPhase = FlightPhaseEnum.Unknown;
    private bool _isSimConnectConnected;
    private bool _isFenixConnected;

    // Engine state
    private bool _isEngineActive;

    // Mode selection
    private bool _isRealisticModeSelected;
    private bool _isTrainingModeSelected = true;
    private bool _isCustomModeSelected;

    // Training
    private TrainingScenarioViewModel? _selectedTrainingScenario;

    // Custom
    private double _globalProbability = 0.2;
    private int _checkIntervalSeconds = 5;

    // Realistic stats
    private int _totalFlights;
    private double _totalFlightHours;
    private int _totalFailuresTriggered;
    private int _engine1WearPercent;
    private int _engine2WearPercent;
    private int _hydraulicsWearPercent;

    public MainWindowViewModel(
        IEngineOrchestrator orchestrator,
        IPresetService presetService,
        IFlightHistoryService flightHistoryService,
        ILogger<MainWindowViewModel> logger) {
        _orchestrator = orchestrator;
        _presetService = presetService;
        _flightHistoryService = flightHistoryService;
        _logger = logger;

        TrainingScenarios = new ObservableCollection<TrainingScenarioViewModel>();
        CustomPresets = new ObservableCollection<CustomPresetViewModel>();
        LogEntries = new ObservableCollection<string>();

        RefreshCommand = new RelayCommand(() => _ = RefreshAsync());
        ToggleEngineCommand = new RelayCommand(() => _ = ToggleEngineAsync());
        ToggleRealisticModeCommand = new RelayCommand(() => _ = ToggleEngineAsync());
        ApplySettingsCommand = new RelayCommand(ApplySettings);
        SelectTrainingScenarioCommand = new RelayCommand<TrainingScenarioViewModel>(SelectTrainingScenario);
        StartTrainingScenarioCommand = new RelayCommand(() => _ = StartTrainingScenarioAsync());
        CreateCustomPresetCommand = new RelayCommand(() => _ = CreateCustomPresetAsync());
        ActivateCustomPresetCommand = new RelayCommand<CustomPresetViewModel>(p => _ = ActivateCustomPresetAsync(p));
        DeleteCustomPresetCommand = new RelayCommand<CustomPresetViewModel>(p => _ = DeleteCustomPresetAsync(p));
    }

    // ── Collections ──────────────────────────────────────────────────────────

    public ObservableCollection<TrainingScenarioViewModel> TrainingScenarios { get; }
    public ObservableCollection<CustomPresetViewModel> CustomPresets { get; }
    public ObservableCollection<string> LogEntries { get; }

    // ── Commands ─────────────────────────────────────────────────────────────

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ToggleEngineCommand { get; }
    public RelayCommand ToggleRealisticModeCommand { get; }
    public RelayCommand ApplySettingsCommand { get; }
    public RelayCommand<TrainingScenarioViewModel> SelectTrainingScenarioCommand { get; }
    public RelayCommand StartTrainingScenarioCommand { get; }
    public RelayCommand CreateCustomPresetCommand { get; }
    public RelayCommand<CustomPresetViewModel> ActivateCustomPresetCommand { get; }
    public RelayCommand<CustomPresetViewModel> DeleteCustomPresetCommand { get; }

    // ── Connection status ─────────────────────────────────────────────────────

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
        set {
            if (SetProperty(ref _currentFlightPhase, value))
                OnPropertyChanged(nameof(CurrentFlightPhaseDisplay));
        }
    }

    public string CurrentFlightPhaseDisplay => CurrentFlightPhase.ToString().ToUpperInvariant();

    public Color SimConnectDotColor => _isSimConnectConnected ? Color.FromRgb(34, 197, 94) : Color.FromRgb(100, 116, 139);
    public Color FenixDotColor => _isFenixConnected ? Color.FromRgb(34, 197, 94) : Color.FromRgb(100, 116, 139);

    public Brush SimConnectStatusBrush => _isSimConnectConnected
        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
        : new SolidColorBrush(Color.FromRgb(100, 116, 139));

    public Brush FenixStatusBrush => _isFenixConnected
        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
        : new SolidColorBrush(Color.FromRgb(100, 116, 139));

    public bool CanStartEngine => _isSimConnectConnected && _isFenixConnected;
    public bool ShowConnectionWarning => !CanStartEngine;

    // ── Engine ────────────────────────────────────────────────────────────────

    public bool IsEngineActive {
        get => _isEngineActive;
        set {
            if (SetProperty(ref _isEngineActive, value))
                OnPropertyChanged(nameof(EngineToggleText));
        }
    }

    public string EngineToggleText => IsEngineActive ? "DETENER MOTOR" : "ACTIVAR MOTOR";

    // ── Mode selection ────────────────────────────────────────────────────────

    public bool IsTrainingModeSelected {
        get => _isTrainingModeSelected;
        set => SetProperty(ref _isTrainingModeSelected, value);
    }

    public bool IsCustomModeSelected {
        get => _isCustomModeSelected;
        set => SetProperty(ref _isCustomModeSelected, value);
    }

    public bool IsRealisticModeSelected {
        get => _isRealisticModeSelected;
        set => SetProperty(ref _isRealisticModeSelected, value);
    }

    // ── Training ──────────────────────────────────────────────────────────────

    public TrainingScenarioViewModel? SelectedTrainingScenario {
        get => _selectedTrainingScenario;
        set {
            if (SetProperty(ref _selectedTrainingScenario, value))
                OnPropertyChanged(nameof(HasSelectedTrainingScenario));
        }
    }

    public bool HasSelectedTrainingScenario => SelectedTrainingScenario is not null;

    // ── Custom settings ───────────────────────────────────────────────────────

    public double GlobalProbability {
        get => _globalProbability;
        set => SetProperty(ref _globalProbability, value);
    }

    public int CheckIntervalSeconds {
        get => _checkIntervalSeconds;
        set => SetProperty(ref _checkIntervalSeconds, value);
    }

    // ── Realistic stats ───────────────────────────────────────────────────────

    public int TotalFlights {
        get => _totalFlights;
        set => SetProperty(ref _totalFlights, value);
    }

    public double TotalFlightHours {
        get => _totalFlightHours;
        set => SetProperty(ref _totalFlightHours, value);
    }

    public int TotalFailuresTriggered {
        get => _totalFailuresTriggered;
        set => SetProperty(ref _totalFailuresTriggered, value);
    }

    public int Engine1WearPercent {
        get => _engine1WearPercent;
        set {
            if (SetProperty(ref _engine1WearPercent, value)) {
                OnPropertyChanged(nameof(Engine1WearWidth));
                OnPropertyChanged(nameof(Engine1WearColor));
            }
        }
    }

    public int Engine2WearPercent {
        get => _engine2WearPercent;
        set {
            if (SetProperty(ref _engine2WearPercent, value)) {
                OnPropertyChanged(nameof(Engine2WearWidth));
                OnPropertyChanged(nameof(Engine2WearColor));
            }
        }
    }

    public int HydraulicsWearPercent {
        get => _hydraulicsWearPercent;
        set {
            if (SetProperty(ref _hydraulicsWearPercent, value)) {
                OnPropertyChanged(nameof(HydraulicsWearWidth));
                OnPropertyChanged(nameof(HydraulicsWearColor));
            }
        }
    }

    // Wear bar widths (bound to Border Width — requires converter or fixed max width approach)
    // These return a double representing percentage of a ~200px bar
    public double Engine1WearWidth => Engine1WearPercent * 2.0;
    public double Engine2WearWidth => Engine2WearPercent * 2.0;
    public double HydraulicsWearWidth => HydraulicsWearPercent * 2.0;

    public Color Engine1WearColor => WearColor(Engine1WearPercent);
    public Color Engine2WearColor => WearColor(Engine2WearPercent);
    public Color HydraulicsWearColor => WearColor(HydraulicsWearPercent);

    private static Color WearColor(int percent) => percent switch {
        < 40 => Color.FromRgb(34, 197, 94),   // green
        < 70 => Color.FromRgb(245, 158, 11),  // amber
        _ => Color.FromRgb(239, 68, 68)        // red
    };

    // ── Init ──────────────────────────────────────────────────────────────────

    #region Init

    public async Task InitializeAsync() {
        await LoadTrainingScenarios();
        await RefreshAsync();
    }

    #endregion

    // ── Private methods ───────────────────────────────────────────────────────

    #region Private methods


    private async Task LoadTrainingScenarios() {
        // These are hardcoded UI scenarios; the actual preset IDs are resolved
        // by the orchestrator when a scenario is started.
        TrainingScenarios.Clear();
        var presets = await _presetService.GetTrainingPresetsAsync(CancellationToken.None);
        if (presets.Count == 0) return;
        foreach (var p in presets) {
            TrainingScenarios.Add(
                new TrainingScenarioViewModel() {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TriggerDescription = p.TriggerDescription,
                    Phase = p.FlightPhase.ToString(),
                    Difficulty = p.Difficulty.ToString(),
                });
        }
    }

    private void SelectTrainingScenario(TrainingScenarioViewModel? scenario) {
        foreach (var s in TrainingScenarios)
            s.IsSelected = false;

        if (scenario is not null)
            scenario.IsSelected = true;

        SelectedTrainingScenario = scenario;
    }

    private async Task StartTrainingScenarioAsync() {
        if (SelectedTrainingScenario is null) return;

        await _orchestrator.SetActivePresetAsync(SelectedTrainingScenario.Id, CancellationToken.None);
        await ToggleEngineAsync();
        _logger.LogInformation("Training scenario started: {Name}", SelectedTrainingScenario.Name);
    }

    private async Task ToggleEngineAsync() {
        var nextState = !IsEngineActive;
        await _orchestrator.ToggleEngineAsync(nextState, CancellationToken.None);
        IsEngineActive = nextState;
        _logger.LogInformation("Engine toggled to {State}", nextState);
    }

    private void ApplySettings() {
        GlobalProbability = Math.Clamp(GlobalProbability, 0, 1);
        CheckIntervalSeconds = Math.Max(5, CheckIntervalSeconds);
    }

    private async Task CreateCustomPresetAsync() {
        // NEW method required in IPresetService:
        // Task<PresetDto> CreateEmptyCustomPresetAsync(CancellationToken ct)
        //var newPreset = await _presetService.CreateEmptyCustomPresetAsync(CancellationToken.None);
        //CustomPresets.Add(new CustomPresetViewModel {
        //    Id = newPreset.Id,
        //    Name = newPreset.Name,
        //    FailureCount = newPreset.FailureCount
        //});
    }

    private async Task ActivateCustomPresetAsync(CustomPresetViewModel? preset) {
        if (preset is null) return;
        await _orchestrator.SetActivePresetAsync(preset.Id, CancellationToken.None);
        await _orchestrator.ToggleEngineAsync(true, CancellationToken.None);
        IsEngineActive = true;
    }

    private async Task DeleteCustomPresetAsync(CustomPresetViewModel? preset) {
        if (preset is null) return;
        // NEW method required in IPresetService:
        // Task DeletePresetAsync(Guid id, CancellationToken ct)
        await _presetService.DeletePresetAsync(preset.Id, CancellationToken.None);
        CustomPresets.Remove(preset);
    }

    private async Task RefreshAsync() {
        // Load custom presets
        var presets = await _presetService.GetCustomPresetsAsync(CancellationToken.None);
        CustomPresets.Clear();
        foreach (var p in presets) {
            CustomPresets.Add(new CustomPresetViewModel {
                Id = p.Id,
                Name = p.Name,
                FailureCount = p.PresetFailureDefinitions.Count
            });
        }

        await UpdateStatusAsync();
        await RefreshLogsAsync();
        await RefreshRealisticStatsAsync();
    }

    private async Task PollAsync() {
        await _orchestrator.PollAndTriggerAsync(CancellationToken.None);
        await UpdateStatusAsync();
        await RefreshLogsAsync();
    }

    private async Task UpdateStatusAsync() {
        var status = await _orchestrator.GetConnectionStatusAsync(CancellationToken.None);
        _isSimConnectConnected = status.IsSimConnectConnected;
        _isFenixConnected = status.IsFenixConnected;
        SimConnectStatus = _isSimConnectConnected ? "Connected" : "Disconnected";
        FenixStatus = _isFenixConnected ? "Connected" : "Disconnected";
        CurrentFlightPhase = status.CurrentFlightPhase;

        OnPropertyChanged(nameof(SimConnectDotColor));
        OnPropertyChanged(nameof(FenixDotColor));
        OnPropertyChanged(nameof(SimConnectStatusBrush));
        OnPropertyChanged(nameof(FenixStatusBrush));
        OnPropertyChanged(nameof(CanStartEngine));
        OnPropertyChanged(nameof(ShowConnectionWarning));
    }

    private async Task RefreshLogsAsync() {
        var logs = await _orchestrator.GetRecentFailuresAsync(CancellationToken.None);
        LogEntries.Clear();
        foreach (var item in logs.OrderByDescending(x => x.TriggeredAtUtc).Take(50)) {
            LogEntries.Add($"{item.TriggeredAtUtc:HH:mm:ss}  {item.FlightPhase,-12}  {item.FailureName}");
        }
    }

    private async Task RefreshRealisticStatsAsync() {
        // NEW interface required: IFlightHistoryService
        // Methods needed:
        //   Task<FlightHistoryStatsDto> GetStatsAsync(CancellationToken ct)
        //   FlightHistoryStatsDto contains:
        //     int TotalFlights
        //     double TotalFlightHours
        //     int TotalFailuresTriggered
        //     int Engine1WearPercent  (0-100)
        //     int Engine2WearPercent  (0-100)
        //     int HydraulicsWearPercent (0-100)
        var stats = await _flightHistoryService.GetStatsAsync(CancellationToken.None);
        TotalFlights = stats.TotalFlights;
        TotalFlightHours = stats.TotalFlightHours;
        TotalFailuresTriggered = stats.TotalFailuresTriggered;
        Engine1WearPercent = stats.Engine1WearPercent;
        Engine2WearPercent = stats.Engine2WearPercent;
        HydraulicsWearPercent = stats.HydraulicsWearPercent;
    }
    #endregion

}