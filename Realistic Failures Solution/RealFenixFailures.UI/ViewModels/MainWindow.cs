using global::RealFenixFailures.Application.Interfaces;
using global::RealFenixFailures.Domain.Enums;
using global::RealFenixFailures.UI.Commands;
using global::RealFenixFailures.UI.ViewModels.Base;
using Microsoft.Extensions.Logging;
using RealFenixFailures.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace RealFenixFailures.UI.ViewModels;

// ─── DTOs de UI ────────────────────────────────────────────────────────────────

public class TrainingScenarioViewModel : ObservableObject {
    private bool _isSelected;

    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Phase { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public string TriggerDescription { get; init; } = string.Empty;

    public bool IsSelected {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public class CustomPresetViewModel : ObservableObject {
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int FailureCount { get; init; }
}

// ─── MainViewModel ─────────────────────────────────────────────────────────────

public class MainWindow : ObservableObject {
    private readonly IFailureOrchestrator _orchestrator;
    private readonly IPresetService _presetService;
    private readonly IFlightHistoryService _flightHistoryService;   // NEW
    private readonly IFailureEngineSettings _settings;
    private readonly ILogger<DebugViewModel> _logger;
    private readonly DispatcherTimer _timer;

    // Connection state
    private string _simConnectStatus = "Disconnected";
    private string _fenixStatus = "Disconnected";
    private FlightPhase _currentFlightPhase = FlightPhase.Unknown;
    private bool _isSimConnectConnected;
    private bool _isFenixConnected;

    // Engine state
    private bool _isEngineActive;

    // Mode selection
    private bool _isTrainingModeSelected = true;
    private bool _isCustomModeSelected;
    private bool _isRealisticModeSelected;

    // Training
    private TrainingScenarioViewModel? _selectedTrainingScenario;

    // Custom
    private double _globalProbability;
    private int _checkIntervalSeconds;

    // Realistic stats
    private int _totalFlights;
    private double _totalFlightHours;
    private int _totalFailuresTriggered;
    private int _engine1WearPercent;
    private int _engine2WearPercent;
    private int _hydraulicsWearPercent;

    public MainWindow(
        IFailureOrchestrator orchestrator,
        IPresetService presetService,
        IFlightHistoryService flightHistoryService,
        IFailureEngineSettings settings,
        ILogger<DebugViewModel> logger) {
        _orchestrator = orchestrator;
        _presetService = presetService;
        _flightHistoryService = flightHistoryService;
        _settings = settings;
        _logger = logger;

        _globalProbability = settings.GlobalProbability;
        _checkIntervalSeconds = settings.CheckIntervalSeconds;

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

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_checkIntervalSeconds) };
        _timer.Tick += async (_, _) => await PollAsync();
        _timer.Start();

        LoadTrainingScenarios();
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

    public FlightPhase CurrentFlightPhase {
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

    public async Task InitializeAsync() {
        await RefreshAsync();
    }

    // ── Private methods ───────────────────────────────────────────────────────

    private void LoadTrainingScenarios() {
        // These are hardcoded UI scenarios; the actual preset IDs are resolved
        // by the orchestrator when a scenario is started.
        TrainingScenarios.Clear();
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Engine Failure Before V1",
            Description = "Falla de motor antes de V1. Requiere rejected takeoff.",
            Phase = "TAKEOFF",
            Difficulty = "HARD",
            TriggerDescription = "Se dispara antes de alcanzar V1. Procedimiento: RTO."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Engine Failure After V1",
            Description = "Falla de motor durante el roll de despegue, después de alcanzar V1.",
            Phase = "TAKEOFF",
            Difficulty = "MEDIUM",
            TriggerDescription = "Se dispara automáticamente al detectar V1 durante el despegue."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Engine Failure After V2",
            Description = "Falla de motor luego de pasar V2 en ascenso inicial.",
            Phase = "CLIMB",
            Difficulty = "MEDIUM",
            TriggerDescription = "Se dispara al superar V2 en el ascenso inicial."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Hydraulic System Failure",
            Description = "Pérdida del sistema hidráulico azul en crucero.",
            Phase = "CRUISE",
            Difficulty = "MEDIUM",
            TriggerDescription = "Se dispara aleatoriamente durante la fase de crucero."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Dual Bleed Failure",
            Description = "Falla de sangrado en ambos motores. Pérdida de presurización.",
            Phase = "CRUISE",
            Difficulty = "HARD",
            TriggerDescription = "Se dispara en crucero. Requiere descenso de emergencia."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "GPWS Warning on Approach",
            Description = "Activación de GPWS durante la aproximación final.",
            Phase = "APPROACH",
            Difficulty = "EASY",
            TriggerDescription = "Se dispara en la aproximación final por debajo de 1000ft AGL."
        });
        TrainingScenarios.Add(new TrainingScenarioViewModel {
            Id = Guid.NewGuid(),
            Name = "Gear Not Down on Final",
            Description = "Tren de aterrizaje no baja correctamente en la final.",
            Phase = "APPROACH",
            Difficulty = "HARD",
            TriggerDescription = "Se dispara al seleccionar gear down en la aproximación."
        });
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

        // NEW method required in IFailureOrchestrator:
        // Task StartTrainingScenarioAsync(Guid scenarioId, CancellationToken ct)
        await _orchestrator.StartTrainingScenarioAsync(SelectedTrainingScenario.Id, CancellationToken.None);
        IsEngineActive = true;
        _logger.LogInformation("Training scenario started: {Name}", SelectedTrainingScenario.Name);
    }

    private async Task ToggleEngineAsync() {
        var nextState = !IsEngineActive;
        await _orchestrator.ToggleEngineAsync(nextState, CancellationToken.None);
        IsEngineActive = nextState;
        _logger.LogInformation("Engine toggled to {State}", nextState);
    }

    private void ApplySettings() {
        _settings.GlobalProbability = Math.Clamp(GlobalProbability, 0, 1);
        _settings.CheckIntervalSeconds = Math.Max(5, CheckIntervalSeconds);
        _timer.Interval = TimeSpan.FromSeconds(_settings.CheckIntervalSeconds);
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
        var presets = await _presetService.GetPresetsAsync(CancellationToken.None);
        CustomPresets.Clear();
        foreach (var p in presets.Where(p => p.PresetType == PresetType.Custom)) {
            CustomPresets.Add(new CustomPresetViewModel {
                Id = p.Id,
                Name = p.Name,
                FailureCount = p.FailureCount
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
}