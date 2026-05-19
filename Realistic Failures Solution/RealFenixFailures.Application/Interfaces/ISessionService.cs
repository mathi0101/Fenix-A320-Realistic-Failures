using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface ISessionService
{
    Task<FlightSession> StartSessionAsync(Guid presetId, CancellationToken cancellationToken);
}
