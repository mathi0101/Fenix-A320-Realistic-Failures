using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixFailureDispatcher
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    Task TriggerFailureAsync(FenixFailureDefinition failureDefinition, CancellationToken cancellationToken);
    Task ResetAllFailuresAsync(CancellationToken cancellationToken);
}
