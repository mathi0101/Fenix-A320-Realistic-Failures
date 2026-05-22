using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Integrations.Fenix.Interfaces;
using RealFenixFailures.Integrations.Fenix.Mappers;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixApiFailureService : IFenixApiFailureService {
    private readonly IFenixApiClient _apiClient;
    private readonly IOptionsMonitor<FenixApiOptions> _optionsMonitor;
    private readonly ILogger<FenixApiFailureService> _logger;
    private readonly SemaphoreSlim _healthLock = new(1, 1);

    private DateTimeOffset _lastHealthCheckAtUtc = DateTimeOffset.MinValue;
    private bool _lastHealthCheckResult;

    public FenixApiFailureService(IFenixApiClient apiClient, IOptionsMonitor<FenixApiOptions> optionsMonitor, ILogger<FenixApiFailureService> logger) {
        _apiClient = apiClient;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }


    public async Task<AllFenixFailuresResponseDto> GetAllFailuresAsync(CancellationToken cancellationToken) {
        var response = await _apiClient.GetManualFailuresAsync(cancellationToken);
        if (response is null) {
            return new AllFenixFailuresResponseDto();
        }

        UpdateHealthState(true);

        return FenixMappers.FenixJsonFailuresToDto(response);
    }



    public Task SetFailureAsync(string failureId, bool failed, CancellationToken cancellationToken) {
        return _apiClient.SetManualFailureAsync(failureId, failed, cancellationToken);
    }

    public async Task ResetAllFailuresAsync(CancellationToken cancellationToken) {
        var failures = await GetAllFailuresAsync(cancellationToken);
        var activeFailures = failures.GetAllFailures().Where(f => f.Failed).ToList();

        foreach (var activeFailure in activeFailures) {
            await SetFailureAsync(activeFailure.FenixId, false, cancellationToken);
        }

        if (activeFailures.Count > 0) {
            _logger.LogInformation("Reset {Count} active Fenix failures.", activeFailures.Count);
        }
    }

    public async Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken) {
        var intervalSeconds = Math.Max(1, _optionsMonitor.CurrentValue.HealthCheckIntervalSeconds);
        var now = DateTimeOffset.UtcNow;

        if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
            return _lastHealthCheckResult;
        }

        await _healthLock.WaitAsync(cancellationToken);
        try {
            now = DateTimeOffset.UtcNow;
            if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
                return _lastHealthCheckResult;
            }

            var response = await _apiClient.GetManualFailuresAsync(cancellationToken);
            UpdateHealthState(response is not null);
            return _lastHealthCheckResult;
        } finally {
            _healthLock.Release();
        }
    }

    private void UpdateHealthState(bool isAvailable) {
        _lastHealthCheckResult = isAvailable;
        _lastHealthCheckAtUtc = DateTimeOffset.UtcNow;
    }

}
