using Microsoft.Extensions.Options;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Domain.Services;

namespace RealFenixFailures.Infrastructure.Services;

public class FailuresPersistenceService : IFailuresPersistenceService {
    private readonly FailureEngineSettings _settings;
    private readonly IFenixFailureDefinitionRepository _fenixFailuresRepository;
    private readonly IFenixStreamFailuresReaderService _fenixFailuresReader;

    public FailuresPersistenceService(IOptions<FailureEngineSettings> settings, IFenixFailureDefinitionRepository fenixFailureDefinitionRepository, IFenixStreamFailuresReaderService fenixJsonReaderService) {
        _settings = settings.Value;
        _fenixFailuresRepository = fenixFailureDefinitionRepository;
        _fenixFailuresReader = fenixJsonReaderService;
    }

    public async Task LoadInitialFailuresAsync(CancellationToken ct) {
        var hasData = await _fenixFailuresRepository.HasAnyData(ct);
        if (hasData) return;
        using Stream stream = EmbeddedJsonLoader.LoadFromEmbeddedJson(_settings.FailuresJson) ?? throw new ArgumentException("Error al cargar fallas iniciales");
        var failures = await _fenixFailuresReader.ReadAsync(stream, ct);

        if (failures.MajorGroups.Count == 0)
            throw new ArgumentException("Error al cargar fallas iniciales");

        var systems = failures.MajorGroups.Select(
            x => new FenixFailureSystem {
                Name = x.Title,
                ShortName = x.ShortTitle,
                FailureGroups = x.SystemGroups.Select(
                x => new FenixFailureGroup {
                    Name = x.Name,
                    FailureDefinitions = x.Failures.Select(
                    x => new FenixFailureDefinition { FenixFailureId = x.FenixId, Name = x.Description, Severity = FailureSeverity.Minor }
                    ).ToList()
                }).ToList()
            }
        ).ToList();

        await _fenixFailuresRepository.LoadNewFailuresAsync(systems, ct);
    }
}
