using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.DeckTags;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.DeckTags.CreateDeckTag;

public class CreateDeckTagCommandHandler : ICommandHandler<CreateDeckTagCommand, CreateDeckTagResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;
    private readonly IDeckTagWriteRepository _deckTagWriteRepository;

    public CreateDeckTagCommandHandler(
        IDeckReadRepository deckReadRepository,
        IDeckTagReadRepository deckTagReadRepository,
        IDeckTagWriteRepository deckTagWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckTagReadRepository = deckTagReadRepository;
        _deckTagWriteRepository = deckTagWriteRepository;
    }

    public CreateDeckTagCommandHandler(IDeckRepository deckRepository, IDeckTagRepository deckTagRepository)
        : this((IDeckReadRepository)deckRepository, deckTagRepository, deckTagRepository)
    {
    }

    public async Task<CreateDeckTagResponse> HandleAsync(
        CreateDeckTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        var existing = await _deckTagReadRepository.GetByDeckIdAsync(command.DeckId, cancellationToken);
        if (existing.Count >= DeckTag.MaxTagsPerDeck)
            throw new ArgumentException($"A deck cannot have more than {DeckTag.MaxTagsPerDeck} tags.");

        if (existing.Any(t => string.Equals(t.Name, command.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A tag with this name already exists in this deck.");

        var tag = DeckTag.Create(command.DeckId, command.UserId, command.Name);
        await _deckTagWriteRepository.SaveAsync(tag, cancellationToken);

        return new CreateDeckTagResponse(tag.Id.Value, tag.DeckId, tag.Name, tag.CreatedAt);
    }
}
