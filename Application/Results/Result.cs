namespace Application.Results;

public sealed class Result<TValue>
{
    public bool IsSuccess { get; }

    private readonly TValue? _value;
    private readonly string? _errorMessage;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public string ErrorMessage => !IsSuccess
        ? _errorMessage!
        : throw new InvalidOperationException("Cannot access ErrorMessage on a successful Result.");

    private Result(bool isSuccess, TValue? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        _value = value;
        _errorMessage = errorMessage;
    }

    public static Result<TValue> Success(TValue value) => new(true, value, default);

    public static Result<TValue> Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);
        return new(false, default, errorMessage);
    }
}
