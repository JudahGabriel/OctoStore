using Raven.Client.Documents.Session.Tokens;

namespace OctoStore.Models;

/// <summary>
/// Represents either a value or an exception, typically an error or error message string.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TException"></typeparam>
public class Either<T, TException>
    where T : class
    where TException : class
{
    private readonly T? value;
    private readonly TException? exception;

    public Either(T value)
    {
        this.value = value;
        this.exception = null;
    }

    public Either(TException exception)
    {
        this.value = null;
        this.exception = exception;
    }

    /// <summary>
    /// Executes the provided action if the value is present, or the exception action if an exception is present.
    /// </summary>
    /// <param name="someAction">The action if the value is present.</param>
    /// <param name="exceptionAction">The action to execute if the value is not present.</param>
    public void Match(Action<T> someAction, Action<TException>? exceptionAction = null)
    {
        if (value is not null)
        {
            someAction(value);
        }
        else if (exceptionAction is not null && exception is not null)
        {
            exceptionAction(exception);
        }
    }

    /// <summary>
    /// Executes the provided action if an exception value is present.
    /// </summary>
    /// <param name="exceptionAction"></param>
    public void MatchException(Action<TException> exceptionAction)
    {
        if (exception is not null)
        {
            exceptionAction(exception);
        }
    }
}