using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixFailureDispatcher : IFenixFailureDispatcher
{
    private readonly IFenixFailureService _failureService;
    private readonly ILogger<FenixFailureDispatcher> _logger;

    public FenixFailureDispatcher(IFenixFailureService failureService, ILogger<FenixFailureDispatcher> logger)
    {
        _failureService = failureService;
        _logger = logger;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken)
    {
        return _failureService.IsApiAvailableAsync(cancellationToken);
    }

    public async Task TriggerFailureAsync(FailureDefinition failureDefinition, CancellationToken cancellationToken)
    {
        var fenixFailureId = failureDefinition.ExternalFailureId;
        if (string.IsNullOrWhiteSpace(fenixFailureId))
        {
            _logger.LogWarning("No Fenix external failure id configured for domain failure {FailureName}", failureDefinition.Name);
            return;
        }

        await _failureService.SetFailureAsync(fenixFailureId, true, cancellationToken);
    }

    public Task ResetAllFailuresAsync(CancellationToken cancellationToken)
    {
        return _failureService.ResetAllFailuresAsync(cancellationToken);
    }
}
