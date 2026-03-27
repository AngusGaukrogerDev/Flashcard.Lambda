namespace Flashcards.Application.Abstractions.Commands;

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
