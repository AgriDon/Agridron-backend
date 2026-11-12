using Agridron.Backend.API.Shared.Domain.Model;

namespace Agridron.Backend.API.Shared.Application.Model;

/// <summary>
///     Represents the outcome of a command or query that can succeed with a value or fail with a
///     domain error.
/// </summary>
/// <typeparam name="T">The type of the successful value.</typeparam>
public class Result<T>
{
    /// <summary>
    ///     Creates a result.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="value">The successful value, if any.</param>
    /// <param name="error">The domain error, if the operation failed.</param>
    protected Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The successful value. Default when the operation failed.</summary>
    public T? Value { get; }

    /// <summary>The domain error. Null when the operation succeeded.</summary>
    public Error? Error { get; }

    /// <summary>The error message, or an empty string when the operation succeeded.</summary>
    public string Message => Error?.Message ?? string.Empty;

    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>Creates a failed result.</summary>
    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(false, default, error);
    }
}

/// <summary>
///     Represents the outcome of a command that has no value on success.
/// </summary>
public class Result : Result<object>
{
    private Result(bool isSuccess, Error? error) : base(isSuccess, null, error)
    {
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>Creates a failed result.</summary>
    public new static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(false, error);
    }
}
