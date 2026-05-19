namespace RealFenixFailures.Integrations.Fenix.Interfaces;

public interface IFenixLvarClient
{
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken);
    Task<double?> ReadLVarAsync(string lvarName, CancellationToken cancellationToken);
    Task WriteLVarAsync(string lvarName, double value, CancellationToken cancellationToken);
}
