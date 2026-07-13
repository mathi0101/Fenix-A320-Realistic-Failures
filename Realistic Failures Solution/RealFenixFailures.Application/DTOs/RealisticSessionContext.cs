using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.DTOs;

public record RealisticSessionContext(
    UserAircraft Aircraft,
    FlightSession Session,
    IReadOnlyList<AircraftSystemWear> CurrentSystemWears
);
