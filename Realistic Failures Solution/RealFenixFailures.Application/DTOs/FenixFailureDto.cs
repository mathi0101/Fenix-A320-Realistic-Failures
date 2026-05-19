namespace RealFenixFailures.Application.DTOs;

public sealed record FenixFailureDto(
    string Id,
    bool Failed,
    string? Name
);
