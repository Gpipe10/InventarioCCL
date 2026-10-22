using FluentValidation.Results;

namespace Inventario.Application.Common;

public sealed record Error(string Code, string Message,
    IReadOnlyDictionary<string, string[]>? Details = null)
{
    public static Error FromValidation(ValidationResult result) =>
        new("validation", "Revisa los datos enviados",
            result.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
}

public sealed class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
}

public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, long Total);