namespace Flashcards.Domain.Users;

public readonly record struct UserId(string Value)
{
    public static UserId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("User ID cannot be empty.", nameof(value));

        return new UserId(value.Trim());
    }

    public override string ToString() => Value;
}
