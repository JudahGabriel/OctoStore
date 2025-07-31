namespace OctoStore.Models;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
public struct Option<T>
    where T : class
{
    private readonly T value;

    public Option(T value)
    {
        this.value = value;
    }

    public void Match(Action<T> some, Action? none = null)
    {
        if (value is not null)
        {
            some(value);
        }
        else if (none is not null)
        {
            none();
        }
    }
}
