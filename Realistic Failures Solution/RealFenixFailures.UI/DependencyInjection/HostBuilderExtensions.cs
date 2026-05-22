using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealFenixFailures.Application;
using RealFenixFailures.Domain;
using RealFenixFailures.Infrastructure;
using RealFenixFailures.Infrastructure.Logging;
using RealFenixFailures.Integrations.Fenix;
using RealFenixFailures.Integrations.SimConnect;
using RealFenixFailures.UI.ViewModels;
using Serilog;

namespace RealFenixFailures.UI.DependencyInjection;

public static class HostBuilderExtensions {
    public static IHostBuilder ConfigureRealFenixFailures(this IHostBuilder hostBuilder) {
        return hostBuilder
            .UseSerilog((context, services, loggerConfiguration) => {
                loggerConfiguration.ApplyDefaultConfiguration(context.Configuration);
            })
            .ConfigureServices((context, services) => {
                services.AddDomain();
                services.AddApplication();
                services.AddInfrastructure(context.Configuration);
                services.AddSimConnectIntegration();
                services.AddFenixIntegration(context.Configuration);
                services.AddTransient<DebugWindow>();
                services.AddTransient<DebugViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<ViewModels.MainWindowViewModel>();
            });
    }
}