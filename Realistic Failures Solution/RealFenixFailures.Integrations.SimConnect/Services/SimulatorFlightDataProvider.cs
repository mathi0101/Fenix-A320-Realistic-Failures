using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

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
    }

    public async Task<bool> IsConnectedAsync(CancellationToken ct) {
        var intervalSeconds = Math.Max(1, 10);
        var now = DateTimeOffset.UtcNow;

        if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
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
            return _lastHealthCheckResult;
        } finally {
            _healthLock.Release();
        }
    }

    public async Task<FlightPhaseEnum> GetCurrentFlightPhaseAsync(CancellationToken ct) {
        var state = await _client.GetAircraftStateAsync(ct);
        return DetermineFlightPhase(state);
    }

    private FlightPhaseEnum DetermineFlightPhase(SimAircraftState state) {
        _logger.LogDebug("SimAircraftState: {@state}", state);
        // Determinar la fase de vuelo basada en los datos
        if (state.IsOnGround) {
            // En tierra - determinar si es taxi, takeoff o shutdown
            if (state.Engine1Running || state.Engine2Running) {
                // Motores encendidos
                if (state.GroundSpeed > 5) // Velocidad significativa
                    return FlightPhaseEnum.Taxi;
                else if (state.ThrottlePercent1 > 80 || state.ThrottlePercent2 > 80)
                    return FlightPhaseEnum.Takeoff;
                else
                    return FlightPhaseEnum.Parked;
            } else {
                // Motores apagados
                return FlightPhaseEnum.Parked;
            }
        } else {
            // En vuelo - determinar fase específica
            if (state.Altitude < 10000) // Bajo
            {
                if (state.VerticalSpeed > 500) // Subiendo
                    return FlightPhaseEnum.Climb;
                else if (state.VerticalSpeed < -500) // Descendiendo
                    return FlightPhaseEnum.Approach;
                else
                    return FlightPhaseEnum.Cruise;
            } else if (state.Altitude >= 10000 && state.Altitude <= 30000) // Medio
              {
                if (Math.Abs(state.VerticalSpeed) < 200) // Nivelado
                    return FlightPhaseEnum.Cruise;
                else if (state.VerticalSpeed > 200)
                    return FlightPhaseEnum.Climb;
                else
                    return FlightPhaseEnum.Descent;
            } else // Alto
              {
                if (state.VerticalSpeed > 200)
                    return FlightPhaseEnum.Climb;
                else if (state.VerticalSpeed < -200)
                    return FlightPhaseEnum.Descent;
                else
                    return FlightPhaseEnum.Cruise;
            }
        }
    }

    public async Task<SimAircraftContext> GetAircraftContextAsync(CancellationToken ct) {
        var state = await _client.GetAircraftStateAsync(ct);

        return new SimAircraftContext {
            FlightPhase = DetermineFlightPhase(state),
            Altitude = state.Altitude,
            Airspeed = state.AirspeedTrue,
            VerticalSpeed = state.VerticalSpeed,
            IsOnGround = state.IsOnGround,
            Heading = state.Heading,
            Latitude = state.Latitude,
            Longitude = state.Longitude,
            RadioHeight = state.RadioHeight,
            EnginesRunning = (state.Engine1Running ? 1 : 0) + (state.Engine2Running ? 1 : 0)
        };
    }

    private void UpdateHealthState(bool isAvailable) {
        _lastHealthCheckResult = isAvailable;
        _lastHealthCheckAtUtc = DateTimeOffset.UtcNow;
    }
}