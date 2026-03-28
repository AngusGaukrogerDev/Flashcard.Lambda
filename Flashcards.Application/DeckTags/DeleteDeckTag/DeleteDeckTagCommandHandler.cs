using Flashcards.Application.Abstractions.Commands;
using Flashcards.Application.Cards;
using Flashcards.Application.Decks;
using Flashcards.Application.DeckTags;
using Flashcards.Domain.DeckTags;
using Flashcards.Domain.Decks;

namespace Flashcards.Application.DeckTags.DeleteDeckTag;

public class DeleteDeckTagCommandHandler : ICommandHandler<DeleteDeckTagCommand>
{
    private readonly IDeckReadRepository _deckReadRepository;
    private readonly ICardReadRepository _cardReadRepository;
    private readonly ICardWriteRepository _cardWriteRepository;
    private readonly IDeckTagReadRepository _deckTagReadRepository;
    private readonly IDeckTagWriteRepository _deckTagWriteRepository;

    public DeleteDeckTagCommandHandler(
        IDeckReadRepository deckReadRepository,
        ICardReadRepository cardReadRepository,
        ICardWriteRepository cardWriteRepository,
        IDeckTagReadRepository deckTagReadRepository,
        IDeckTagWriteRepository deckTagWriteRepository)
    {
        _deckReadRepository = deckReadRepository;
        _cardReadRepository = cardReadRepository;
        _cardWriteRepository = cardWriteRepository;
        _deckTagReadRepository = deckTagReadRepository;
        _deckTagWriteRepository = deckTagWriteRepository;
    }

    public DeleteDeckTagCommandHandler(
        IDeckRepository deckRepository,
        ICardRepository cardRepository,
        IDeckTagRepository deckTagRepository)
        : this((IDeckReadRepository)deckRepository, cardRepository, cardRepository, deckTagRepository, deckTagRepository)
    {
    }

    public async Task HandleAsync(DeleteDeckTagCommand command, CancellationToken cancellationToken = default)
    {
        var deck = await _deckReadRepository.GetByIdAsync(command.DeckId, cancellationToken)
            ?? throw new DeckNotFoundException(command.DeckId);

        if (deck.UserId.Value != command.UserId)
            throw new UnauthorisedDeckAccessException(command.DeckId);

        var tag = await _deckTagReadRepository.GetByIdAsync(command.TagId, cancellationToken)
            ?? throw new DeckTagNotFoundException(command.TagId);

        if (tag.DeckId != command.DeckId)
            throw new DeckTagNotFoundException(command.TagId);

        await RemoveTagFromAllCardsInDeckAsync(command.DeckId, command.TagId, cancellationToken);
        await _deckTagWriteRepository.DeleteAsync(command.TagId, cancellationToken);
    }

    private async Task RemoveTagFromAllCardsInDeckAsync(string deckId, string tagId, CancellationToken cancellationToken)
    {
        string? paginationToken = null;
        do
        {
            var (cards, next) = await _cardReadRepository.GetByDeckIdAsync(
                deckId,
                pageSize: 100,
                paginationToken,
                cancellationToken);

            foreach (var card in cards)
            {
                if (!card.TagIds.Contains(tagId))
                    continue;

                card.RemoveTagId(tagId);
                await _cardWriteRepository.SaveAsync(card, cancellationToken);
            }

            paginationToken = next;
        } while (paginationToken is not null);
    }
}
