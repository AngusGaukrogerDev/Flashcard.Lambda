using Amazon.Lambda.Core;

namespace Flashcards.Functions;

public class AddCardToDeckFunction
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}