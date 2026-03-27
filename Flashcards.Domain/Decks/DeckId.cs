namespace Flashcards.Domain.Decks;

public record DeckId(Guid Value)
{
    public static DeckId New() => new(Guid.NewGuid());

    public static DeckId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
