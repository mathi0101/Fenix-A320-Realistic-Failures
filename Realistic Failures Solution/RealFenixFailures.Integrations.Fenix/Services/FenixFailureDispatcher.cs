using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Helpers;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixFailureDispatcher : IFenixFailureDispatcher {
    private readonly ILogger<FenixFailureDispatcher> _logger;
    private readonly IFenixApiFailureService _failureService;
    private readonly IFailureTrigger _failureTrigger;
    private readonly ITriggeredFailureRepository _triggeredFailureRepository;


    public FenixFailureDispatcher(ILogger<FenixFailureDispatcher> logger, IFenixApiFailureService failureService, IFailureTrigger failureTrigger, ITriggeredFailureRepository triggeredFailureRepository) {
        _logger = logger;
        _failureService = failureService;
        _failureTrigger = failureTrigger;
        _triggeredFailureRepository = triggeredFailureRepository;
    }

    public Task<bool> IsConnectedAsync(CancellationToken ct) {
        return _failureService.IsApiAvailableAsync(ct);
    }



    public Task ResetAllFailuresAsync(CancellationToken ct) {
        return _failureService.ResetAllFailuresAsync(ct);
    }

    public async Task ExecutePresetAsync(FailurePreset preset, FlightSession session, CancellationToken ct) {
        var triggered = _failureTrigger.GetTriggeredPresetFailures(preset);
        foreach (var def in triggered) {
            if (def is null) continue;

            try {
                await ExecuteFailureAsync(def, session, ct);

                await _triggeredFailureRepository.AddAsync(new TriggeredFailure {
                    FenixFailureId = def.FenixFailureId,
                    FlightSessionId = session.Id,
                    TriggeredAtUtc = DateTimeOffset.UtcNow
                }, ct);

                _logger.LogInformation("Applied scenario failure: {FailureName}", def.FenixFailure!.Name);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to apply scenario failure: {FailureName}", def.FenixFailure!.Name);
                await ResetAllFailuresAsync(ct);
                break;
            }
        }

        await _triggeredFailureRepository.SaveChangesAsync(ct);
    }
    public async Task ExecuteFailureAsync(PresetFailureDefinition fd, FlightSession session, CancellationToken ct) {
        if (fd is null) return;
        if (string.IsNullOrWhiteSpace(fd.FenixFailureId) || fd.FenixFailure is null) {
            _logger.LogWarning("No Fenix external failure id configured for domain failure {FailureName}", fd.FenixFailure?.Name ?? fd.FenixFailureId);
            return;
        }

        var def = new FenixSaveManualRequest(fd.FenixFailureId, false, GetFailureConditionRequest(fd));



        await _failureService.ArmFailureAsync(def, ct);
    }

    #region Privadas

    private FenixFailureConditionRequest? GetFailureConditionRequest(PresetFailureDefinition fd) {
        if (fd is null) return null;
        var fc = new FenixFailureConditionRequest {
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
