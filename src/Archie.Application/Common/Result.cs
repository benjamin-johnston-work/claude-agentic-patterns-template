namespace Archie.Application.Common;

public class Result<T>
{
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Error = null;
    }

    private Result(string error)
    {
        Value = default;
        IsSuccess = false;
        Error = error;
    }

    public T? Value { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess && Value is not null ? onSuccess(Value) : onFailure(Error ?? "Unknown error");
    }
}

public class Result
{
    private Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);

    public static implicit operator Result(string error) => Failure(error);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error ?? "Unknown error");
    }
}