using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Helpers;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;

public class SimulatorConnectionService : ISimulatorConnectionService {
    private readonly ILogger<SimulatorConnectionService> _logger;
    private readonly ISimFlightDataProvider _simFlightDataProvider;
    private readonly IFenixFailureApiDispatcher _fenixFailureApiDispatcher;
    private readonly IFailureTrigger _failureTrigger;
    private readonly ITriggeredFailureRepository _triggeredFailureRepository;

    public SimulatorConnectionService(ILogger<SimulatorConnectionService> logger, IFenixFailureApiDispatcher failureService, IFailureTrigger failureTrigger, ITriggeredFailureRepository triggeredFailureRepository, ISimFlightDataProvider flightDataProvider) {
        _logger = logger;
        _fenixFailureApiDispatcher = failureService;
        _failureTrigger = failureTrigger;
        _triggeredFailureRepository = triggeredFailureRepository;
        _simFlightDataProvider = flightDataProvider;
    }

    public async Task<(bool isSimConnected, bool isFenixRunning)> IsConnectedAsync(CancellationToken ct) {
        var fenixRunning = await _fenixFailureApiDispatcher.IsApiAvailableAsync(ct);
        var simConnected = await _simFlightDataProvider.IsConnectedAsync(ct);
        return (simConnected, fenixRunning);
    }

    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken ct) {
        var simConnected = await _simFlightDataProvider.IsConnectedAsync(ct);
        var fenixConnected = await _fenixFailureApiDispatcher.IsApiAvailableAsync(ct);
        var phase = simConnected
            ? await _simFlightDataProvider.GetCurrentFlightPhaseAsync(ct)
            : FlightPhaseEnum.Unknown;

        return new ConnectionStatusDto(simConnected, fenixConnected, phase);
    }

    public Task<bool> ResetAllFailuresAsync(CancellationToken ct) {
        return _fenixFailureApiDispatcher.ResetAllFailuresAsync(ct);
    }

    public async Task<ServiceResult<IReadOnlyList<PresetFailureDefinition>>> ExecutePresetAsync(FailurePreset preset, FlightSession session, CancellationToken ct) {
        List<PresetFailureDefinition> response = [];
        var triggered = _failureTrigger.GetTriggeredPresetFailures(preset);
        foreach (var def in triggered) {
            if (def is null) continue;

            try {
                var isArmed = await ExecuteFailureAsync(def, session, ct);
                if (!isArmed) continue;
                response.Add(def);
                await _triggeredFailureRepository.AddAsync(new TriggeredFailure {
                    FenixFailureId = def.FenixFailureId,
                    FlightSessionId = session.Id,
                    TriggeredAtUtc = DateTimeOffset.UtcNow
                }, ct);

            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to apply scenario failure: {FailureName}", def.FenixFailure!.Name);
                await ResetAllFailuresAsync(ct);
                return ServiceResult<IReadOnlyList<PresetFailureDefinition>>.Fail(ex);
            }
        }

        await _triggeredFailureRepository.SaveChangesAsync(ct);
        _logger.LogInformation("Applied preset failure: {PresetName}", preset.Name);
        return ServiceResult<IReadOnlyList<PresetFailureDefinition>>.Ok(response);
    }
    public async Task<bool> ExecuteFailureAsync(PresetFailureDefinition fd, FlightSession session, CancellationToken ct) {
        if (fd is null) return false;
        if (string.IsNullOrWhiteSpace(fd.FenixFailureId) || fd.FenixFailure is null) {
            _logger.LogWarning("No Fenix external failure id configured for domain failure {FailureName}", fd.FenixFailure?.Name ?? fd.FenixFailureId);
            return false;
        }

        var def = new FenixArmFailureRequest(fd.FenixFailureId, false, GetFailureConditionRequest(fd));



        return await _fenixFailureApiDispatcher.ArmFailureAsync(def, ct);
    }

    #region Privadas

    private FenixArmFailureConditionRequest? GetFailureConditionRequest(PresetFailureDefinition fd) {
        if (fd is null) return null;
        var fc = new FenixArmFailureConditionRequest {
            Ias = int.TryParse(fd.Ias, out var r1) ? r1 : FenixHelper.Intervalos.GetValorRandomIntervalo(fd.Ias),
            Alt = int.TryParse(fd.Above_Altitude, out var r2) ? r2 : FenixHelper.Intervalos.GetValorRandomIntervalo(fd.Above_Altitude),
            Altb = int.TryParse(fd.Below_Altitude, out var r3) ? r3 : FenixHelper.Intervalos.GetValorRandomIntervalo(fd.Below_Altitude),
            Time = int.TryParse(fd.Time, out var r4) ? r4 : FenixHelper.Intervalos.GetValorRandomIntervalo(fd.Time),
            AfterEvent = fd.AfterEvent,
            AfterEventSeconds = int.TryParse(fd.AfterEventSeconds, out var r5) ? r5 : FenixHelper.Intervalos.GetValorRandomIntervalo(fd.AfterEventSeconds)
        };
        return fc;
    }
    #endregion
}
