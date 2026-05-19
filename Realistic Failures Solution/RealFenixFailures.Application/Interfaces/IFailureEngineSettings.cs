namespace RealFenixFailures.Application.Interfaces;

public interface IFailureEngineSettings
{
    double GlobalProbability { get; set; }
    int CheckIntervalSeconds { get; set; }
}