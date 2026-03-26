using Amazon.Lambda.Core;

namespace Flashcards.Functions;

public class UpdateCardFunction
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}