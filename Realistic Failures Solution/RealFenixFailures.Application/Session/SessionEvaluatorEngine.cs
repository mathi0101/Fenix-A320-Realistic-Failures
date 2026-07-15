using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
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
public class SessionEvaluatorEngine : INotifyPropertyChanged {
    #region Engine

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
    private readonly ILogger<SessionEvaluatorEngine> _logger;
    private readonly IFlightSessionService _sessionService;
    private readonly IPresetService _presetService;
    private readonly Random _random = Random.Shared;

    private bool isRunning = false;

    private RealisticSessionState? _state;
    private readonly FlightContextProcessor _processor;

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public RealisticSessionState? State {
        get => _state;
        private set => _state = value;
    }
    #endregion

    #region Constructor
    public SessionEvaluatorEngine(
        ISimulatorConnectionService simulator,
        ILogger<SessionEvaluatorEngine> logger,
        IFlightSessionService sessionService,
        IPresetService presetService) {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionService = sessionService;
        _presetService = presetService;
        _processor = new FlightContextProcessor(_simulator, _logger, _random);
    }
    #endregion

    #region Public API

    public async Task<StartSessionResult> TryStartSession(RealisticSessionContext context, CancellationToken ct) {
        var conn = await _simulator.GetConnectionStatusAsync(ct);
        string msg = string.Empty;
        bool isFullSimConnected = conn.IsSimConnectConnected && conn.IsFenixConnected;
        bool isAircraftParkedAndEnginesOff = conn.CurrentFlightPhase == FlightPhaseEnum.ColdAndDark;
        if (!isFullSimConnected)
            msg = "Simulator connection not detected yet";
        else if (!isAircraftParkedAndEnginesOff)
            msg = "Aircraft must be on gate with all engines off";

        var newSession = await _sessionService.StartNewAsync(context.RiskLevel, context.Aircraft, ct);
        State = new RealisticSessionState(newSession, context.Aircraft) {
            AvailablePresets = (await _presetService.GetRealisticPresetsAsync(ct)).ToList()
        };
        isRunning = true;

        return new StartSessionResult(isFullSimConnected && isAircraftParkedAndEnginesOff, msg);
    }

    public async Task StopSession(CancellationToken ct) {
        if (!isRunning || State == null) return;
        await _sessionService.StopAsync(State!.Session.Id, DateTime.UtcNow, ct);
        State = null;
        isRunning = false;
    }

