using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.DeckTags;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.DeckTags.UpdateDeckTag;

public class UpdateDeckTagCommandHandler : ICommandHandler<UpdateDeckTagCommand, UpdateDeckTagResponse>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;
    private readonly IDeckTagWriteRepository _deckTagWriteRepository;

    public UpdateDeckTagCommandHandler(
        IDeckReadRepository deckReadRepository,
        IDeckTagReadRepository deckTagReadRepository,
        IDeckTagWriteRepository deckTagWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _deckTagReadRepository = deckTagReadRepository;
        _deckTagWriteRepository = deckTagWriteRepository;
    }

    public UpdateDeckTagCommandHandler(IDeckRepository deckRepository, IDeckTagRepository deckTagRepository)
        : this((IDeckReadRepository)deckRepository, deckTagRepository, deckTagRepository)
    {
    }

    public async Task<UpdateDeckTagResponse> HandleAsync(
        UpdateDeckTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        var tag = await _deckTagReadRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new DeckTagNotFoundException(command.TagId);

        if (tag.DeckId != command.DeckId)
            throw new DeckTagNotFoundException(command.TagId);

        var existing = await _deckTagReadRepository.GetByDeckIdAsync(command.DeckId, cancellationToken);
        if (existing.Any(t => t.Id.Value != tag.Id.Value && string.Equals(t.Name, command.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A tag with this name already exists in this deck.");

        tag.Rename(command.Name);
        await _deckTagWriteRepository.SaveAsync(tag, cancellationToken);

        return new UpdateDeckTagResponse(tag.Id.Value, tag.DeckId, tag.Name, tag.CreatedAt);
    }
}
