using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Interfaces;
using RealFenixFailures.Integrations.SimConnect.Services;

namespace RealFenixFailures.Integrations.SimConnect;

public static class DependencyInjection {
    public static IServiceCollection AddSimConnectIntegration(this IServiceCollection services) {
#if DEBUG
        services.AddSingleton<ISimConnectClient, MockSimConnectClient>();
#else
        services.AddSingleton<ISimConnectClient, SimConnectClient>();
#endif
        services.AddSingleton<IFlightDataProvider, SimConnectFlightDataProvider>();
        return services;
    }
}