    /// <summary>
    /// Evaluates the current session state against the simulator once (one timer tick).
    /// Triggers at most a single failure per tick. Returns an <see cref="EvaluationResult"/>
    /// describing what happened so the manager can raise events / pause the session.
    /// </summary>
    public async Task<EvaluationResult> EvaluateAsync(CancellationToken ct) {
        if (!isRunning) return EvaluationResult.Disconnected();
        // 1. Connectivity guard.
        var conn = await _simulator.GetConnectionStatusAsync(ct);
        if (!conn.IsFenixConnected || !conn.IsSimConnectConnected) {
            _logger.LogWarning(
                "Simulator connection unavailable (Fenix={Fenix}, SimConnect={SimConnect}). Pausing evaluation.",
                conn.IsFenixConnected, conn.IsSimConnectConnected);
            return EvaluationResult.Disconnected();
        }

        var rawSimulatorDataResult = await _simulator.GetSimulatorData(ct);
        var fenixFailuresResult = await _simulator.GetCurrentFenixFailures(ct);
        if (!rawSimulatorDataResult.IsSuccess || !fenixFailuresResult.IsSuccess) {
            _logger.LogWarning(
                "Simulator connection unavailable (Fenix={Fenix}, SimConnect={SimConnect}). Pausing evaluation.",
                conn.IsFenixConnected, conn.IsSimConnectConnected);
            return EvaluationResult.Disconnected();
        }

        var rawData = rawSimulatorDataResult.Value!;
        var failures = fenixFailuresResult.Value!;

        State!.FlightPhase = conn.CurrentFlightPhase;

        var snapshot = GetSnapshot(rawData, failures);
        State.ArmedFenixFailures = snapshot.FenixFailures.Where(x => x.IsArmed).ToList();
        State.ActivatedFenixFailures = snapshot.FenixFailures.Where(x => x.Failed).ToList();

        var result = await _processor.ProcessAsync(snapshot, State, ct);




        //// 2. Simultaneous-active-failures guard.
        //if (State.ActiveFailures.Count >= MaxSimultaneousActiveFailures) {
        //    _logger.LogDebug(
        //        "Max simultaneous active failures reached ({Count}/{Max}); skipping tick.",
        //        State.ActiveFailures.Count, MaxSimultaneousActiveFailures);
        //    return EvaluationResult.NoAction();
        //}

        //// 3. Global cooldown guard (cheap check before iterating presets).
        //var timeSinceLastFailure = DateTimeOffset.UtcNow - State.LastFailureTriggeredAtUtc;
        //if (timeSinceLastFailure.TotalSeconds < MinCooldownSeconds) {
        //    _logger.LogDebug(
        //        "Global cooldown active ({Elapsed:F1}s / {Cooldown}s); skipping tick.",
        //        timeSinceLastFailure.TotalSeconds, MinCooldownSeconds);
        //    return EvaluationResult.NoAction();
        //}


        //// 4. Select applicable presets for the current flight phase.
        //var applicablePresets = SelectApplicablePresets(currentPhase);
        //if (applicablePresets.Count == 0) {
        //    _logger.LogDebug("No applicable presets for phase {Phase}.", currentPhase);
        //    return EvaluationResult.NoAction();
        //}

        //// 5. Iterate presets/candidates and try to trigger AT MOST one failure this tick.
        //foreach (var preset in applicablePresets) {
        //    var candidates = FilterCandidateFailures(preset, currentPhase);
        //    foreach (var failure in candidates) {
        //        if (!ShouldTriggerFailure(failure)) {
        //            continue;
        //        }

        //        var triggered = await TriggerFailureAsync(failure, preset, ct);
        //        if (triggered is not null) {
        //            // Only one failure per tick.
        //            return EvaluationResult.Completed(new List<TriggeredFailureInfo> { triggered });
        //        }
        //    }
        //}

        return EvaluationResult.NoAction();
    }

    private SimulatorSnapshot GetSnapshot(SimulatorAircraftState raw, AllFenixFailuresResponseDto fenixFailures) {
        if (State == null) throw new ArgumentNullException();
        return new SimulatorSnapshot(
        FlightPhase: State.FlightPhase,
        Engine1Running: raw.Engine1Running,
        Engine2Running: raw.Engine2Running,
        AltitudeFeet: raw.Altitude,
        GroundSpeedKnots: raw.GroundSpeed,
        VerticalSpeedFpm: raw.VerticalSpeed,
        OnGround: raw.IsOnGround,
        CapturedAt: DateTime.UtcNow,
        fenixFailures.GetFailuresList());
    }


    #endregion

    #endregion

    #region Nested Private Class

    private class FlightContextProcessor {
        // ── Umbrales ────────────────────────────────────────────────
        private const double AltitudeThresholdFeet = 10_000;
        private const double SpeedThresholdKnots = 250;
        private const double ClimbRateFpmThreshold = 500;

        // ── Multiplicadores de probabilidad por evento ───────────────
        private const double MultiplierPhaseChange = 2.0;
        private const double MultiplierEngineChange = 1.8;
        private const double MultiplierAltitudeCross = 1.4;
        private const double MultiplierSpeedCross = 1.3;

        // ── Dependencias (recibidas del outer class) ─────────────────
        private readonly ISimulatorConnectionService _simulator;
        private readonly ILogger _logger;
        private readonly Random _random;

        // ── Estado interno ───────────────────────────────────────────
        private SimulatorSnapshot? _previous;

        // ── Fallas armadas: key = FenixFailureId hash, value = condición de disparo ──
        private readonly Dictionary<string, ArmedFailure> _armedFailures = new();

