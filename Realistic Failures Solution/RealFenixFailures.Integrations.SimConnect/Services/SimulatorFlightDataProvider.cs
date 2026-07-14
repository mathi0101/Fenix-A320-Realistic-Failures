using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Interfaces;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimulatorFlightDataProvider : ISimFlightDataProvider {
    private readonly ISimConnectClient _client;
    private readonly ILogger<SimulatorFlightDataProvider> _logger;
    private readonly SemaphoreSlim _healthLock = new(1, 1);

    private DateTimeOffset _lastHealthCheckAtUtc = DateTimeOffset.MinValue;
    private bool _lastHealthCheckResult;

    public SimulatorFlightDataProvider(ISimConnectClient client, ILogger<SimulatorFlightDataProvider> logger) {
        _client = client;
        _logger = logger;
        _logger.LogInformation("SimulatorFlightDataProvider initialized");
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct) {
        var intervalSeconds = 10;
        var now = DateTimeOffset.UtcNow;

        if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
            _logger.LogDebug("Returning cached health check result: {Result}", _lastHealthCheckResult);
            return _lastHealthCheckResult;
        }

        await _healthLock.WaitAsync(ct);
        try {
            now = DateTimeOffset.UtcNow;
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

    public async Task<SimulatorAircraftState> GetAircraftRawData(CancellationToken ct) {
        var state = await _client.GetAircraftStateAsync(ct);
        return new SimulatorAircraftState(state.IsConnected, state.Latitude, state.Longitude, state.Altitude,
            state.Heading, state.GroundSpeed, state.TrueAirspeed, state.VerticalSpeed, state.IsOnGround, state.FlapsHandleIndex,
            state.Engine1Running, state.Engine2Running, state.ThrottlePercent1, state.ThrottlePercent2, state.RadioHeight);
    }



    private void UpdateHealthState(bool isAvailable) {
        _lastHealthCheckResult = isAvailable;
        _lastHealthCheckAtUtc = DateTimeOffset.UtcNow;
    }


}
