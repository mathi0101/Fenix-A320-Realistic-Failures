using Microsoft.Extensions.Configuration;
using Serilog;

namespace RealFenixFailures.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static LoggerConfiguration ApplyDefaultConfiguration(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        return loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext();
    }
}
