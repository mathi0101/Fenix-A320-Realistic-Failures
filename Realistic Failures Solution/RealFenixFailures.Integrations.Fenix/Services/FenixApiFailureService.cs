using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Integrations.Fenix.Interfaces;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixApiFailureService : IFenixApiFailureService {
    private readonly IFenixApiClient _apiClient;
    private readonly IFenixStreamFailuresReaderService _jsonReader;
    private readonly IOptionsMonitor<FenixApiOptions> _optionsMonitor;
    private readonly ILogger<FenixApiFailureService> _logger;
    private readonly SemaphoreSlim _healthLock = new(1, 1);

    private DateTimeOffset _lastHealthCheckAtUtc = DateTimeOffset.MinValue;
    private bool _lastHealthCheckResult;

    public FenixApiFailureService(IFenixApiClient apiClient, IFenixStreamFailuresReaderService jsonReader, IOptionsMonitor<FenixApiOptions> optionsMonitor, ILogger<FenixApiFailureService> logger) {
        _apiClient = apiClient;
        _jsonReader = jsonReader;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }


    #region Minimal

    public async Task<bool> ArmFailureAsync(FenixSaveManualRequest def, CancellationToken ct) {
        using var stream = await _apiClient.SendFailureAsync(def, ct);
        bool httpOk = stream is not null;
        UpdateHealthState(httpOk);
        return httpOk;

    }

    #endregion

    public async Task<AllFenixFailuresResponseDto> GetAllFailuresAsync(CancellationToken ct) {
        try {
            using (var stream = await _apiClient.GetManualFailuresAsync(ct)) {
                if (stream is null) {
                    UpdateHealthState(false);
                    return new AllFenixFailuresResponseDto();
                }
                var result = await _jsonReader.ReadAsync(stream, ct);

                UpdateHealthState(result.MajorGroups.Count > 0);
                return result;
            }
        } catch (Exception ex) {
            throw;
        }

    }


    public async Task<bool> ResetAllFailuresAsync(CancellationToken ct) {
        var failures = await GetAllFailuresAsync(ct);
        var activeFailures = failures.GetFailuresList().Where(f => f.Failed || f.FailureCondition is not null).ToList();
        if (activeFailures.Count > 0) {
            _logger.LogInformation("Resetting {Count} active Fenix failures.", activeFailures.Count);
            foreach (var activeFailure in activeFailures) {
                await _apiClient.SendFailureAsync(new FenixSaveManualRequest(activeFailure.FenixId, false, null), ct);
            }
        }


        failures = await GetAllFailuresAsync(ct);
        return failures.GetFailuresList().All(f => !f.Failed);
    }

    public async Task<bool> IsApiAvailableAsync(CancellationToken ct) {
        var intervalSeconds = Math.Max(1, _optionsMonitor.CurrentValue.HealthCheckIntervalSeconds);
        var now = DateTimeOffset.UtcNow;

        if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
            return _lastHealthCheckResult;
        }

        await _healthLock.WaitAsync(ct);
        try {
            now = DateTimeOffset.UtcNow;
            if (now - _lastHealthCheckAtUtc < TimeSpan.FromSeconds(intervalSeconds)) {
                return _lastHealthCheckResult;
            }
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.HealthCheckTimeout));
            var isAlive = await _apiClient.IsApiAlive(cts.Token);
            UpdateHealthState(isAlive);
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
