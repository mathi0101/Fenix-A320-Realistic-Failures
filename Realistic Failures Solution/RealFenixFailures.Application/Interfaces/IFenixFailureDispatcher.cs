using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixFailureDispatcher
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    Task TriggerFailureAsync(FailureDefinition failureDefinition, CancellationToken cancellationToken);
    Task ResetAllFailuresAsync(CancellationToken cancellationToken);
}