        // ────────────────────────────────────────────────────────────
        public FlightContextProcessor(
            ISimulatorConnectionService simulator,
            ILogger logger,
            Random random) {
            _simulator = simulator;
            _logger = logger;
            _random = random;
        }

        // ════════════════════════════════════════════════════════════
        //  Punto de entrada único — llamado desde EvaluateAsync
        // ════════════════════════════════════════════════════════════
        public async Task<EvaluationResult> ProcessAsync(
            SimulatorSnapshot current,
            RealisticSessionState state,
            CancellationToken ct) {
            // 1. Detectar qué cambió respecto al tick anterior
            var changes = DetectChanges(current);
            _previous = current;

            if (changes.IsFirstSnapshot)
                return EvaluationResult.NoAction();

            // 2. Resolver fallas armadas que ya cumplen su condición
            var fromArmed = await ResolveArmedFailuresAsync(current, state, ct);

            // 3. Si no hubo cambios relevantes, nada más que hacer
            if (!changes.HasAnyChange)
                return fromArmed.Count > 0
                    ? EvaluationResult.Completed(fromArmed)
                    : EvaluationResult.NoAction();

            // 4. Construir candidatos según el contexto del cambio
            var candidates = BuildCandidates(changes, state);
            if (candidates.Count == 0)
                return EvaluationResult.NoAction();

            // 5. Para cada candidato: armar o ejecutar según probabilidad y tipo de evento
            var triggered = new List<TriggeredFailureInfo>(fromArmed);

            foreach (var (definition, preset, eventType) in candidates) {
                var decision = EvaluateDecision(definition, eventType, changes);

                switch (decision) {
                    case TriggerDecision.ExecuteNow:
                        var result = await ExecuteFailureAsync(definition, preset, state, ct);
                        if (result is not null) {
                            triggered.Add(result);
                            // Un solo execute inmediato por tick
                            goto doneTriggers;
                        }
                        break;

                    case TriggerDecision.Arm:
                        ArmFailure(definition, preset, current, state);
                        break;

                    case TriggerDecision.Skip:
                    default:
                        break;
                }
            }

doneTriggers:
            return triggered.Count > 0
                ? EvaluationResult.Completed(triggered)
                : EvaluationResult.NoAction();
        }

        // ════════════════════════════════════════════════════════════
        //  1. Detección de cambios
        // ════════════════════════════════════════════════════════════
        private StateChangeSet DetectChanges(SimulatorSnapshot current) {
            if (_previous is null)
                return StateChangeSet.FirstSnapshot(current);

            var prev = _previous;

            return new StateChangeSet(
                PreviousPhase: prev.FlightPhase,
                NewPhase: current.FlightPhase,
                FlightPhaseChanged: current.FlightPhase != prev.FlightPhase,
                EngineStateChanged: current.Engine1Running != prev.Engine1Running
                                       || current.Engine2Running != prev.Engine2Running,
                AltitudeCrossedThreshold: CrossedThreshold(prev.AltitudeFeet,
                                              current.AltitudeFeet, AltitudeThresholdFeet),
                SpeedCrossedThreshold: CrossedThreshold(prev.GroundSpeedKnots,
                                              current.GroundSpeedKnots, SpeedThresholdKnots),
                IsFirstSnapshot: false);
        }

        // ════════════════════════════════════════════════════════════
        //  2. Resolver fallas armadas
        // ════════════════════════════════════════════════════════════
        private async Task<List<TriggeredFailureInfo>> ResolveArmedFailuresAsync(
            SimulatorSnapshot current,
            RealisticSessionState state,
            CancellationToken ct) {
            var triggered = new List<TriggeredFailureInfo>();
            var toRemove = new List<string>();

            foreach (var (key, armed) in _armedFailures) {
                if (!armed.ConditionMet(current)) continue;

                _logger.LogDebug(
                    "Armed failure {FailureId} condition met — executing now.",
                    armed.Definition.FenixFailureId);

                var result = await ExecuteFailureAsync(armed.Definition, armed.Preset, state, ct);
                if (result is not null)
                    triggered.Add(result);

                toRemove.Add(key);
            }

            foreach (var key in toRemove)
                _armedFailures.Remove(key);

            return triggered;
        }

