namespace Application.Results;

public sealed class Result<TValue, TError>
{
    public bool IsSuccess { get; }

    private readonly TValue? _value;
    private readonly TError? _error;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public TError Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    private Result(bool isSuccess, TValue? value, TError? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<TValue, TError> Success(TValue value) => new(true, value, default);

    public static Result<TValue, TError> Failure(TError error) => new(false, default, error);
}
