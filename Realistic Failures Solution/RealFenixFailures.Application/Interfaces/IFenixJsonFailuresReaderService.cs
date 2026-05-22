using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixJsonFailuresReaderService {
    Task<AllFenixFailuresResponseDto> ReadAsync(CancellationToken cancellationToken = default);
}
