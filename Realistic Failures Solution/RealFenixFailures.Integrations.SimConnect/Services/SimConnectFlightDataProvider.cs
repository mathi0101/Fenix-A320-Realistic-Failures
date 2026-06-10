using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Models;

namespace RealFenixFailures.Integrations.SimConnect.Services;

public class SimConnectFlightDataProvider : IFlightDataProvider {
    private readonly ISimConnectClient _client;

    public SimConnectFlightDataProvider(ISimConnectClient client) {
        _client = client;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken) {
        return _client.IsConnectedAsync(cancellationToken);
    }

    public async Task<FlightPhaseEnum> GetCurrentFlightPhaseAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);
        return DetermineFlightPhase(state);
    }

    private FlightPhaseEnum DetermineFlightPhase(SimAircraftState state) {
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

    public async Task<double> GetCurrentAltitudeAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);
        return state.Altitude;
    }

    public async Task<double> GetCurrentAirspeedAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);
        return state.AirspeedTrue;
    }

    public async Task<bool> IsOnGroundAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);
        return state.IsOnGround;
    }

    public async Task<SimAircraftContext> GetAircraftContextAsync(CancellationToken cancellationToken) {
        var state = await _client.GetAircraftStateAsync(cancellationToken);

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
}