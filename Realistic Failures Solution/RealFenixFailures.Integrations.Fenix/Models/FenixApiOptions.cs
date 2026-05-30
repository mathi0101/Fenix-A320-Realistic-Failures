namespace RealFenixFailures.Integrations.Fenix.Models;

public class FenixApiOptions {
    public const string SectionName = "FenixApi";

    public string BaseUrl { get; set; } = "http://localhost";
    public int Port { get; set; } = 8083;
    public string ManualFailuresPath { get; set; } = "/fenix/failures/manual";
    public string SaveManualPath { get; set; } = "/fenix/failures/saveManual";
    public int HealthCheckIntervalSeconds { get; set; } = 10;
    public int HealthCheckTimeout { get; set; } = 2;
}
