using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Domain.Interfaces.Repositories;

public interface ITriggeredFailureRepository
{
    Task<TriggeredFailure> AddAsync(TriggeredFailure triggeredFailure, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
