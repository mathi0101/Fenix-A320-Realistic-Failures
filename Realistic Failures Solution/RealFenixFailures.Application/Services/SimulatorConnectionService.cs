using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Helpers;
using RealFenixFailures.Domain.Interfaces;

namespace RealFenixFailures.Application.Services;

public class SimulatorConnectionService : ISimulatorConnectionService {
    private readonly ILogger<SimulatorConnectionService> _logger;
    private readonly ISimFlightDataProvider _simFlightDataProvider;
    private readonly IFenixFailureApiDispatcher _fenixFailureApiDispatcher;
    private readonly IFailureTrigger _failureTrigger;

    public SimulatorConnectionService(ILogger<SimulatorConnectionService> logger, IFenixFailureApiDispatcher failureService, IFailureTrigger failureTrigger, ISimFlightDataProvider flightDataProvider) {
        _logger = logger;
        _fenixFailureApiDispatcher = failureService;
        _failureTrigger = failureTrigger;
        _simFlightDataProvider = flightDataProvider;
    }

    #region Connection Status
    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(CancellationToken ct) {
        var simConnected = await _simFlightDataProvider.IsConnectedAsync(ct);
        var fenixConnected = await _fenixFailureApiDispatcher.IsApiAvailableAsync(ct);
        var rawData = await _simFlightDataProvider.GetAircraftRawData(ct);

        var phase = simConnected
            ? DetermineFlightPhase(rawData)
            : FlightPhaseEnum.Unknown;

        return new ConnectionStatusDto(simConnected, fenixConnected, phase);
    }
    #endregion

    #region FlightData
    public async Task<ServiceResult<SimulatorAircraftState>> GetSimulatorData(CancellationToken ct) {
        try {
            return ServiceResult<SimulatorAircraftState>.Ok(await _simFlightDataProvider.GetAircraftRawData(ct));
        } catch (Exception ex) {
            _logger.LogError("GetSimulatorData Error: {a}", ex.Message);
            return ServiceResult<SimulatorAircraftState>.Fail(ex);
        }
    }
    #endregion

    #region Failures Handler

    public async Task<ServiceResult<AllFenixFailuresResponseDto>> GetCurrentFenixFailures(CancellationToken ct) {
        try {
            return ServiceResult<AllFenixFailuresResponseDto>.Ok(await _fenixFailureApiDispatcher.GetAllFailuresAsync(ct));
        } catch (Exception ex) {
            _logger.LogError("GetCurrentFenixFailures Error: {a}", ex.Message);
            return ServiceResult<AllFenixFailuresResponseDto>.Fail(ex);
        }
    }

    public async Task<bool> ResetAllFailuresAsync(CancellationToken ct) {
        return await _fenixFailureApiDispatcher.ResetAllFailuresAsync(ct);
    }

    public async Task<ServiceResult<IReadOnlyList<PresetFailureDefinition>>> ExecutePresetAsync(FailurePreset preset, CancellationToken ct) {
        List<PresetFailureDefinition> response = [];
        var triggered = _failureTrigger.GetTriggeredPresetFailures(preset);
        foreach (var def in triggered) {
            if (def is null) continue;

            try {
                var isArmed = await ExecuteFailureAsync(def, ct);
                if (!isArmed) continue;
                response.Add(def);

            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to apply scenario failure: {FailureName}", def.FenixFailure!.Name);
                await ResetAllFailuresAsync(ct);
                return ServiceResult<IReadOnlyList<PresetFailureDefinition>>.Fail(ex);
            }
        }

        _logger.LogInformation("Applied preset failure: {PresetName}", preset.Name);
        return ServiceResult<IReadOnlyList<PresetFailureDefinition>>.Ok(response);
    }
    public async Task<bool> ExecuteFailureAsync(PresetFailureDefinition fd, CancellationToken ct) {
        if (fd is null) return false;
        if (string.IsNullOrWhiteSpace(fd.FenixFailureId) || fd.FenixFailure is null) {
            _logger.LogWarning("No Fenix external failure id configured for domain failure {FailureName}", fd.FenixFailure?.Name ?? fd.FenixFailureId);
            return false;
        }

        var def = new FenixArmFailureRequest(fd.FenixFailureId, false, GetFailureConditionRequest(fd));



        return await _fenixFailureApiDispatcher.ArmFailureAsync(def, ct);
    }
    #endregion

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

    private FlightPhaseEnum DetermineFlightPhase(SimulatorAircraftState state) {
        if (state.IsOnGround) {
            if (state.Engine1Running || state.Engine2Running) {
                if (state.GroundSpeed > 5)
                    return FlightPhaseEnum.Taxi;
                else if (state.ThrottlePercent1 > 80 || state.ThrottlePercent2 > 80)
                    return FlightPhaseEnum.Takeoff;
                else
                    return FlightPhaseEnum.Parked;
            } else {
                return FlightPhaseEnum.ColdAndDark;
            }
        } else {
            if (state.Altitude < 10000) {
                if (state.VerticalSpeed > 500)
                    return FlightPhaseEnum.Climb;
                else if (state.VerticalSpeed < -500)
                    return FlightPhaseEnum.Approach;
                else
                    return FlightPhaseEnum.Cruise;
            } else if (state.Altitude >= 10000 && state.Altitude <= 30000) {
                if (Math.Abs(state.VerticalSpeed) < 200)
                    return FlightPhaseEnum.Cruise;
                else if (state.VerticalSpeed > 200)
                    return FlightPhaseEnum.Climb;
                else
                    return FlightPhaseEnum.Descent;
            } else {
                if (state.VerticalSpeed > 200)
                    return FlightPhaseEnum.Climb;
                else if (state.VerticalSpeed < -200)
                    return FlightPhaseEnum.Descent;
                else
                    return FlightPhaseEnum.Cruise;
            }
        }
    }
    #endregion
}