        // ════════════════════════════════════════════════════════════
        //  3. Construcción de candidatos
        // ════════════════════════════════════════════════════════════
        private List<(PresetFailureDefinition Definition, FailurePreset Preset, FlightEventType EventType)> BuildCandidates(StateChangeSet changes, RealisticSessionState state) {
            var result = new List<(PresetFailureDefinition, FailurePreset, FlightEventType)>();

            // Determinar qué tipos de evento ocurrieron en este tick
            var activeEvents = GetActiveEvents(changes);

            foreach (var preset in state.AvailablePresets) {
                if (state.ExecutedPresets.Contains(preset)) continue;
                if (!activeEvents.Any(e => PresetMatchesEvent(preset, e, changes))) continue;

                foreach (var definition in preset.PresetFailureDefinitions) {
                    var key = definition.FenixFailureId;
                    if (state.ExecutedFailures.Contains(definition)) continue;
                    if (state.ArmedFenixFailures.Any(x => x.FenixId == key)) continue;
                    if (_armedFailures.ContainsKey(key)) continue;

                    var matchingEvent = activeEvents
                        .FirstOrDefault(e => PresetMatchesEvent(preset, e, changes));

                    result.Add((definition, preset, matchingEvent));
                }
            }

            // Orden no determinístico
            Shuffle(result);
            return result;
        }

        private static List<FlightEventType> GetActiveEvents(StateChangeSet changes) {
            var events = new List<FlightEventType>();
            if (changes.FlightPhaseChanged) events.Add(FlightEventType.PhaseTransition);
            if (changes.EngineStateChanged) events.Add(FlightEventType.EngineStateChange);
            if (changes.AltitudeCrossedThreshold) events.Add(FlightEventType.AltitudeThreshold);
            if (changes.SpeedCrossedThreshold) events.Add(FlightEventType.SpeedThreshold);
            return events;
        }

        private static bool PresetMatchesEvent(
            FailurePreset preset,
            FlightEventType eventType,
            StateChangeSet changes) {
            return eventType switch {
                FlightEventType.PhaseTransition => preset.FlightPhase == changes.NewPhase,
                FlightEventType.EngineStateChange
                    or FlightEventType.AltitudeThreshold
                    or FlightEventType.SpeedThreshold
                    or FlightEventType.ClimbInitiated
                    or FlightEventType.DescentInitiated
                                                 => preset.FlightPhase == changes.NewPhase,
                _ => false
            };
        }

        // ════════════════════════════════════════════════════════════
        //  4. Decisión: ejecutar ahora, armar, o ignorar
        // ════════════════════════════════════════════════════════════
        private TriggerDecision EvaluateDecision(
            PresetFailureDefinition definition,
            FlightEventType eventType,
            StateChangeSet changes) {
            var multiplier = GetMultiplier(changes);
            var probability = Math.Clamp(BaseProbability * multiplier, 0.0, 1.0);
            var roll = _random.NextDouble();

            _logger.LogDebug(
                "Decision roll for {FailureId}: roll={Roll:F4} vs prob={Prob:F4} (×{Mult:F2}) event={Event}",
                definition.FenixFailureId, roll, probability, multiplier, eventType);

            if (roll >= probability)
                return TriggerDecision.Skip;

            // Transiciones de fase → armar para el siguiente momento relevante
            // Cambios inmediatos (motor, altitud) → ejecutar ya
            return eventType == FlightEventType.PhaseTransition
                ? TriggerDecision.Arm
                : TriggerDecision.ExecuteNow;
        }

        private static double GetMultiplier(StateChangeSet changes) {
            double m = 1.0;
            if (changes.FlightPhaseChanged) m *= MultiplierPhaseChange;
            if (changes.EngineStateChanged) m *= MultiplierEngineChange;
            if (changes.AltitudeCrossedThreshold) m *= MultiplierAltitudeCross;
            if (changes.SpeedCrossedThreshold) m *= MultiplierSpeedCross;
            return m;
        }

