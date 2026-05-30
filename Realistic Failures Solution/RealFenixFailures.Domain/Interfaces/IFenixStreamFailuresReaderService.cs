using RealFenixFailures.Domain.DTOs;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFenixStreamFailuresReaderService {
    Task<AllFenixFailuresResponseDto> ReadAsync(Stream stream, CancellationToken ct);
}
