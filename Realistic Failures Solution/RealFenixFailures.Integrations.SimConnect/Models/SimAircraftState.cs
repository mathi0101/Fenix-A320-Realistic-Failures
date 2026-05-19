using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Integrations.SimConnect.Models;

public sealed record SimAircraftState(
    bool IsConnected,
    FlightPhase FlightPhase
);