        // ════════════════════════════════════════════════════════════
        //  5. Armar falla diferida
        // ════════════════════════════════════════════════════════════
        private void ArmFailure(
            PresetFailureDefinition definition,
            FailurePreset preset,
            SimulatorSnapshot current,
            RealisticSessionState state) {
            var key = definition.FenixFailureId;

            // Condición de disparo: cuando la fase cambie de nuevo o al alcanzar cierta altitud
            Func<SimulatorSnapshot, bool> condition = snap =>
                snap.FlightPhase != current.FlightPhase ||
                snap.AltitudeFeet >= current.AltitudeFeet + 2_000;

            _armedFailures[key] = new ArmedFailure(definition, preset, condition);

            _logger.LogInformation(
                "Armed failure {FailureId} ('{Name}') — waiting for trigger condition.",
                definition.FenixFailureId, definition.FenixFailure?.Name ?? "Unknown");
        }

        // ════════════════════════════════════════════════════════════
        //  6. Ejecutar falla en el simulador
        // ════════════════════════════════════════════════════════════
        private async Task<TriggeredFailureInfo?> ExecuteFailureAsync(
            PresetFailureDefinition definition,
            FailurePreset preset,
            RealisticSessionState state,
            CancellationToken ct) {
            var accepted = await _simulator.ExecuteFailureAsync(definition, ct);
            if (!accepted) {
                _logger.LogWarning(
                    "Simulator rejected failure {FailureId} from preset {Preset}.",
                    definition.FenixFailureId, preset.Name);
                return null;
            }

            var now = DateTime.UtcNow;
            var key = definition.FenixFailureId;

            state.ExecutedFailures.Add(definition);
            if (!state.ExecutedPresets.Contains(preset))
                state.ExecutedPresets.Add(preset);

            var description = definition.FenixFailure?.Name ?? "Unknown failure";

            _logger.LogInformation(
                "Executed failure {FailureId} ('{Description}') from preset {Preset} at {At:o}.",
                definition.FenixFailureId, description, preset.Name, now);

            return new TriggeredFailureInfo(
                FenixFailureId: definition.FenixFailureId,
                Description: description,
                PresetName: preset.Name,
                TriggeredAtUtc: now);
        }

        // ════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════
        private static bool CrossedThreshold(double prev, double current, double threshold) =>
            (prev < threshold && current >= threshold) ||
            (prev >= threshold && current < threshold);

        private void Shuffle<T>(IList<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ── Tipos auxiliares internos ────────────────────────────────
        private sealed record ArmedFailure(
            PresetFailureDefinition Definition,
            FailurePreset Preset,
            Func<SimulatorSnapshot, bool> ConditionMet);

        private enum TriggerDecision { Skip, ExecuteNow, Arm }
    }
    private enum FlightEventType {
        PhaseTransition,
        EngineStateChange,
        AltitudeThreshold,
        SpeedThreshold,
        ClimbInitiated,
        DescentInitiated
    }
    // Dentro de SessionProcessorEngine o en un archivo propio si crece
    private sealed record SimulatorSnapshot(
        FlightPhaseEnum FlightPhase,
        bool Engine1Running,
        bool Engine2Running,
        double AltitudeFeet,
        double GroundSpeedKnots,
        double VerticalSpeedFpm,
        bool OnGround,
        DateTime CapturedAt,
        IReadOnlyList<FenixFailureDto> FenixFailures
    );
    private sealed record StateChangeSet(
        FlightPhaseEnum? PreviousPhase,
        FlightPhaseEnum? NewPhase,
        bool FlightPhaseChanged,
        bool EngineStateChanged,
        bool AltitudeCrossedThreshold,
        bool SpeedCrossedThreshold,
        bool IsFirstSnapshot
    ) {
        public bool HasAnyChange =>
            FlightPhaseChanged || EngineStateChanged ||
            AltitudeCrossedThreshold || SpeedCrossedThreshold;

        internal static StateChangeSet FirstSnapshot(SimulatorSnapshot current) {
            return new StateChangeSet(null, current.FlightPhase, true, true, false, false, true);
        }
    }
    #endregion
}
