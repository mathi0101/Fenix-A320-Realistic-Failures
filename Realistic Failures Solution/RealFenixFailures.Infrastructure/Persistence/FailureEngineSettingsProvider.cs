using Microsoft.Extensions.Options;
using RealFenixFailures.Application.Interfaces;

namespace RealFenixFailures.Infrastructure.Persistence;

public class FailureEngineSettingsProvider : IFailureEngineSettings
{
    private readonly FailureEngineSettings _settings;

    public FailureEngineSettingsProvider(IOptions<FailureEngineSettings> options)
    {
        _settings = options.Value;
    }

    public double GlobalProbability
    {
        get => _settings.GlobalProbability;
        set => _settings.GlobalProbability = value;
    }

    public int CheckIntervalSeconds
    {
        get => _settings.CheckIntervalSeconds;
        set => _settings.CheckIntervalSeconds = value;
    }
}