using Amazon.Lambda.Core;

namespace Flashcards.Functions;

public class CreateDeckFunction
{
    public string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}