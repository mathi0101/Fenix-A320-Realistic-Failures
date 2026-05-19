using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Services;

namespace RealFenixFailures.Integrations.SimConnect;

public static class DependencyInjection
{
    public static IServiceCollection AddSimConnectIntegration(this IServiceCollection services)
    {
        services.AddSingleton<ISimConnectClient, SimConnectClient>();
        services.AddSingleton<IFlightDataProvider, SimConnectFlightDataProvider>();
        return services;
    }
}
