using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.Interfaces;

public interface ISessionService {
    Task<FlightSession> StartSessionAsync(RiskLevel risk, int userAircraftId, CancellationToken cancellationToken);
}
