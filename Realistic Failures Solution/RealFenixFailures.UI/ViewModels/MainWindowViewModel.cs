using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.UI.Commands;
using RealFenixFailures.UI.ViewModels.Base;
using RealFenixFailures.UI.ViewModels.Extra;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RealFenixFailures.UI.ViewModels;

public class MainWindowViewModel : ObservableObject {
    // Representa qué "modo" está corriendo ahora (si corresponde)
    private enum AppMode { None, Training, Custom, Realistic }

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

    // Active mode (representa el modo "ejecutándose")
    private AppMode _activeMode = AppMode.None;

    // Panel selection (qué panel muestra la UI para configurar)
    private bool _isRealisticModeSelected;
    private bool _isTrainingModeSelected = true;
    private bool _isCustomModeSelected;

    // Estado simple de engine (refleja la propiedad pública del orchestrator)
    private bool _isEngineActive;

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

        // Commands with canExecute where it makes sense
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());

        ToggleEngineCommand = new RelayCommand(async () => await ToggleRealisticModeAsync(), () => CanToggleRealistic);
        ToggleRealisticModeCommand = new RelayCommand(async () => await ToggleRealisticModeAsync(), () => CanToggleRealistic);

        ApplySettingsCommand = new RelayCommand(ApplySettings);

        SelectTrainingScenarioCommand = new RelayCommand<TrainingScenarioViewModel>(SelectTrainingScenario);
        StartTrainingScenarioCommand = new RelayCommand(async () => await StartOrStopTrainingScenarioAsync(), () => CanStartTraining);

        CreateCustomPresetCommand = new RelayCommand(async () => await CreateCustomPresetAsync());
        ActivateCustomPresetCommand = new RelayCommand<CustomPresetViewModel>(async p => await ActivateCustomPresetAsync(p),
            p => CanActivateCustomPreset);
        DeleteCustomPresetCommand = new RelayCommand<CustomPresetViewModel>(async p => await DeleteCustomPresetAsync(p),
            p => true);

        // initial update of command states (in case initial flags differ)
        UpdateCommandStates();
    }

    // Collections
    public ObservableCollection<TrainingScenarioViewModel> TrainingScenarios { get; }
    public ObservableCollection<CustomPresetViewModel> CustomPresets { get; }
    public ObservableCollection<string> LogEntries { get; }

    // Commands
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ToggleEngineCommand { get; }
    public RelayCommand ToggleRealisticModeCommand { get; }
    public RelayCommand ApplySettingsCommand { get; }
    public RelayCommand<TrainingScenarioViewModel> SelectTrainingScenarioCommand { get; }
    public RelayCommand StartTrainingScenarioCommand { get; }
    public RelayCommand CreateCustomPresetCommand { get; }
    public RelayCommand<CustomPresetViewModel> ActivateCustomPresetCommand { get; }
    public RelayCommand<CustomPresetViewModel> DeleteCustomPresetCommand { get; }

    // Connection status
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

    // Dot colors (binding antiguo en XAML)
    public Color SimConnectDotColor => _isSimConnectConnected ? Color.FromRgb(34, 197, 94) : Color.FromRgb(100, 116, 139);
    public Color FenixDotColor => _isFenixConnected ? Color.FromRgb(34, 197, 94) : Color.FromRgb(100, 116, 139);

    // Status brushes (puede usarse directamente en XAML)
    public Brush SimConnectStatusBrush => _isSimConnectConnected
        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
        : new SolidColorBrush(Color.FromRgb(100, 116, 139));

    public Brush FenixStatusBrush => _isFenixConnected
        ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
        : new SolidColorBrush(Color.FromRgb(100, 116, 139));

    // Si el orchestrator indica que hay conexión en ambos servicios (puede arrancar "engine" realista)
    public bool CanStartEngine => _isSimConnectConnected && _isFenixConnected;

    public bool ShowConnectionWarning => !CanStartEngine && ActiveMode == AppMode.Realistic;

    // Engine / Mode state (valor reflejado desde VM, pero la lógica corre en IEngineOrchestrator)
    public bool IsEngineActive {
        get => _isEngineActive;
        private set => SetProperty(ref _isEngineActive, value);
    }

    // Control local del modo activo (se actualiza al activar/desactivar via orchestrator)
    private AppMode ActiveMode {
        get => _activeMode;
        set {
            if (_activeMode == value) return;
            _activeMode = value;
            OnPropertyChanged(nameof(IsAnyModeActive));
            OnPropertyChanged(nameof(CanSwitchModes));

            OnPropertyChanged(nameof(IsTrainingActive));
            OnPropertyChanged(nameof(IsCustomActive));
            OnPropertyChanged(nameof(IsRealisticActive));

            OnPropertyChanged(nameof(TrainingStartText));
            OnPropertyChanged(nameof(RealisticToggleText));
            OnPropertyChanged(nameof(CustomModeButtonText));

            OnPropertyChanged(nameof(CanActivateCustomPreset));
            OnPropertyChanged(nameof(CanStartTraining));
            OnPropertyChanged(nameof(CanToggleRealistic));

            OnPropertyChanged(nameof(EngineToggleText));

            IsEngineActive = _activeMode == AppMode.Realistic;

            // Re-evaluar comandos
            UpdateCommandStates();
        }
    }

    public bool IsAnyModeActive => ActiveMode != AppMode.None;
    public bool CanSwitchModes => ActiveMode == AppMode.None;

    public bool IsTrainingActive => ActiveMode == AppMode.Training;
    public bool IsCustomActive => ActiveMode == AppMode.Custom;
    public bool IsRealisticActive => ActiveMode == AppMode.Realistic;

    // Derived properties for button availability (used by canExecute)
    public bool CanStartTraining => SelectedTrainingScenario is not null && (!IsAnyModeActive || IsTrainingActive);
    public bool CanActivateCustomPreset => (!IsAnyModeActive || IsCustomActive);
    public bool CanToggleRealistic => (!IsAnyModeActive || IsRealisticActive) && CanStartEngine;

    public string TrainingStartText => IsTrainingActive ? "⏹ DETENER ESCENARIO" : "▶ INICIAR ESCENARIO";
    public string RealisticToggleText => IsRealisticActive ? "⏹ DETENER MODO REALISTA" : "▶ ACTIVAR MODO REALISTA";
    public string CustomModeButtonText => IsCustomActive ? "⏹ DETENER PRESET" : "▶ ACTIVAR PRESET";


    // Compatibilidad para bindings antiguos
    public string EngineToggleText {
        get {
            if (IsRealisticActive) return RealisticToggleText;
            if (IsTrainingActive) return TrainingStartText;
            return CustomModeButtonText;
        }
    }

    // Panel selection (no cambia la lógica del orchestrator)
    public bool IsTrainingModeSelected {
        get => _isTrainingModeSelected;
        set {
            if (!CanSwitchModes && value && ActiveMode != AppMode.Training) return;
            SetProperty(ref _isTrainingModeSelected, value);
        }
    }

    public bool IsCustomModeSelected {
        get => _isCustomModeSelected;
        set {
            if (!CanSwitchModes && value && ActiveMode != AppMode.Custom) return;
            SetProperty(ref _isCustomModeSelected, value);
        }
    }

    public bool IsRealisticModeSelected {
        get => _isRealisticModeSelected;
        set {
            if (!CanSwitchModes && value && ActiveMode != AppMode.Realistic) return;
            SetProperty(ref _isRealisticModeSelected, value);
        }
    }

    // Training
    public TrainingScenarioViewModel? SelectedTrainingScenario {
        get => _selectedTrainingScenario;
        set {
            if (SetProperty(ref _selectedTrainingScenario, value)) {
                OnPropertyChanged(nameof(HasSelectedTrainingScenario));
                OnPropertyChanged(nameof(CanStartTraining));
                UpdateCommandStates();
            }
        }
    }

    public bool HasSelectedTrainingScenario => SelectedTrainingScenario is not null;

    // Custom settings
    public double GlobalProbability {
        get => _globalProbability;
        set => SetProperty(ref _globalProbability, value);
    }

    public int CheckIntervalSeconds {
        get => _checkIntervalSeconds;
        set => SetProperty(ref _checkIntervalSeconds, value);
    }

    // Realistic stats
    public int TotalFlights { get => _totalFlights; set => SetProperty(ref _totalFlights, value); }
    public double TotalFlightHours { get => _totalFlightHours; set => SetProperty(ref _totalFlightHours, value); }
    public int TotalFailuresTriggered { get => _totalFailuresTriggered; set => SetProperty(ref _totalFailuresTriggered, value); }

    public int Engine1WearPercent {
        get => _engine1WearPercent;
        set {
            if (SetProperty(ref _engine1WearPercent, value)) {
                OnPropertyChanged(nameof(Engine1WearColor));
                OnPropertyChanged(nameof(Engine1WearBrush));
            }
        }
    }

    public int Engine2WearPercent {
        get => _engine2WearPercent;
        set {
            if (SetProperty(ref _engine2WearPercent, value)) {
                OnPropertyChanged(nameof(Engine2WearColor));
                OnPropertyChanged(nameof(Engine2WearBrush));
            }
        }
    }

    public int HydraulicsWearPercent {
        get => _hydraulicsWearPercent;
        set {
            if (SetProperty(ref _hydraulicsWearPercent, value)) {
                OnPropertyChanged(nameof(HydraulicsWearColor));
                OnPropertyChanged(nameof(HydraulicsWearBrush));
            }
        }
    }

    public Color Engine1WearColor => WearColor(Engine1WearPercent);
    public Color Engine2WearColor => WearColor(Engine2WearPercent);
    public Color HydraulicsWearColor => WearColor(HydraulicsWearPercent);

    // Brush properties kept for compatibility con XAML actual (puedes cambiar a converter + resources)
    public Brush Engine1WearBrush => new SolidColorBrush(Engine1WearColor);
    public Brush Engine2WearBrush => new SolidColorBrush(Engine2WearColor);
    public Brush HydraulicsWearBrush => new SolidColorBrush(HydraulicsWearColor);

    private static Color WearColor(int percent) => percent switch {
        < 40 => Color.FromRgb(34, 197, 94),   // green
        < 70 => Color.FromRgb(245, 158, 11),  // amber
        _ => Color.FromRgb(239, 68, 68)       // red
    };

    // Initialization
    public async Task InitializeAsync() {
        await RefreshAsync();
    }

    // --- Private helpers and commands that delegate to orchestrator (no engine logic here) ---

    private async Task LoadTrainingScenarios() {
        TrainingScenarios.Clear();
        var presets = await _presetService.GetTrainingPresetsAsync(CancellationToken.None);
        if (presets == null || presets.Count == 0) return;
        foreach (var p in presets) {
            TrainingScenarios.Add(new TrainingScenarioViewModel {
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
        foreach (var s in TrainingScenarios) s.IsSelected = false;
        if (scenario is not null) scenario.IsSelected = true;
        SelectedTrainingScenario = scenario;
    }

    private async Task StartOrStopTrainingScenarioAsync() {
        if (SelectedTrainingScenario is null) return;

        if (ActiveMode == AppMode.Training) {
            // Stop training scenario via orchestrator
            await DeactivateCurrentModeAsync();
            _logger.LogInformation("Training scenario stopped: {Name}", SelectedTrainingScenario.Name);
            return;
        }

        if (IsAnyModeActive) return;

        // Activate the preset in orchestrator and toggle engine ON (orchestrator will apply the scenario)
        await _orchestrator.ActivatePresetAsync(SelectedTrainingScenario.Id, CancellationToken.None);

        ActiveMode = AppMode.Training;
        _logger.LogInformation("Training scenario started: {Name}", SelectedTrainingScenario.Name);

        // Refresh UI state from orchestrator
        await UpdateStatusAsync();
        await RefreshLogsAsync();
    }

    private async Task ActivateCustomPresetAsync(CustomPresetViewModel? preset) {
        if (preset is null) return;

        if (ActiveMode == AppMode.Custom) {
            await DeactivateCurrentModeAsync();
            _logger.LogInformation("Custom preset stopped: {Name}", preset.Name);
            return;
        }

        if (IsAnyModeActive) return;

        await _orchestrator.ActivatePresetAsync(preset.Id, CancellationToken.None);
        await _orchestrator.ToggleEngineAsync(true, CancellationToken.None);

        ActiveMode = AppMode.Custom;
        _logger.LogInformation("Custom preset triggered: {Name}", preset.Name);

        await UpdateStatusAsync();
        await RefreshLogsAsync();
    }

    private async Task DeleteCustomPresetAsync(CustomPresetViewModel? preset) {
        if (preset is null) return;
        await _presetService.DeletePresetAsync(preset.Id, CancellationToken.None);
        CustomPresets.Remove(preset);
    }

    // Realistic mode toggle: VM delegates entirely to orchestrator.
    private async Task ToggleRealisticModeAsync() {
        if (ActiveMode == AppMode.Realistic) {
            await DeactivateCurrentModeAsync();
            return;
        }

        if (IsAnyModeActive) return;

        // Try to get realistic preset(s) from the preset service (adapt if your service differs)
        var realisticPresets = await _presetService.GetRealisticPresetsAsync(CancellationToken.None); // returns list
        var realisticPreset = realisticPresets?.FirstOrDefault();
        if (realisticPreset is null) {
            _logger.LogWarning("No realistic preset found. Please provide a Realistic-mode preset in presets.");
            return;
        }

        await _orchestrator.ActivatePresetAsync(realisticPreset.Id, CancellationToken.None);
        await _orchestrator.ToggleEngineAsync(true, CancellationToken.None);

        ActiveMode = AppMode.Realistic;
        _logger.LogInformation("Realistic mode activated (orchestrator handles polling/triggers).");

        await UpdateStatusAsync();
        await RefreshLogsAsync();
        await RefreshRealisticStatsAsync();
    }

    // Centralized stop for current active mode
    private async Task DeactivateCurrentModeAsync() {
        if (!IsAnyModeActive) return;

        // Deactivate preset in orchestrator (cleans state)
        await _orchestrator.DeactivatePresetAsync(CancellationToken.None);

        ActiveMode = AppMode.None;

        await UpdateStatusAsync();
        await RefreshLogsAsync();
        await RefreshRealisticStatsAsync();
    }

    private void ApplySettings() {
        GlobalProbability = Math.Clamp(GlobalProbability, 0, 1);
        CheckIntervalSeconds = Math.Max(1, CheckIntervalSeconds);

        // Si querés que el orchestrator actualice el intervalo ahora, descomenta:
        // _orchestrator.SetPollingInterval(TimeSpan.FromSeconds(CheckIntervalSeconds));
    }

    private async Task CreateCustomPresetAsync() {
        var newPreset = await _presetService.CreateEmptyCustomPresetAsync(CancellationToken.None); // implement if missing
        CustomPresets.Add(new CustomPresetViewModel {
            Id = newPreset.Id,
            Name = newPreset.Name,
            FailureCount = newPreset.PresetFailureDefinitions.Count,
        });
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
        await LoadTrainingScenarios();
        await UpdateStatusAsync();
        await RefreshLogsAsync();
        await RefreshRealisticStatsAsync();
    }

    private async Task UpdateStatusAsync() {
        var status = await _orchestrator.GetConnectionStatusAsync(CancellationToken.None);
        _isSimConnectConnected = status.IsSimConnectConnected;
        _isFenixConnected = status.IsFenixConnected;
        SimConnectStatus = _isSimConnectConnected ? "Connected" : "Disconnected";
        FenixStatus = _isFenixConnected ? "Connected" : "Disconnected";
        CurrentFlightPhase = status.CurrentFlightPhase;

        // reflect engine state from orchestrator
        IsEngineActive = _orchestrator.IsEngineActive;

        // notify color/brush bindings
        OnPropertyChanged(nameof(SimConnectDotColor));
        OnPropertyChanged(nameof(FenixDotColor));
        OnPropertyChanged(nameof(SimConnectStatusBrush));
        OnPropertyChanged(nameof(FenixStatusBrush));
        OnPropertyChanged(nameof(CanStartEngine));
        OnPropertyChanged(nameof(ShowConnectionWarning));
        OnPropertyChanged(nameof(EngineToggleText));
        OnPropertyChanged(nameof(IsEngineActive));

        // Re-evaluar comandos al cambiar estado de conexión/engine
        UpdateCommandStates();
    }

    private async Task RefreshLogsAsync() {
        var logs = await _orchestrator.GetRecentFailuresAsync(CancellationToken.None);
        LogEntries.Clear();
        foreach (var item in logs?.OrderByDescending(x => x.TriggeredAtUtc).Take(50) ?? Enumerable.Empty<FailureTriggerLogDto>()) {
            // Assumes DTO has these properties; adapt formatting to actual DTO
            LogEntries.Add($"{item.TriggeredAtUtc:HH:mm:ss}  {item.FlightPhase,-12}  {item.FailureName}");
        }
    }

    private async Task RefreshRealisticStatsAsync() {
        var stats = await _flightHistoryService.GetStatsAsync(CancellationToken.None);
        if (stats is not null) {
            TotalFlights = stats.TotalFlights;
            TotalFlightHours = stats.TotalFlightHours;
            TotalFailuresTriggered = stats.TotalFailuresTriggered;
            Engine1WearPercent = stats.Engine1WearPercent;
            Engine2WearPercent = stats.Engine2WearPercent;
            HydraulicsWearPercent = stats.HydraulicsWearPercent;
        }
    }

    // ----- Helpers -----
    private void UpdateCommandStates() {
        // Asumo que tu RelayCommand tiene RaiseCanExecuteChanged(); si tu implementación tiene otro nombre, ajustalo.
        try {
            RefreshCommand?.RaiseCanExecuteChanged();
            ToggleEngineCommand?.RaiseCanExecuteChanged();
            ToggleRealisticModeCommand?.RaiseCanExecuteChanged();
            StartTrainingScenarioCommand?.RaiseCanExecuteChanged();
            ActivateCustomPresetCommand?.RaiseCanExecuteChanged();
            CreateCustomPresetCommand?.RaiseCanExecuteChanged();
            DeleteCustomPresetCommand?.RaiseCanExecuteChanged();
            ApplySettingsCommand?.RaiseCanExecuteChanged();
        } catch {
            // Si tu RelayCommand no expone RaiseCanExecuteChanged, simplemente no hacemos nada.
            // Alternativa: enlazar IsEnabled en XAML a las props CanStartTraining/CanToggleRealistic/...
        }
    }
}