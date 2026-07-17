using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Integrations.SimConnect.Interfaces;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimulatorFlightDataProvider : ISimFlightDataProvider {
    private readonly ISimConnectClient _client;
    private readonly ILogger<SimulatorFlightDataProvider> _logger;
    private readonly SemaphoreSlim _healthLock = new(1, 1);

    private DateTime _lastHealthCheckAtUtc = DateTime.MinValue;
    private bool _lastHealthCheckResult;

    public SimulatorFlightDataProvider(ISimConnectClient client, ILogger<SimulatorFlightDataProvider> logger) {
        _client = client;
        _logger = logger;
        _logger.LogInformation("SimulatorFlightDataProvider initialized");
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct) {
        var intervalSeconds = 10;
        var now = DateTime.UtcNow;

        if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
            _logger.LogDebug("Returning cached health check result: {Result}", _lastHealthCheckResult);
            return _lastHealthCheckResult;
        }

        await _healthLock.WaitAsync(ct);
        try {
            now = DateTime.UtcNow;
            if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
                return _lastHealthCheckResult;
            }

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var isAlive = await _client.IsConnectedAsync(cts.Token);
            UpdateHealthState(isAlive);

            if (isAlive != _lastHealthCheckResult) {
                _logger.LogInformation("Connection status changed: {IsConnected}", isAlive);
            }

            return _lastHealthCheckResult;
        } catch (OperationCanceledException) {
            _logger.LogWarning("Health check timed out");
            UpdateHealthState(false);
            return false;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error during health check");
            UpdateHealthState(false);
            return false;
        } finally {
            _healthLock.Release();
        }
    }

    public async Task<SimulatorAircraftStateSnapshot> GetAircraftRawData(CancellationToken ct) {
        var s = await _client.GetAircraftStateAsync(ct);
        return new SimulatorAircraftStateSnapshot() {
            FlightPhase = s.IsOnGround ? SimpleFlightPhaseEnum.OnGround : SimpleFlightPhaseEnum.Flying,
            IsOnGround = s.IsOnGround,
            AltitudeMSL = s.AltitudeMSL,
            IndicatedAltitude = s.IndicatedAltitude,
            GroundSpeed = s.GroundSpeed,
            VerticalSpeed = s.VerticalSpeed,
            Engine1Combustion = s.Engine1Combustion == 1,
            Engine2Combustion = s.Engine2Combustion == 1,
            Engine1N1Percent = s.Engine1N1Percent,
            Engine2N1Percent = s.Engine2N1Percent,
            TrueAirspeed = s.TrueAirspeed,
            FlapsHandleIndex = s.FlapsHandleIndex,
            Heading = s.Heading,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            AltitudeAGL = s.RadioHeight,
            ThrottlePercent1 = s.ThrottlePercent1,
            ThrottlePercent2 = s.ThrottlePercent2,
        };
    }



    private void UpdateHealthState(bool isAvailable) {
        _lastHealthCheckResult = isAvailable;
        _lastHealthCheckAtUtc = DateTime.UtcNow;
    }


}
