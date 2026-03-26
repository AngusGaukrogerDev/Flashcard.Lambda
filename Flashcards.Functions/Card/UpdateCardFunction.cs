using Amazon.Lambda.Core;

namespace Flashcards.Functions.Card;

public class UpdateCardFunction
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}