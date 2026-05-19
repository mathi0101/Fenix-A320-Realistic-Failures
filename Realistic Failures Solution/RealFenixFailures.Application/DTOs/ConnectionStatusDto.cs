using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public sealed record ConnectionStatusDto(
    bool IsSimConnectConnected,
    bool IsFenixConnected,
    FlightPhase CurrentFlightPhase
);
