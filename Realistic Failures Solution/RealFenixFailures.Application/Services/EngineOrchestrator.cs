using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;


/// <summary>
/// Este servicio controlará los 2 modos de fallas
/// - Modo 1: Ejecucion de fallas aleatorias dentro del vuelo (Engine On/Off)
/// - Modo 2: Modo manual de encendido y apagado de fallas segun usuario (Presets)
/// </summary>
public class EngineOrchestrator : IEngineOrchestrator {
    private readonly IFailureEngine _failureEngine;
    private readonly IPresetService _presetService;
    private readonly ITriggeredFailureRepository _triggeredFailureRepository;
    private readonly ISessionService _sessionService;
    private readonly IFlightDataProvider _flightDataProvider;
    private readonly IFenixFailureDispatcher _fenixDispatcher;
    private readonly IFailureEngineSettings _settings;
    private readonly List<FailureTriggerLogDto> _recentLogs = new();

    private int? _activePresetId;
    private PresetDto? _activePreset;
    private FlightSession? _activeSession;
    public bool IsEngineActive { get; private set; }

    public EngineOrchestrator(
        IFailureEngine failureEngine,
        IPresetService presetService,
        ITriggeredFailureRepository triggeredFailureRepository,
        ISessionService sessionService,
        IFlightDataProvider flightDataProvider,
        IFenixFailureDispatcher fenixDispatcher,
        IFailureEngineSettings settings) {
        _failureEngine = failureEngine;
        _presetService = presetService;
        _triggeredFailureRepository = triggeredFailureRepository;
        _sessionService = sessionService;
        _flightDataProvider = flightDataProvider;
        _fenixDispatcher = fenixDispatcher;
        _settings = settings;
    }

    #region Presets Controller

    public async Task SetActivePresetAsync(int presetId, CancellationToken cancellationToken) {
        _activePresetId = presetId;
        _activePreset = await _presetService.GetByIdAsync(presetId, cancellationToken);
        _activeSession = null;
    }

    #endregion

    public async Task ToggleEngineAsync(bool isActive, CancellationToken cancellationToken) {
        IsEngineActive = isActive;

        if (!isActive) {
            _activeSession = null;
            await _fenixDispatcher.ResetAllFailuresAsync(cancellationToken);
            return;
        }

        if (_activePresetId is null) {
            return;
        }

        _activeSession = await _sessionService.StartSessionAsync(_activePresetId.Value, cancellationToken);
    }

    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken cancellationToken) {
        var simConnected = await _flightDataProvider.IsConnectedAsync(cancellationToken);
        var fenixConnected = await _fenixDispatcher.IsConnectedAsync(cancellationToken);
        var phase = simConnected
            ? await _flightDataProvider.GetCurrentFlightPhaseAsync(cancellationToken)
            : FlightPhaseEnum.Unknown;

        return new ConnectionStatusDto(simConnected, fenixConnected, phase);
    }

    public Task<IReadOnlyList<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken cancellationToken) {
        return Task.FromResult<IReadOnlyList<FailureTriggerLogDto>>(_recentLogs.OrderByDescending(x => x.TriggeredAtUtc).Take(100).ToList());
    }

    public async Task PollAndTriggerAsync(CancellationToken cancellationToken) {
        if (!IsEngineActive || _activePresetId is null || _activeSession is null) {
            return;
        }

        if (!await _fenixDispatcher.IsConnectedAsync(cancellationToken)) {
            return;
        }

        _activePreset ??= await _presetService.GetByIdAsync(_activePresetId.Value, cancellationToken);
        if (_activePreset is null) {
            return;
        }

        throw new NotImplementedException();

        //var phase = await _flightDataProvider.GetCurrentFlightPhaseAsync(cancellationToken);
        //var trigger = _failureEngine.TryTriggerFailure(_activePreset, phase, _settings.GlobalProbability, DateTimeOffset.UtcNow);

        //if (trigger is null) {
        //    return;
        //}

        //trigger.FlightSessionId = _activeSession.Id;

        //var failure = _activePreset.PresetFailureDefinitions.FirstOrDefault(x => x.FenixFailure!.Id == trigger.FailureDefinitionId)?.FenixFailure;
        //if (failure is null) {
        //    return;
        //}

        //await _fenixDispatcher.TriggerFailureAsync(failure, cancellationToken);

        //await _triggeredFailureRepository.AddAsync(trigger, cancellationToken);
        //await _triggeredFailureRepository.SaveChangesAsync(cancellationToken);

        //_recentLogs.Add(new FailureTriggerLogDto(trigger.TriggeredAtUtc, failure.Name, phase, _activePreset.Name));
    }


}