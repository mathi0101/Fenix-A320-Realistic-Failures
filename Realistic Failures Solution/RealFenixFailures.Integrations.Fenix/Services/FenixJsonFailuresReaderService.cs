using Microsoft.Extensions.Options;
using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Integrations.Fenix.Mappers;
using RealFenixFailures.Integrations.Fenix.Models;
using System.Text.Json;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixJsonFailuresReaderService : IFenixJsonFailuresReaderService {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IOptions<FenixApiOptions> _optionsMonitor;

    public FenixJsonFailuresReaderService(IOptions<FenixApiOptions> options) {
        _optionsMonitor = options;
    }


    public async Task<AllFenixFailuresResponseDto> ReadAsync(CancellationToken cancellationToken = default) {
        var filePath = Path.Combine(AppContext.BaseDirectory, _optionsMonitor.Value.FailuresJson);

        if (!File.Exists(filePath)) {
            throw new FileNotFoundException("No encontrado", filePath);
        }

        await using var stream = File.OpenRead(filePath);

        var payload = await JsonSerializer.DeserializeAsync<FenixManualFailuresResponse>(
            stream,
            JsonOptions,
            cancellationToken);

        if (payload is null) {
            return new AllFenixFailuresResponseDto();
        }

        return FenixMappers.FenixJsonFailuresToDto(payload);
    }


}
