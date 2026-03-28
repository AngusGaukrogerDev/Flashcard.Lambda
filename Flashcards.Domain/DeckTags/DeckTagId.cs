namespace Flashcards.Domain.DeckTags;

public record DeckTagId(Guid Value)
{
    public static DeckTagId New() => new(Guid.NewGuid());

    public static DeckTagId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
