namespace Flashcards.Infrastructure.Decks;

internal class DeckDocument
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = default!;
}
