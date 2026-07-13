using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;

public class SessionService : ISessionService {
    private readonly IFlightSessionRepository _sessionRepository;

    public SessionService(IFlightSessionRepository sessionRepository) {
        _sessionRepository = sessionRepository;
    }

    public async Task<FlightSession> StartSessionAsync(RiskLevel risk, int userAircraftId, CancellationToken cancellationToken) {
        var session = new FlightSession {
            StartedAtUtc = DateTimeOffset.UtcNow,
            RiskLevel = (int)risk,
            UserAircraftId = userAircraftId,
        };

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        return session;
    }
}
