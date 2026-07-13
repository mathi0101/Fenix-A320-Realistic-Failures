namespace RealFenixFailures.Application.DTOs;

public sealed class ServiceResult<T> {
    public T? Value { get; }
    public Exception? Error { get; }
    public bool IsSuccess => Error is null;

    private ServiceResult(T value) => Value = value;
    private ServiceResult(Exception error) => Error = error;

    public static ServiceResult<T> Ok(T value) => new(value);
    public static ServiceResult<T> Fail(Exception error) => new(error);
}
