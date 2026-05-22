using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;

public class SessionService : ISessionService {
    private readonly IFlightSessionRepository _sessionRepository;

    public SessionService(IFlightSessionRepository sessionRepository) {
        _sessionRepository = sessionRepository;
    }

    public async Task<FlightSession> StartSessionAsync(int presetId, CancellationToken cancellationToken) {
        var session = new FlightSession {
            StartedAtUtc = DateTimeOffset.UtcNow,
            PresetId = presetId
        };

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        return session;
    }
}
