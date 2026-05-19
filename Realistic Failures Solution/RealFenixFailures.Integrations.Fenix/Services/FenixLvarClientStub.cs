using RealFenixFailures.Integrations.Fenix.Interfaces;

namespace RealFenixFailures.Integrations.Fenix.Services;

public class FenixLvarClientStub : IFenixLvarClient
{
    private readonly Dictionary<string, double> _memory = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }

    public Task<double?> ReadLVarAsync(string lvarName, CancellationToken cancellationToken)
    {
        if (_memory.TryGetValue(lvarName, out var value))
        {
            return Task.FromResult<double?>(value);
        }

        return Task.FromResult<double?>(null);
    }

    public Task WriteLVarAsync(string lvarName, double value, CancellationToken cancellationToken)
    {
        _memory[lvarName] = value;
        return Task.CompletedTask;
    }
}