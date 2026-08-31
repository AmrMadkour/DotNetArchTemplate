using Application.Constants;

namespace Application.Results;

public sealed class Result<TValue>
{
    public bool IsSuccess { get; }

    private readonly TValue? _value;
    private readonly string? _errorMessage;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(ValidationMessages.CannotAccessValueOnFailedResult);

    public string ErrorMessage => !IsSuccess
        ? _errorMessage!
        : throw new InvalidOperationException(ValidationMessages.CannotAccessErrorMessageOnSuccessfulResult);

    private Result(bool isSuccess, TValue? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        _value = value;
        _errorMessage = errorMessage;
    }

    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(true, value, default);
    }

    public static Result<TValue> Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);
        return new(false, default, errorMessage);
    }
}
