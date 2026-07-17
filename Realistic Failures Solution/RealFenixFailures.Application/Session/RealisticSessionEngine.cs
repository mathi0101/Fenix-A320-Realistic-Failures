using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.DTOs;
using System.ComponentModel;

namespace RealFenixFailures.Application.Session;

/// <summary>
/// Encapsulates all realistic-failure evaluation logic for a single active session.
/// A new instance is created by <c>RealisticSessionManager</c> when a session starts and is
/// discarded when the session stops. The engine owns the failure-selection, probability and
/// state-mutation logic; the manager only owns the session lifecycle, the timer and event dispatch.
/// </summary>
/// <remarks>
/// Not sealed so it can be subclassed/mocked in tests.
/// </remarks>
public class RealisticSessionEngine : INotifyPropertyChanged, IDisposable {

    #region Constants
    /// <summary>Base probability that a given candidate failure is triggered on a tick where it is evaluated.</summary>
    private const double BaseProbability = 0.10;

    /// <summary>Minimum time (seconds) that must elapse since the last triggered failure before another can fire.</summary>
    private const int MinCooldownSeconds = 60;

    /// <summary>Maximum number of failures that may be active simultaneously in a session.</summary>
    private const int MaxSimultaneousActiveFailures = 3;
    #endregion

    #region Fields
    private readonly ISimulatorConnectionService _simulator;
    private readonly ILogger<RealisticSessionEngine> _logger;
    private readonly IFlightSessionService _sessionService;
    private readonly IPresetService _presetService;
    private readonly Random _random = Random.Shared;


    private RealisticSession? _session;

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public RealisticSession? Session {
        get => _session;
        private set => _session = value;
    }
    public bool IsRunning => Session != null;

    #endregion

    #region Constructor
    public RealisticSessionEngine(
        ISimulatorConnectionService simulator,
        ILogger<RealisticSessionEngine> logger,
        IFlightSessionService sessionService,
        IPresetService presetService) {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionService = sessionService;
        _presetService = presetService;
    }
    #endregion

    #region Public API

    public async Task<StartSessionResult> TryStartSession(RealisticModeContext context, CancellationToken ct) {
        var conn = await _simulator.GetConnectionStatusAsync(ct);
        var data = await _simulator.GetSimulatorData(ct);
        string msg = string.Empty;
        bool isFullSimConnected = conn.IsSimConnectConnected && conn.IsFenixConnected && data.IsSuccess;
        bool isAircraftParkedAndEnginesOff = data.Value!.IsOnGround && !(data.Value!.Engine1IsRunning || data.Value!.Engine2IsRunning);
        if (!isFullSimConnected)
            msg = "Simulator connection not detected yet";
        else if (!isAircraftParkedAndEnginesOff)
            msg = "Aircraft must be on gate with all engines off";

        var newSession = await _sessionService.StartNewAsync(context.RiskLevel, context.Aircraft, ct);
        Session = new RealisticSession(newSession, context.Aircraft) {
            AvailablePresets = (await _presetService.GetRealisticPresetsAsync(ct)).ToList()
        };

        return new StartSessionResult(isFullSimConnected && isAircraftParkedAndEnginesOff, msg);
    }

    public async Task StopSession(CancellationToken ct) {
        if (!IsRunning) return;
        await _sessionService.StopAsync(Session!.Session.Id, DateTime.UtcNow, ct);
        Session = null;
    }

    /// <summary>
    /// Evaluates the current session state against the simulator once (one timer tick).
    /// Triggers at most a single failure per tick. Returns an <see cref="EvaluationResult"/>
    /// describing what happened so the manager can raise events / pause the session.
    /// </summary>
    public async Task<EvaluationResult> EvaluateAsync(CancellationToken ct) {
        if (!IsRunning) return EvaluationResult.Disconnected();
        try {
            (SimulatorAircraftStateSnapshot rawData, IReadOnlyList<FenixFailureDto> failures) = await GetSimulatorData(ct);

            Session!.ProcessSimData(rawData, failures);

        } catch (ExecutionEngineException ex) {
            _logger.LogWarning("Error al recibir datos en EvaluateAsync, Ex: {a}", ex.Message);
            return EvaluationResult.Disconnected();
        } catch (Exception ex) {
            _logger.LogCritical("Error fatal en EvaluateAsync. Finalizando sesion. Ex: {a}", ex.Message);
            await StopSession(CancellationToken.None);
            throw;
        }

        return EvaluationResult.NoAction();
    }




    #endregion

    #region Privates
    private async Task<(SimulatorAircraftStateSnapshot rawData, IReadOnlyList<FenixFailureDto> failures)> GetSimulatorData(CancellationToken ct) {
        const string msgSimConn = "Simulator connection unavailable (Fenix={Fenix}, SimConnect={SimConnect}). Pausing evaluation.";
        const string msgSimData = "Simulator data corrupted (Fenix={Fenix}, SimConnect={SimConnect}). Pausing evaluation.";
        var conn = await _simulator.GetConnectionStatusAsync(ct);
        if (!conn.IsFenixConnected || !conn.IsSimConnectConnected) {
            _logger.LogWarning(
                msgSimConn,
                conn.IsFenixConnected, conn.IsSimConnectConnected);
            throw new ExecutionEngineException(msgSimConn);
        }

        var rawSimulatorDataResult = await _simulator.GetSimulatorData(ct);
        var fenixFailuresResult = await _simulator.GetCurrentFenixFailures(ct);
        if (!rawSimulatorDataResult.IsSuccess || !fenixFailuresResult.IsSuccess) {
            _logger.LogWarning(
                msgSimData,
                fenixFailuresResult.IsSuccess, rawSimulatorDataResult.IsSuccess);
            throw new ExecutionEngineException(msgSimData);
        }

        var rawData = rawSimulatorDataResult.Value!;
        var failures = fenixFailuresResult.Value!.GetFailuresList();
        return (rawData, failures);
    }

    #endregion

    public async void Dispose() {
        if (IsRunning) {
            await StopSession(CancellationToken.None);
        }
    }


}
