using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealFenixFailures.Infrastructure.Persistence;
using RealFenixFailures.UI.DependencyInjection;

namespace RealFenixFailures.UI;

public partial class App : System.Windows.Application {
    private readonly IHost _host;

    public App() {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configure => {
                configure.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureRealFenixFailures()
            .Build();
    }

    protected override async void OnStartup(System.Windows.StartupEventArgs e) {
        await _host.StartAsync();

        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RealFenixDbContext>();
        await dbContext.Database.MigrateAsync();

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.Show();
        var newWindow = _host.Services.GetRequiredService<NewWindow>();
        newWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e) {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}