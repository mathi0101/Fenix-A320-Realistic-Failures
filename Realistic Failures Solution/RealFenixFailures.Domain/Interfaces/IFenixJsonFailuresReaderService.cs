using RealFenixFailures.Domain.DTOs;

namespace RealFenixFailures.Domain.Interfaces;

public interface IFenixJsonFailuresReaderService {
    Task<AllFenixFailuresResponseDto> ReadAsync(CancellationToken cancellationToken = default);
}
