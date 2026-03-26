using Amazon.Lambda.Core;

namespace Flashcards.Functions.Deck;

public class ReviewDeckFunction
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}