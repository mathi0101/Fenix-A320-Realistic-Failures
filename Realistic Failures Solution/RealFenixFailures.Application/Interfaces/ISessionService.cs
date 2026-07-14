using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Interfaces;

public interface ISessionService {
    Task<FlightSession> StartSessionAsync(RiskLevel risk, UserAircraftDto aircraft, CancellationToken cancellationToken);
}
