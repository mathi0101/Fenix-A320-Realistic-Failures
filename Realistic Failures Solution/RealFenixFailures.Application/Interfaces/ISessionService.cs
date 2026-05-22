using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface ISessionService {
    Task<FlightSession> StartSessionAsync(int presetId, CancellationToken cancellationToken);
}
