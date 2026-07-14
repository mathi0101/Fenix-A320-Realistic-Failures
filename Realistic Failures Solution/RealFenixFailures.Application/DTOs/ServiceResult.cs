namespace RealFenixFailures.Application.DTOs;

public sealed class ServiceResult<T> {
    public T? Value { get; }
    public Exception? Error { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => Error is null;

    private ServiceResult(T value) => Value = value;
    private ServiceResult(Exception error) => Error = error;
    private ServiceResult(Exception error, string? msg) {
        Error = error;
        ErrorMessage = msg;
    }

    public static ServiceResult<T> Ok(T value) => new(value);
    public static ServiceResult<T> Fail(Exception error, string? errorMsg = null) => new(error, errorMsg);
}
