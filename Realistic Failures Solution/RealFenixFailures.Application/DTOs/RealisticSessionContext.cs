using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public record RealisticSessionContext(
    RiskLevel RiskLevel,
    UserAircraftDto Aircraft
);
