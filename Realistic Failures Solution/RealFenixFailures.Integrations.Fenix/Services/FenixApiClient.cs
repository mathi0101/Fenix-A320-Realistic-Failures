using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealFenixFailures.Integrations.Fenix.Interfaces;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixApiClient : IFenixApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<FenixApiOptions> _optionsMonitor;
    private readonly ILogger<FenixApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FenixApiClient(HttpClient httpClient, IOptionsMonitor<FenixApiOptions> optionsMonitor, ILogger<FenixApiClient> logger)
    {
        _httpClient = httpClient;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task<FenixManualFailuresResponse?> GetManualFailuresAsync(CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = BuildEndpointUri(_optionsMonitor.CurrentValue.ManualFailuresPath);
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fenix GET manual failures returned status code {StatusCode}", response.StatusCode);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<FenixManualFailuresResponse>(stream, JsonOptions, cancellationToken);
            if (payload is null)
            {
                return new FenixManualFailuresResponse(Array.Empty<FenixAtaBlock>());
            }

            return payload.Atas is null
                ? payload with { Atas = Array.Empty<FenixAtaBlock>() }
                : payload;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Unable to connect to Fenix API GET manual failures endpoint.");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout while requesting Fenix API GET manual failures endpoint.");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON received from Fenix GET manual failures endpoint.");
            return null;
        }
    }

    public async Task SetManualFailureAsync(string failureId, bool failed, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = BuildEndpointUri(_optionsMonitor.CurrentValue.SaveManualPath);
            var request = new FenixSaveManualRequest(failureId, failed);
            using var response = await _httpClient.PostAsJsonAsync(endpoint, request, JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fenix POST saveManual failed for {FailureId}. Status code {StatusCode}", failureId, response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Unable to connect to Fenix API POST saveManual endpoint for {FailureId}", failureId);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout while calling Fenix API POST saveManual endpoint for {FailureId}", failureId);
        }
    }

    private Uri BuildEndpointUri(string relativePath)
    {
        var options = _optionsMonitor.CurrentValue;
        var normalizedPath = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        var baseUri = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/", UriKind.Absolute);
        var builder = new UriBuilder(baseUri)
        {
            Port = options.Port,
            Path = normalizedPath
        };

        return builder.Uri;
    }
}
