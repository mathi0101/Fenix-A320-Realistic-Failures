namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixAtaBlock(
    int Id,
    string Title,
    string ShortTitle,
    IReadOnlyList<FenixFailureGroup> Groups
);
